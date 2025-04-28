using Microsoft.EntityFrameworkCore;
using MusicWebAPI.Core;
using MusicWebAPI.Core.Utilities;
using MusicWebAPI.Domain.Base;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Interfaces.Repositories;
using MusicWebAPI.Infrastructure.Data.Context;
using MusicWebAPI.Infrastructure.Data.Repositories.Base;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MusicWebAPI.Infrastructure.Data.Repositories
{
    public class SongRepository : Repository<Song>, ISongRepository
    {
        private MusicDbContext _context;
        public SongRepository(MusicDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<PaginatedResult<object>> GetSongsByTerm(string term, int pageSize, int pageNumber, CancellationToken cancellationToken)
        {
            try
            {
                var query = _context
                            .Songs
                            .Include(x => x.Artist)
                            .Include(x => x.Album)
                            .Include(x => x.Genre)
                            .Where(s =>
                                s.SearchVector.Matches(EF.Functions.PhraseToTsQuery("english", term)) ||
                                s.Genre.Name.Contains(term) ||
                                s.Artist.FullName.Contains(term) ||
                                s.Title.Contains(term) ||
                                (s.Album != null && s.Album.Title.Contains(term)))
                            .Select(s => new
                            {
                                Id = s.Id,
                                Title = s.Title,
                                AudioUrl = s.AudioUrl,
                                AlbumTitle = s.Album.Title,
                                GenreName = s.Genre.Name,
                                ArtistName = s.Artist.FullName,
                                Rank = s.SearchVector.Rank(EF.Functions.PhraseToTsQuery("english", term)),
                            })
                            .OrderByDescending(s => s.Rank);

                var result = await query.ToPaginatedListAsync(pageNumber, pageSize, cancellationToken: cancellationToken);

                return new PaginatedResult<object>(
                    result.Items.Cast<object>().ToList(),
                    result.TotalCount,
                    result.PageSize,
                    result.PageNumber
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetSongsByTerm] An error occurred while fetching songs: {ex.Message}");
                throw;
            }
        }
    }
}
