using JobMonitor.Domain.Models.Configs;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace JobMonitor.Application.RabbitMq;

public class RabbitMqService
{
    private readonly RabbitMqConfig _config;
    private readonly ConnectionFactory _factory;

    public RabbitMqService(IOptions<RabbitMqConfig> options)
    {
        _config = options.Value;
        _factory = new ConnectionFactory
        {
            HostName = _config.HostName,
            Port = _config.Port,
            UserName = _config.UserName,
            Password = _config.Password,
            VirtualHost = _config.VirtualHost
        };
    }

    public async Task Publish<T>(string queue, T message)
    {
        var connection = await _factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = new BasicProperties();
        await channel.BasicPublishAsync("", queue, false, props, body);
    }
}
