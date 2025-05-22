using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Interfaces.Repositories;
using MusicWebAPI.Infrastructure.Data.Context;
using MusicWebAPI.Infrastructure.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicWebAPI.Core.Resources;
using MusicWebAPI.Domain.Interfaces.Repositories.Base;
using static MusicWebAPI.Domain.Base.Exceptions.CustomExceptions;

namespace MusicWebAPI.Infrastructure.Data.Repositories
{
    public class PlayListRepository : Repository<Playlist>, IPlayListRepository
    {
        private MusicDbContext _context;
        public PlayListRepository(MusicDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Playlist> CreatePlaylist(Playlist playList, CancellationToken cancellationToken)
        {
            var playlist = await this
                .AddAndGetAsync(new Playlist
                {
                    UserId = playList.UserId,
                    Name = playList.Name,
                    CreatedByUserId = playList.CreatedByUserId,
                },
                   cancellationToken,
                   saveNow: true);
            return playlist;
        }

        public async Task DeletePlayList(Guid playListId, CancellationToken cancellationToken)
        {
            // Check if the playlist exists
            var playList = await GetPlayList(playListId, cancellationToken);

            if (playList == null)
                throw new NotFoundException(Resource.PlaylistNotFound);

            playList.IsDeleted = true;

            await this.UpdateAsync(playList, cancellationToken, saveNow: true);
        }

        public async Task<Playlist> GetPlayList(Guid playListId, CancellationToken cancellationToken)
        {
            return await Table
                  .FirstOrDefaultAsync(x => x.Id == playListId && !x.IsDeleted, cancellationToken);
        }

        public async Task<List<object>> GetUserPlaylist(Guid userId, CancellationToken cancellationToken)
        {
            return
                await Table
                .AsNoTracking()
                .Where(p => p.UserId == userId.ToString() && !p.IsDeleted)
                .Include(p => p.PlaylistSongs)
                .Select(p => (object)new
                {
                    PlayListId = p.Id,
                    Name = p.Name,
                    UserId = p.UserId,
                    CreatedByUserId = p.CreatedByUserId,
                    Songs = p.PlaylistSongs.Select(pp => pp.SongId).ToList()
                })
                .ToListAsync(cancellationToken);
        }
    }
}
