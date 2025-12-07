using RedLockNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Domain.Interfaces.Services.External
{
    public interface IRedisLockFactory
    {
        Task<IRedLock> CreateLockAsync(
            string resource,
            TimeSpan expiry
            );
    }
}
