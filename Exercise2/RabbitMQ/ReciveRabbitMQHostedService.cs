using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EmployeeManagerment.RabbitMQ
{
    public class ReciveRabbitMQHostedService : BackgroundService
    {
        //public readonly IConfiguration _configuration;
        //private IConnection _connection;
        private IModel _channel;
        private readonly DefaultObjectPool<IModel> _objectPool;
        public ReciveRabbitMQHostedService(/*IConfiguration configuration,*/ IPooledObjectPolicy<IModel> objectPolicy)
        {
            //_configuration = configuration;
            _objectPool = new DefaultObjectPool<IModel>(objectPolicy, Environment.ProcessorCount * 2);
            InitRabbitMQ();          
        }

        private void InitRabbitMQ()
        {
            //var factory = new ConnectionFactory()
            //{
            //    HostName = _configuration.GetSection("RabbitMqConnection")["HostName"],
            //    UserName = _configuration.GetSection("RabbitMqConnection")["UserName"],
            //    Password = _configuration.GetSection("RabbitMqConnection")["Password"],
            //    Port = 5672,
            //    VirtualHost = "/",
            //};
            //_connection = factory.CreateConnection();
            //_channel = _connection.CreateModel();
            _channel = _objectPool.Get();
            try
            {
                _channel.ExchangeDeclarePassive("bactestRabbitMQ");

                QueueDeclareOk ok = _channel.QueueDeclarePassive("bactestRabbitMQ");
                if (ok.MessageCount > 0)
                {
                    _channel.QueueBind("bactestRabbitMQ", "bactestRabbitMQ", "bactestRabbitMQ", null);
                    _channel.BasicQos(0, 1, false);
                }
            }
            catch {}
            //_connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                var content = System.Text.Encoding.UTF8.GetString(ea.Body);

                HandleMessage(content);
                _channel.BasicAck(ea.DeliveryTag, false);
            };
            //consumer.Shutdown += OnConsumerShutdown;
            //consumer.Registered += OnConsumerRegistered;
            //consumer.Unregistered += OnConsumerUnregistered;
            //consumer.ConsumerCancelled += OnConsumerConsumerCancelled;
            try
            {
                _channel.BasicConsume("bactestRabbitMQ", false, consumer);
            }catch { }
            return Task.CompletedTask;
        }
        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }

        private void HandleMessage(string content)
        {
            Console.WriteLine(content);
        }
        public override void Dispose()
        {
            _channel.Close();
            //_connection.Close();
            base.Dispose();
        }
    }
}
