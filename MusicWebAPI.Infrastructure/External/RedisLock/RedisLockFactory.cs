using MusicWebAPI.Domain.Interfaces.Services.External;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Infrastructure.External.RedisLock
{
    public class RedisLockFactory : IRedisLockFactory
    {
        private readonly IDistributedLockFactory _distributedLockFactory;

        public RedisLockFactory(IDistributedLockFactory distributedLockFactory)
        {
            _distributedLockFactory = distributedLockFactory;
        }

        public Task<IRedLock> CreateLockAsync(string resource, TimeSpan expiry)
        {
            return _distributedLockFactory.CreateLockAsync(resource, expiry);
        }
    }
}
