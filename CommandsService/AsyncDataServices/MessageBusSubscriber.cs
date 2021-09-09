using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandsService.EventProcessing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommandsService.AsyncDataServices
{
    // Background Service running as a "Long Running Task"
    public class MessageBusSubscriber : BackgroundService, IDisposable
    {
        private const string EXCHANGE_NAME = "trigger";
        private readonly IConfiguration _configuration;
        private readonly IEventProcessor _eventProcessor;
        private IConnection _connection;
        private IModel _channel;
        private string _queueName;

        public MessageBusSubscriber(IConfiguration configuration, IEventProcessor eventProcessor)
        {
            this._configuration = configuration;
            this._eventProcessor = eventProcessor;

            InitializeMessageBusConnection();
        }

        private void InitializeMessageBusConnection()
        {
            var connectionFactory = new ConnectionFactory()
            {
                HostName = this._configuration["RabbitMQHost"],
                Port = int.Parse(this._configuration["RabbitMQPort"])
            };

            this._connection = connectionFactory.CreateConnection();
            this._channel = this._connection.CreateModel();
            this._channel.ExchangeDeclare(
                exchange: EXCHANGE_NAME,
                type: ExchangeType.Fanout
            );
            this._queueName = this._channel.QueueDeclare().QueueName;
            this._channel.QueueBind(
                queue: this._queueName,
                exchange: EXCHANGE_NAME,
                routingKey: ""
            );

            Console.WriteLine("--> Listening on Message Bus");

            this._connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(this._channel);

            consumer.Received += (ModuleHandle, ea) =>
            {
                Console.WriteLine("--> Event received");

                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body.ToArray());

                this._eventProcessor.ProcessEvent(message);
            };

            this._channel.BasicConsume(
                queue: this._queueName,
                autoAck: true,
                consumer: consumer
            );

            return Task.CompletedTask;
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> Connection Shutdown");
        }

        public override void Dispose()
        {
            if (this._channel.IsOpen)
            {
                this._channel.Close();
                this._connection.Close();
            }

            base.Dispose();
        }
    }
}