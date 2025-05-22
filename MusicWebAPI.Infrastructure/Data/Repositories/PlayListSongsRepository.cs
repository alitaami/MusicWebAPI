using Microsoft.EntityFrameworkCore;
using MusicWebAPI.Core.Resources;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Interfaces.Repositories;
using MusicWebAPI.Domain.Interfaces.Repositories.Base;
using MusicWebAPI.Infrastructure.Data.Context;
using MusicWebAPI.Infrastructure.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MusicWebAPI.Domain.Base.Exceptions.CustomExceptions;

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
            // Check if the song already exists in the playlist
            var existingPlaylistSong = await IsSongInPlaylist(songId, playListId, cancellationToken);

            if (!existingPlaylistSong)
            {
                var playlistSong = new PlaylistSong
                {
                    SongId = songId,
                    PlayListId = playListId
                };
                await this.AddAsync(playlistSong, cancellationToken, saveNow: true);
            }
            else
            {
                throw new LogicException(Resource.DuplicateSongInPlaylist);
            }
        }

        public async Task DeleteSongFromPlayList(Guid songId, Guid playListId, CancellationToken cancellationToken)
        {
            var playlistSong = await GetPlaylistSong(songId, playListId, cancellationToken);

            if (playlistSong == null)
                throw new NotFoundException(Resource.PlaylistSongNotFound);

            playlistSong.IsDeleted = true;

            await this.UpdateAsync(playlistSong, cancellationToken, saveNow: true);
        }

        public async Task<PlaylistSong> GetPlaylistSong(Guid songId, Guid playListId, CancellationToken cancellationToken)
        {
            return await Table
                .FirstOrDefaultAsync(x => x.SongId == songId && x.PlayListId == playListId && !x.IsDeleted, cancellationToken);
        }

        private async Task<bool> IsSongInPlaylist(Guid songId, Guid playListId, CancellationToken cancellationToken)
        {
            return await _context.PlaylistSongs
                .AnyAsync(ps => ps.SongId == songId && ps.PlayListId == playListId && !ps.IsDeleted, cancellationToken);
        }

    }
}
