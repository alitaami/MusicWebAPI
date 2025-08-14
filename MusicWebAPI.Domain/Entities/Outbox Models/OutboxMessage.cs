using Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Infrastructure.Outbox
{
    public sealed class OutboxMessage : BaseEntity<Guid>
    {
        public string Type { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime OccuredDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public int AttemptCount { get; set; } = 0;
        public string? Error { get; set; } = string.Empty;
    }
}
