using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Interfaces.Repositories;
using MusicWebAPI.Infrastructure.Data.Context;
using MusicWebAPI.Infrastructure.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<Playlist> GetPlayList(Guid playListId, CancellationToken cancellationToken)
        {
            return await Table
                  .FirstOrDefaultAsync(x => x.Id == playListId, cancellationToken);
        }

    }
}
