using Microsoft.EntityFrameworkCore;
using MusicWebAPI.Domain.Interfaces.Services.External;
using MusicWebAPI.Infrastructure.Data.Context;
using MusicWebAPI.Infrastructure.Outbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Infrastructure.External.Outbox
{
    public class OutboxService : IOutboxService
    {
        private readonly MusicDbContext _db;

        public OutboxService(MusicDbContext db)
        {
            _db = db;
        }
        public async Task AddAsync(OutboxMessage message)
        {
            await _db.OutboxMessages.AddAsync(message);
        }

        public async Task<IList<OutboxMessage>> GetUnprocessedAsync(int limit = 100)
        {
            return await _db.OutboxMessages
                .Where(x => x.ProcessedDate == null)
                .OrderBy(x => x.OccuredDate)
                .Take(limit)
                .ToListAsync();
        }

        public async Task MarkProcessedAsync(Guid id)
        {
            var msg = await _db.OutboxMessages.FindAsync(id);
            if (msg == null) return;
            msg.ProcessedDate = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task IncrementAttemptAsync(Guid id, string error)
        {
            var msg = await _db.OutboxMessages.FindAsync(id);
            if (msg == null) return;
            msg.AttemptCount++;
            msg.Error = error;
            await _db.SaveChangesAsync();
        }
    }
}
