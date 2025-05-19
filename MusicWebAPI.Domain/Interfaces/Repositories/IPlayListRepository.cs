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
    public interface IPlayListRepository : IRepository<Playlist>
    {
        Task<Playlist> CreatePlaylist(Playlist playList, CancellationToken cancellationToken);
        public Task<Playlist> GetPlayList(Guid playListId, CancellationToken cancellationToken);
    }
}
