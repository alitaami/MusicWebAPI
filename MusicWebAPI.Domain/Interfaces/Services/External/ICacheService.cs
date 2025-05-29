using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Domain.External.Caching
{
    public interface ICacheService
    {
        Task SetAsync<T>(string key, T value, int minutes);
        Task<T?> GetAsync<T>(string key);
        Task RemoveAsync(string key);
    }
}
