public class HttpErrorAuditEvent : DomainEvent
{
    public override string EventType => "http.error.audit";
    public string Method { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public int StatusCode { get; init; }
    public string ExceptionType { get; init; } = string.Empty;
    public string ExceptionMessage { get; init; } = string.Empty;
    public string CorrelationId { get; init; } = string.Empty;
}
