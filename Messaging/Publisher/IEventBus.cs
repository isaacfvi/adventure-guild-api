public interface IEventBus
{
    void Publish<T>(T domainEvent, string routingKey) where T : DomainEvent;
}
