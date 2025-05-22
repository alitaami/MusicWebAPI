using MusicWebAPI.Core;
using MusicWebAPI.Domain.Base;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Interfaces.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Domain.Interfaces.Repositories
{
    public interface IPlayListSongsRepository : IRepository<PlaylistSong>
    {
        Task DeleteSongFromPlayList(Guid songId, Guid playListId, CancellationToken cancellationToken);
        Task AddSongsToPlayList(Guid songId, Guid playListId, CancellationToken cancellationToken);
    }
}
