using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PlatformService.Dtos;
using RabbitMQ.Client;

namespace PlatformService.AsyncDataServices.Http
{
    public class MessageBusClient : IMessageBusClient, IDisposable
    {
        private const string EXCHANGE_NAME = "trigger";
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessageBusClient(IConfiguration configuration)
        {
            this._configuration = configuration;

            var connectionFactory = new ConnectionFactory()
            {
                HostName = this._configuration["RabbitMQHost"],
                Port = int.Parse(this._configuration["RabbitMQPort"])
            };

            try
            {
                this._connection = connectionFactory.CreateConnection();
                this._channel = this._connection.CreateModel();

                this._channel.ExchangeDeclare(
                    exchange: EXCHANGE_NAME,
                    type: ExchangeType.Fanout
                );

                this._connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

                Console.WriteLine($"--> RabbitMQ Connection established");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connect to RabbitMQ: {ex.Message}");
            }
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine($"--> RabbitMQ Connection Shutdown");
        }

        public void PublishNewPlatform(PlatformPublishDto platformPublishDto)
        {
            var message = JsonSerializer.Serialize<PlatformPublishDto>(platformPublishDto);

            if (this._connection.IsOpen)
            {
                Console.WriteLine($"--> Sending Message");
                SendMessage(message);
            }
            else
            {
                Console.WriteLine($"--> Connection not open - not sending");
            }
        }

        private void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            this._channel.BasicPublish(
                exchange: EXCHANGE_NAME,
                routingKey: "",
                basicProperties: null,
                body: body
            );

            Console.WriteLine($"--> Message published: {message}");
        }

        public void Dispose()
        {
            Console.WriteLine($"--> Disposing Message Bus Client");

            if (this._channel.IsOpen)
            {
                this._channel.Close();
                this._connection.Close();
            }
        }
    }
}