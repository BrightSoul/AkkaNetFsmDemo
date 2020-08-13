namespace AkkanetFsmDemo.Models.Options
{
    public class MqttOptions
    {
        public string ClientName { get; set; } = string.Empty;
        public string NotificationTopicName { get; set; } = string.Empty;
        public string WebSocketServer { get; set; } = string.Empty;
    }
}