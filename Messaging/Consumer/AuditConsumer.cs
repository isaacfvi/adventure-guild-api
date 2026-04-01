using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

public class AuditConsumer : IHostedService
{
    private const string QueueName = "adventure-guild.audit";
    private const string DlqName = "adventure-guild.audit.dlq";
    private const string ExchangeName = "adventure-guild";

    private readonly RabbitMqConnection _rabbitConnection;
    private readonly ILogger<AuditConsumer> _logger;
    private IChannel? _channel;

    public AuditConsumer(RabbitMqConnection rabbitConnection, ILogger<AuditConsumer> logger)
    {
        _rabbitConnection = rabbitConnection;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _channel = await _rabbitConnection.CreateChannelAsync();

        if (_channel == null)
        {
            _logger.LogError("AuditConsumer sem canal — consumer não iniciado.");
            return;
        }

        // Declare DLQ first (referenced by main queue)
        await _channel.QueueDeclareAsync(
            queue: DlqName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        // Declare main audit queue with dead-letter exchange pointing to DLQ
        var queueArgs = new Dictionary<string, object?> { { "x-dead-letter-exchange", DlqName } };
        await _channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArgs,
            cancellationToken: cancellationToken);

        // Bind to exchange with wildcard — receives all events
        await _channel.QueueBindAsync(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: "#",
            cancellationToken: cancellationToken);

        _logger.LogInformation("AuditConsumer iniciado. Aguardando mensagens na fila {Queue}.", QueueName);

        await RegisterConsumerAsync(cancellationToken);
    }

    private async Task RegisterConsumerAsync(CancellationToken cancellationToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel!);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            await HandleEventAsync(ea);
        };

        await _channel!.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Dispose();
        _logger.LogInformation("AuditConsumer encerrado.");
        return Task.CompletedTask;
    }

    private async Task HandleEventAsync(BasicDeliverEventArgs ea)
    {
        try
        {
            var payload = Encoding.UTF8.GetString(ea.Body.ToArray());
            var routingKey = ea.RoutingKey;

            string eventType = routingKey;
            if (ea.BasicProperties.Headers != null &&
                ea.BasicProperties.Headers.TryGetValue("eventType", out var headerValue) &&
                headerValue is byte[] headerBytes)
            {
                eventType = Encoding.UTF8.GetString(headerBytes);
            }

            _logger.LogInformation(
                "Evento recebido — EventType: {EventType}, RoutingKey: {RoutingKey}, Payload: {Payload}",
                eventType, routingKey, payload);

            await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem da fila {Queue}. Enviando para DLQ.", QueueName);
            await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
        }
    }
}
