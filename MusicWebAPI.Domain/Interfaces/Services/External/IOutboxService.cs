using MusicWebAPI.Infrastructure.Outbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Domain.Interfaces.Services.External
{
    public interface IOutboxService
    {
        Task AddAsync(OutboxMessage message);
        Task<IList<OutboxMessage>> GetUnprocessedAsync(int limit = 100);
        Task MarkProcessedAsync(Guid id);
        Task IncrementAttemptAsync(Guid id, string error);
    } 
}
