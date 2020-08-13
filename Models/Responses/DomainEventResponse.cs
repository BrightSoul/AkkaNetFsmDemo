namespace AkkanetFsmDemo.Models.Responses
{
    public class DomainEventResponse : IResponse
    {
        public DomainEventResponse(object payload)
        {
            Payload = payload;
            DomainEventName = payload.GetType().Name;
        }

        public string DomainEventName { get; }
        
        public object Payload { get; }

        public string Name => "DomainEvent";
    }
}