using MusicWebAPI.Domain.Interfaces.Services.External;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace MusicWebAPI.Infrastructure.External.RabbitMq
{
    public class RabbitMqService : IRabbitMqService, IDisposable
    {
        private readonly string _hostName;
        private readonly string _exchange;
        private readonly ConnectionFactory factory;
        private IConnection connection;
        private IModel channel; 

        public RabbitMqService()
        {
            _hostName = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME");
            _exchange = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE");

            factory = new ConnectionFactory()
            {
                HostName = _hostName
            };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.ExchangeDeclare(exchange: _exchange, type: ExchangeType.Topic, durable: true);
        }

        public void Publish(string routingKey, string message)
        {
            try
            {
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(
                    exchange: _exchange,
                    routingKey: routingKey,
                    basicProperties: null,
                    body: body
                ); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RabbitMQ] ERROR while publishing message to '{routingKey}': {ex.Message}");
                throw;
            }
        }

        public void Consume(string routingKey, Action<string> onMessageReceived)
        {
            try
            {
                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue: queueName, exchange: _exchange, routingKey: routingKey);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body); 
                    onMessageReceived(message);
                };

                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RabbitMQ] ERROR while setting up consumer for '{routingKey}': {ex.Message}");
                throw;
            }
        } 
        public void Dispose()
        {
            try
            {  
                channel?.Close();
                channel?.Dispose();
                connection?.Close();
                connection?.Dispose(); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RabbitMQ] ERROR while disposing: {ex.Message}");
            }
        }
    }
}
