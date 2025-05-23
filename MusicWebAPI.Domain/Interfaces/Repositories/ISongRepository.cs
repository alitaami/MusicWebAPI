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
    public interface ISongRepository : IRepository<Song>
    {
        Task<PaginatedResult<object>> GetSongsByTerm(string term, int pageSize, int pageNumber, CancellationToken cancellationToken);
        Task<PaginatedResult<object>> GetPopularSongs(int pageSize, int pageNumber, CancellationToken cancellationToken);
    }
}
