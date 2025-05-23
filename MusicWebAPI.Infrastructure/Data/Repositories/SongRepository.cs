using Microsoft.EntityFrameworkCore;
using MusicWebAPI.Core;
using MusicWebAPI.Core.Utilities;
using MusicWebAPI.Domain.Base;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Interfaces.Repositories;
using MusicWebAPI.Infrastructure.Caching;
using MusicWebAPI.Infrastructure.Caching.Base;
using MusicWebAPI.Infrastructure.Data.Context;
using MusicWebAPI.Infrastructure.Data.Repositories.Base;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MusicWebAPI.Infrastructure.Data.Repositories
{
    public class SongRepository : Repository<Song>, ISongRepository
    {
        private MusicDbContext _context;
        private readonly ICacheService _cacheService;

        public SongRepository(MusicDbContext context, ICacheService cacheService) : base(context)
        {
            _cacheService = cacheService;
            _context = context;
        }
        public async Task<PaginatedResult<object>> GetSongsByTerm(string term, int pageSize, int pageNumber, CancellationToken cancellationToken)
        {
            try
            {
                term = term.Trim().ToLower();

                var query = _context
                    .Songs
                    .Include(x => x.Artist)
                    .Include(x => x.Album)
                    .Include(x => x.Genre)
                    .Where(s =>
                       !s.IsDeleted &&
                        s.SearchVector.Matches(EF.Functions.PhraseToTsQuery("english", term)) ||
                        EF.Functions.ILike(s.Genre.Name, $"%{term}%") ||
                        EF.Functions.ILike(s.Artist.FullName, $"%{term}%") ||
                        EF.Functions.ILike(s.Title, $"%{term}%") ||
                        (s.Album != null && EF.Functions.ILike(s.Album.Title, $"%{term}%")))
                    .Select(s => new
                    {
                        Id = s.Id,
                        Title = s.Title,
                        AudioUrl = s.AudioUrl,
                        AlbumTitle = s.Album.Title,
                        GenreName = s.Genre.Name,
                        ArtistName = s.Artist.FullName,
                        Listens = s.Listens,
                        Rank = s.SearchVector.Rank(EF.Functions.PhraseToTsQuery("english", term)),
                    })
                    .OrderByDescending(s => s.Rank);

                var result = await query.ToPaginatedListAsync(pageNumber, pageSize, cancellationToken: cancellationToken);

                return new PaginatedResult<object>(
                    result.Items.Cast<object>().ToList(),
                    result.TotalCount,
                    result.PageNumber,
                    result.PageSize
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetSongsByTerm] An error occurred while fetching songs: {ex.Message}");
                throw;
            }
        }

        public async Task<PaginatedResult<object>> GetPopularSongs(int pageSize, int pageNumber, CancellationToken cancellationToken)
        {
            try
            {
                var popularSongs = this
                       .TableNoTracking
                       .Where(s => !s.IsDeleted)
                       .OrderByDescending(s => s.Listens)
                       .Select(s => (object)new
                       {
                           Id = s.Id,
                           Title = s.Title,
                           AudioUrl = s.AudioUrl,
                           AlbumTitle = s.Album.Title,
                           GenreName = s.Genre.Name,
                           ArtistName = s.Artist.FullName,
                           Listens = s.Listens,
                           Rank = 0f // just to be mapped in future
                       })
                       .AsQueryable();

                var pagedItems = await popularSongs.ToPaginatedListAsync(pageNumber, pageSize, cancellationToken: cancellationToken);

                return pagedItems;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetPopularSongs] An error occurred while fetching songs: {ex.Message}");
                throw;
            }
        }
    }
}
