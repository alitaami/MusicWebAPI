using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using MusicWebAPI.Application.ViewModels;
using MusicWebAPI.Domain.Interfaces;
using MusicWebAPI.Domain.Interfaces.Repositories.Base;
using MusicWebAPI.Infrastructure.Caching;
using MusicWebAPI.Infrastructure.Caching.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static MusicWebAPI.Application.ViewModels.HomeViewModel;

namespace MusicWebAPI.Application.Services
{
    public class RecommendationService : IRecommendationService
    {
        private const string ModelCacheKey = "recommendation_model";
        private const string ModelDataCacheKey = "recommendation_model_data";
        private const int CacheMinutes = 60 * 24; // 1 day

        private readonly IRepositoryManager _repositoryManager;
        private readonly ICacheService _cacheService;
        private readonly MLContext _mlContext;
        private ITransformer _model;
        private RecommendationModelData _modelData;

        public RecommendationService(IRepositoryManager repositoryManager, ICacheService cacheService)
        {
            _repositoryManager = repositoryManager;
            _cacheService = cacheService;
            _mlContext = new MLContext();
        }

        public async Task<List<object>> RecommendSongsAsync(string userId, int count, CancellationToken cancellationToken)
        {
            await EnsureModelIsReadyAsync(cancellationToken);

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

        public async Task TrainAsync(CancellationToken cancellationToken)
        {
            var playlistData = await _repositoryManager.PlayList
                .Get(cancellationToken, p => !p.IsDeleted)
                .SelectMany(p => p.PlaylistSongs
                    .Where(ps => !ps.IsDeleted)
                    .Select(ps => new
                    {
                        p.UserId,
                        SongId = ps.SongId.ToString(),
                        GenreId = ps.Song.GenreId.ToString()
                    }))
                .ToListAsync(cancellationToken);

            if (!playlistData.Any())
                return;

            var allSongs = await _repositoryManager.Song
                .Get(cancellationToken)
                .Select(s => new { SongId = s.Id.ToString(), GenreId = s.GenreId.ToString() })
                .ToListAsync(cancellationToken);

            var userIds = playlistData.Select(p => p.UserId).Distinct().ToList();
            var songIds = allSongs.Select(s => s.SongId).Distinct().ToList();
            var genreIds = allSongs.Select(g => g.GenreId).Distinct().ToList();

            var userMap = userIds.Select((u, i) => new { u, i = (uint)i }).ToDictionary(x => x.u, x => x.i);
            var songMap = songIds.Select((s, i) => new { s, i = (uint)i }).ToDictionary(x => x.s, x => x.i);
            var genreMap = genreIds.Select((g, i) => new { g, i = (uint)i }).ToDictionary(x => x.g, x => x.i);
            var reverseSongMap = songMap.ToDictionary(x => x.Value, x => x.Key);

            var trainingData = new List<MappedPlaylistSong>();

            foreach (var userId in userIds)
            {
                var userSongs = playlistData.Where(p => p.UserId == userId).ToList();
                var userGenres = userSongs.Select(p => p.GenreId).Distinct().ToList();
                var userSongIds = userSongs.Select(p => p.SongId).ToHashSet();

                // Positive samples
                trainingData.AddRange(userSongs.Select(p => new MappedPlaylistSong
                {
                    UserIndex = userMap[userId],
                    SongIndex = songMap[p.SongId],
                    GenreIndex = genreMap[p.GenreId],
                    Label = true
                }));

                // Negative samples (balance with positives)
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

            await _cacheService.SetAsync(ModelCacheKey, ms.ToArray(), CacheMinutes);
            await _cacheService.SetAsync(ModelDataCacheKey, _modelData, CacheMinutes);
        }

        private async Task EnsureModelIsReadyAsync(CancellationToken cancellationToken)
        {
            if (_model != null && _modelData != null) return;

            try
            {
                var modelBytes = await _cacheService.GetAsync<byte[]>(ModelCacheKey);
                var modelData = await _cacheService.GetAsync<RecommendationModelData>(ModelDataCacheKey);

                if (modelBytes != null && modelData != null)
                {
                    using var ms = new MemoryStream(modelBytes);
                    _model = _mlContext.Model.Load(ms, out _);
                    _modelData = modelData;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error loading model: {ex.Message}");
            }

            if (_model == null || _modelData == null)
                throw new InvalidOperationException("Model not trained.");
        }

        private class RecommendationModelData
        {
            public Dictionary<string, uint> UserIndexMap { get; set; }
            public Dictionary<string, uint> SongIndexMap { get; set; }
            public Dictionary<string, uint> GenreIndexMap { get; set; }
            public Dictionary<uint, string> ReverseSongIndexMap { get; set; }
        }

        private class MappedPlaylistSong
        {
            [KeyType(count: 1000)]
            public uint UserIndex { get; set; }

            [KeyType(count: 10000)]
            public uint SongIndex { get; set; }

            [KeyType(count: 100)]
            public uint GenreIndex { get; set; }

            public bool Label { get; set; }
        }

        private class SongPrediction
        {
            public float Score { get; set; }
        }
    }
}
