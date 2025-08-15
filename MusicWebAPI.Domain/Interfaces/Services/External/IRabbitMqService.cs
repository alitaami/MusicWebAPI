using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Domain.Interfaces.Services.External
{
    public interface IRabbitMqService
    {
        void Publish(string routingKey, string message);
        void Consume(string routingKey, Action<string> onMessageReceived);
    }
}
