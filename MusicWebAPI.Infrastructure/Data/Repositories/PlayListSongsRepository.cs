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
    public class PlayListSongsRepository : Repository<PlaylistSong>, IPlayListSongsRepository
    {
        private MusicDbContext _context;
        public PlayListSongsRepository(MusicDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task AddSongsToPlayList(Guid songId, Guid playListId, CancellationToken cancellationToken)
        {
            var playlistSong = new PlaylistSong
            {
                SongId = songId,
                PlayListId = playListId
            };
            await this.AddAsync(playlistSong, cancellationToken, saveNow: true);
        }

    }
}
