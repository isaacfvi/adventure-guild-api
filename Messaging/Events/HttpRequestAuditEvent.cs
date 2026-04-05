public class HttpRequestAuditEvent : DomainEvent
{
    public override string EventType => "http.request.audit";
    public string Method { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public int StatusCode { get; init; }
    public long ElapsedMs { get; init; }
    public string CorrelationId { get; init; } = string.Empty;
}
