using MusicWebAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Domain.Interfaces.Services
{
    public interface ISongService
    {
        Task<PaginatedResult<object>> GetSongs(string term, int pageSize, int pageNumber, CancellationToken cancellationToken);
        Task<PaginatedResult<object>> GetPopularSongs(int pageSize, int pageNumber, CancellationToken cancellationToken);
    }
}
