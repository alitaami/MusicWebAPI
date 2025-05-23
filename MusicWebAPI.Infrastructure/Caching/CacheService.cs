using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MusicWebAPI.Core.Utilities;
using MusicWebAPI.Infrastructure.Caching.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MusicWebAPI.Infrastructure.Caching
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public CacheService(IDistributedCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task SetAsync<T>(string key, T value, int minutes)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(minutes)
                };

                var serialized = JsonSerializer.Serialize(value);

                await _cache.SetStringAsync(key, serialized, options);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in CacheService.SetAsync: {ex}");
                throw;
            }
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var data = await _cache.GetStringAsync(key);

                return data is null ? default : JsonSerializer.Deserialize<T>(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in CacheService.GetAsync: {ex}");
                throw;
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in CacheService.RemoveAsync: {ex}");
                throw;
            }
        }
    }
}
