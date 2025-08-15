using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MusicWebAPI.Core.Utilities;
using MusicWebAPI.Domain.External.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MusicWebAPI.Infrastructure.External.Caching
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public CacheService(IDistributedCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task SetAsync<T>(string key, T value, int minutes, string? prefix = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(minutes)
            };

            var serialized = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, serialized, options);

            // Store reference in a key list for the given prefix
            if (!string.IsNullOrEmpty(prefix))
            {
                var trackingKey = $"CacheKeys:{prefix}";
                var current = await _cache.GetStringAsync(trackingKey);
                var keys = current != null ? JsonSerializer.Deserialize<HashSet<string>>(current)! : new HashSet<string>();

                keys.Add(key);
                await _cache.SetStringAsync(trackingKey, JsonSerializer.Serialize(keys), options);
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

        public async Task RemoveByPrefixAsync(List<string> prefixes)
        {
            foreach (var prefix in prefixes)
            {
                var trackingKey = $"CacheKeys:{prefix}";
                var data = await _cache.GetStringAsync(trackingKey);

                if (data != null)
                {
                    var keys = JsonSerializer.Deserialize<HashSet<string>>(data);

                    if (keys != null)
                    {
                        foreach (var key in keys)
                        {
                            await RemoveAsync(key);
                        }
                    }

                    // Remove tracking key itself
                    await RemoveAsync(trackingKey);
                }
            }
        }
    }
}
