using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using MusicWebAPI.Application.ViewModels;
using MusicWebAPI.Domain.External.Caching;
using MusicWebAPI.Domain.Interfaces.Repositories.Base;
using MusicWebAPI.Domain.Interfaces.Services;
using MusicWebAPI.Infrastructure.Caching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static MusicWebAPI.Application.DTOs.RecommendationDTO;
using static MusicWebAPI.Application.ViewModels.HomeViewModel;

namespace MusicWebAPI.Application.Services
{
    public class RecommendationService : IRecommendationService
    {
        private const string ModelCacheKey = "recommendation_model";
        private const string ModelDataCacheKey = "recommendation_model_data";
        private const int CacheMinutes = 60 * 24; // 1 day
        private const string SpotifySearchUrl = "https://api.spotify.com/v1/search";
        private const string SpotifyTopTracksUrl = "https://api.spotify.com/v1/artists/{0}/top-tracks?market=US";
        private const string SpotifyRelatedArtistsUrl = "https://api.spotify.com/v1/artists/{0}/related-artists";
        private const string SpotifyTokenUrl = "https://accounts.spotify.com/api/token";

        private readonly IRepositoryManager _repositoryManager;
        private readonly ICacheService _cacheService;
        private readonly MLContext _mlContext;
        private ITransformer _model;
        private RecommendationModelData _modelData;
        private readonly HttpClient _httpClient;
        private readonly string _clientSecret;
        private readonly string _clientId;

        public RecommendationService(IRepositoryManager repositoryManager, ICacheService cacheService, IConfiguration configuration, HttpClient httpClient)
        {
            _repositoryManager = repositoryManager;
            _cacheService = cacheService;
            _mlContext = new MLContext();
            _httpClient = httpClient;
            _clientSecret = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET");
            _clientId = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID");
        }

        public async Task<List<object>> RecommendSongs(string userId, int count, CancellationToken cancellationToken)
        {
            await EnsureModelIsReady(cancellationToken);

            if (_model == null || _modelData == null)
                return new List<object>(); // or maybe throw a custom exception

            if (!_modelData.UserIndexMap.TryGetValue(userId, out var userIndex))
                return new List<object>();

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<MappedPlaylistSong, SongPrediction>(_model);
            var scoredSongs = new List<(string SongId, float Score)>();

            var allSongEntities = await _repositoryManager.Song
                .Get(cancellationToken)
                .Select(s => new { s.Id, s.GenreId })
                .ToListAsync(cancellationToken);

            foreach (var song in allSongEntities)
            {
                var songId = song.Id.ToString();

                if (!_modelData.SongIndexMap.TryGetValue(songId, out var songIndex))
                    continue;

                var genreIndex = _modelData.GenreIndexMap.TryGetValue(song.GenreId.ToString(), out var gIndex) ? gIndex : 0;

                var prediction = predictionEngine.Predict(new MappedPlaylistSong
                {
                    UserIndex = userIndex,
                    SongIndex = songIndex,
                    GenreIndex = genreIndex
                });

                scoredSongs.Add((songId, prediction.Score));
            }

            var topSongIds = scoredSongs
                .OrderByDescending(x => x.Score)
                .Take(count)
                .Select(x => x.SongId)
                .ToList();

            var topSongs = await _repositoryManager.Song
                .Get(cancellationToken)
                .Where(s => topSongIds.Contains(s.Id.ToString()))
                .Select(s => (object)new GetSongsViewModel
                {
                    Id = s.Id,
                    Title = s.Title,
                    AlbumTitle = s.Album.Title,
                    ArtistName = s.Artist.FullName,
                    AudioUrl = s.AudioUrl,
                    GenreName = s.Genre.Name,
                    Listens = s.Listens,
                    Rank = 0f
                })
                .ToListAsync(cancellationToken);

            return topSongs;
        }

        public async Task Train(CancellationToken cancellationToken)
        {
            try
            {
                var playlists = await _repositoryManager.PlayList
                    .Get(cancellationToken)
                    .Where(p => !p.IsDeleted)
                    .Include(p => p.PlaylistSongs)
                    .ThenInclude(ps => ps.Song)
                    .Select(p => new
                    {
                        p.UserId,
                        p.Name,
                        Songs = p.PlaylistSongs
                            .Where(ps => !ps.IsDeleted)
                            .Select(ps => new
                            {
                                SongId = ps.SongId.ToString(),
                                GenreId = ps.Song.GenreId.ToString(),
                                SongTitle = ps.Song.Title
                            }).ToList()
                    })
                    .ToListAsync(cancellationToken);

                if (!playlists.Any()) return;

                var allSongs = await _repositoryManager.Song
                    .Get(cancellationToken)
                    .AsNoTracking()
                    .Select(s => new { SongId = s.Id.ToString(), Title = s.Title, GenreId = s.GenreId.ToString() })
                    .ToListAsync(cancellationToken);

                var userIds = playlists.Select(p => p.UserId).Distinct().ToList();
                var songIds = allSongs.Select(s => s.SongId).Distinct().ToList();
                var genreIds = allSongs.Select(g => g.GenreId).Distinct().ToList();

                var userMap = userIds.Select((u, i) => new { u, i = (uint)i }).ToDictionary(x => x.u, x => x.i);
                var songMap = songIds.Select((s, i) => new { s, i = (uint)i }).ToDictionary(x => x.s, x => x.i);
                var genreMap = genreIds.Select((g, i) => new { g, i = (uint)i }).ToDictionary(x => x.g, x => x.i);
                var reverseSongMap = songMap.ToDictionary(x => x.Value, x => x.Key);

                var trainingData = new List<MappedPlaylistSong>();

                foreach (var playlist in playlists)
                {
                    var userId = playlist.UserId;
                    var userSongs = playlist.Songs;
                    var userGenres = userSongs.Select(s => s.GenreId).Distinct().ToList();
                    var userSongIds = userSongs.Select(s => s.SongId).ToHashSet();

                    // Spotify-suggested songs based on song name
                    var relatedSongs = new HashSet<string>();
                    foreach (var song in playlist.Songs.Select(s => s.SongTitle))
                    {
                        var spotifySuggestions = await GetRelatedSongFromSpotify(song);
                        foreach (var suggestion in spotifySuggestions)
                            relatedSongs.Add(suggestion);
                    }

                    foreach (var relatedSongTitle in relatedSongs)
                    {
                        var matchedSong = allSongs.FirstOrDefault(s => s.Title.Contains(relatedSongTitle, StringComparison.OrdinalIgnoreCase));
                        if (matchedSong != null && !userSongIds.Contains(matchedSong.SongId))
                        {
                            trainingData.Add(new MappedPlaylistSong
                            {
                                UserIndex = userMap[userId],
                                SongIndex = songMap[matchedSong.SongId],
                                GenreIndex = genreMap[matchedSong.GenreId],
                                Label = true
                            });
                        }
                    }

                    relatedSongs.Clear();

                    // Positive samples
                    trainingData.AddRange(userSongs.Select(p => new MappedPlaylistSong
                    {
                        UserIndex = userMap[userId],
                        SongIndex = songMap[p.SongId],
                        GenreIndex = genreMap[p.GenreId],
                        Label = true
                    }));

                    // Negative samples
                    var negatives = allSongs
                        .Where(s => userGenres.Contains(s.GenreId) && !userSongIds.Contains(s.SongId))
                        .Take(userSongs.Count);

                    trainingData.AddRange(negatives.Select(s => new MappedPlaylistSong
                    {
                        UserIndex = userMap[userId],
                        SongIndex = songMap[s.SongId],
                        GenreIndex = genreMap[s.GenreId],
                        Label = false
                    }));
                }

                var mlData = _mlContext.Data.LoadFromEnumerable(trainingData);

                var pipeline = _mlContext.Transforms
                    .Conversion.MapValueToKey(nameof(MappedPlaylistSong.UserIndex))
                    .Append(_mlContext.Transforms.Conversion.MapValueToKey(nameof(MappedPlaylistSong.SongIndex)))
                    .Append(_mlContext.Transforms.Conversion.MapValueToKey(nameof(MappedPlaylistSong.GenreIndex)))
                    .Append(_mlContext.Transforms.Categorical.OneHotEncoding(new[]
                    {
                     new InputOutputColumnPair("UserFeatures", nameof(MappedPlaylistSong.UserIndex)),
                     new InputOutputColumnPair("SongFeatures", nameof(MappedPlaylistSong.SongIndex)),
                     new InputOutputColumnPair("GenreFeatures", nameof(MappedPlaylistSong.GenreIndex)),
                    }))
                    .Append(_mlContext.Transforms.Concatenate("Features", "UserFeatures", "SongFeatures", "GenreFeatures"))
                    .Append(_mlContext.BinaryClassification.Trainers.FieldAwareFactorizationMachine(new FieldAwareFactorizationMachineTrainer.Options
                    {
                        LabelColumnName = nameof(MappedPlaylistSong.Label),
                        FeatureColumnName = "Features",
                        NumberOfIterations = 20,
                        LatentDimension = 16,
                    }));

                _model = pipeline.Fit(mlData);

                _modelData = new RecommendationModelData
                {
                    UserIndexMap = userMap,
                    SongIndexMap = songMap,
                    ReverseSongIndexMap = reverseSongMap,
                    GenreIndexMap = genreMap
                };

                using var ms = new MemoryStream();
                _mlContext.Model.Save(_model, mlData.Schema, ms);

                await _cacheService.SetAsync(ModelCacheKey, ms.ToArray(), CacheMinutes, ModelCacheKey);
                await _cacheService.SetAsync(ModelDataCacheKey, _modelData, CacheMinutes, ModelDataCacheKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($" TrainAsync ex :" + ex.Message + ex.StackTrace + ex.InnerException);
            }
        }

        private async Task<List<string?>> GetRelatedSongFromSpotify(string songTitle)
        {
            try
            {
                var accessToken = await GetSpotifyAccessToken();

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                // Step 1: Search the track
                var searchResponse = await client.GetAsync($"{SpotifySearchUrl}?q={Uri.EscapeDataString(songTitle)}&type=track&limit=1");
                if (!searchResponse.IsSuccessStatusCode)
                    return new List<string>();

                var searchJson = await searchResponse.Content.ReadAsStringAsync();
                var trackData = JsonDocument.Parse(searchJson)
                    .RootElement.GetProperty("tracks").GetProperty("items")[0];
                var artistId = trackData.GetProperty("artists")[0].GetProperty("id").GetString();

                // Step 2: Fetch artist’s top tracks
                var topTracksResponse = await client.GetAsync(string.Format(SpotifyTopTracksUrl, artistId));
                if (!topTracksResponse.IsSuccessStatusCode)
                    return new List<string>();

                var topTracksJson = await topTracksResponse.Content.ReadAsStringAsync();
                var topTracks = JsonDocument.Parse(topTracksJson)
                    .RootElement.GetProperty("tracks")
                    .EnumerateArray()
                    .Select(track => track.GetProperty("name").GetString())
                    .ToList();

                // Step 3: Fetch related artists and their top tracks
                var relatedResponse = await client.GetAsync(string.Format(SpotifyRelatedArtistsUrl, artistId));
                if (relatedResponse.IsSuccessStatusCode)
                {
                    var relatedJson = await relatedResponse.Content.ReadAsStringAsync();
                    var relatedArtists = JsonDocument.Parse(relatedJson)
                        .RootElement.GetProperty("artists")
                        .EnumerateArray()
                        .Take(2);

                    foreach (var artist in relatedArtists)
                    {
                        var relatedArtistId = artist.GetProperty("id").GetString();
                        var relatedTopTracksResponse = await client.GetAsync(string.Format(SpotifyTopTracksUrl, relatedArtistId));
                        if (!relatedTopTracksResponse.IsSuccessStatusCode) continue;

                        var relatedTopTracksJson = await relatedTopTracksResponse.Content.ReadAsStringAsync();
                        var relatedTracks = JsonDocument.Parse(relatedTopTracksJson)
                            .RootElement.GetProperty("tracks")
                            .EnumerateArray()
                            .Select(track => track.GetProperty("name").GetString());

                        topTracks.AddRange(relatedTracks);
                    }
                }

                return topTracks?.Distinct()?.Take(10)?.ToList() ?? new List<string?>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private async Task<string> GetSpotifyAccessToken()
        {
            using var client = new HttpClient();

            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {authHeader}");

            var requestBody = new FormUrlEncodedContent(new[]
            {
             new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            var response = await client.PostAsync(SpotifyTokenUrl, requestBody);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to get token: {await response.Content.ReadAsStringAsync()}");

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(jsonResponse).RootElement.GetProperty("access_token").GetString();
        }

        private async Task EnsureModelIsReady(CancellationToken cancellationToken)
        {
            if (_model != null && _modelData != null)
                return;

            try
            {
                var modelBytes = await _cacheService.GetAsync<byte[]>(ModelCacheKey);
                var modelData = await _cacheService.GetAsync<RecommendationModelData>(ModelDataCacheKey);

                if (modelBytes == null || modelData == null)
                {
                    _model = null;
                    _modelData = null;
                    return; // No model trained yet
                }

                using var ms = new MemoryStream(modelBytes);
                _model = _mlContext.Model.Load(ms, out _);
                _modelData = modelData;
            }
            catch
            {
                _model = null;
                _modelData = null;
            }
        }
    }
}
