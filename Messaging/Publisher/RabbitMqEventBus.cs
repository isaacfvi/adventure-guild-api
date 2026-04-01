using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

public class RabbitMqEventBus : IEventBus, IDisposable
{
    private IChannel? _channel;
    private const string ExchangeName = "adventure-guild";
    private readonly ILogger<RabbitMqEventBus> _logger;

    public RabbitMqEventBus(RabbitMqConnection connection, ILogger<RabbitMqEventBus> logger)
    {
        _logger = logger;
        InitializeAsync(connection).GetAwaiter().GetResult();
    }

    private async Task InitializeAsync(RabbitMqConnection connection)
    {
        _channel = await connection.CreateChannelAsync();

        if (_channel == null)
        {
            _logger.LogCritical("RabbitMqEventBus sem canal — mensageria desabilitada.");
            return;
        }

        await _channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        _logger.LogInformation("RabbitMqEventBus inicializado.");
    }

    public void Publish<T>(T domainEvent, string routingKey) where T : DomainEvent
    {
        if (_channel == null)
        {
            _logger.LogWarning("EventBus sem conexão. Evento {EventType} descartado.", domainEvent.EventType);
            return;
        }

        try
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(domainEvent);
            _channel.BasicPublishAsync(ExchangeName, routingKey, body).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao publicar evento {EventType}.", domainEvent.EventType);
        }
    }

    public void Dispose() => _channel?.Dispose();
}
