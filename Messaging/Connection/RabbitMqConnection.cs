using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

public class RabbitMqConnection : IDisposable
{
    private IConnection? _connection;
    private readonly ILogger<RabbitMqConnection> _logger;

    public bool IsConnected => _connection != null;

    public RabbitMqConnection(ILogger<RabbitMqConnection> logger)
    {
        _logger = logger;

        var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST")
            ?? throw new InvalidOperationException(
                "Variável de ambiente RABBITMQ_HOST não está definida. " +
                "Adicione-a ao arquivo .env na raiz do projeto.");

        var portStr = Environment.GetEnvironmentVariable("RABBITMQ_PORT");
        var user = Environment.GetEnvironmentVariable("RABBITMQ_USER");
        var pass = Environment.GetEnvironmentVariable("RABBITMQ_PASS");

        ConnectAsync(host, portStr, user, pass).GetAwaiter().GetResult();
    }

    private async Task ConnectAsync(string host, string? portStr, string? user, string? pass)
    {
        var resolvedUser = user ?? "guest";
        var resolvedPort = int.TryParse(portStr, out var port) ? port : 5672;
        _logger.LogInformation("RabbitMQ tentando conectar em {Host}:{Port} com usuário '{User}'", host, resolvedPort, resolvedUser);

        var factory = new ConnectionFactory
        {
            HostName = host,
            Port = resolvedPort,
            UserName = resolvedUser,
            Password = pass ?? "guest"
        };

        const int maxAttempts = 10;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                _connection = await factory.CreateConnectionAsync();
                _logger.LogInformation("RabbitMQ conectado com sucesso na tentativa {Attempt}/{Max}.", attempt, maxAttempts);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Tentativa {Attempt}/{Max} de conexão com RabbitMQ falhou.", attempt, maxAttempts);
                if (attempt < maxAttempts)
                    await Task.Delay(3000);
            }
        }

        _logger.LogCritical("RabbitMQ indisponível após {Max} tentativas. Mensageria desabilitada.", maxAttempts);
        _connection = null;
    }

    public async Task<IChannel?> CreateChannelAsync()
    {
        if (_connection == null) return null;
        return await _connection.CreateChannelAsync();
    }

    public void Dispose() => _connection?.Dispose();
}
