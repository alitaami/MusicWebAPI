using Common.Utilities;
using MusicWebAPI.Domain.Interfaces.Services.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Infrastructure.Outbox
{ 
    public class OutboxProcessor
    {
        private readonly IOutboxService _outbox;
        public OutboxProcessor(IOutboxService outbox)
        {
            _outbox = outbox;
        }

        // Hangfire will call this method periodically
        public async Task ProcessPendingAsync()
        {
            var messages = await _outbox.GetUnprocessedAsync(limit: 50);

            foreach (var msg in messages)
            {
                try
                {
                    if (msg.Type.StartsWith("Email:", StringComparison.OrdinalIgnoreCase))
                    {
                        var email = System.Text.Json.JsonSerializer.Deserialize<EmailPayload>(msg.Content);
                        if (email != null)
                        { 
                            await SendMail.SendAsync(email.To, email.Subject, email.Body);
                        }
                    }
                    else if (msg.Type.StartsWith("Event:", StringComparison.OrdinalIgnoreCase))
                    {
                        // TODO: publish to message broker 
                    }
                     
                    await _outbox.MarkProcessedAsync(msg.Id);
                }
                catch (Exception ex)
                { 
                    await _outbox.IncrementAttemptAsync(msg.Id, ex.Message);
                }
            }
        }
    }
     
    public class EmailPayload { public string To { get; set; } = ""; public string Subject { get; set; } = ""; public string Body { get; set; } = ""; }
}
