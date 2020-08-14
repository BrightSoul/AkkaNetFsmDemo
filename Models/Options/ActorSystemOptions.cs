namespace AkkanetFsmDemo.Models.Options
{
    public class ActorSystemOptions
    {
        public string PersistenceId { get; set; } = string.Empty;
        public string PrimaryCommandHandler { get; set; } = string.Empty;
        public string PrimaryEventHandler { get; set; } = string.Empty;
        public int AskTimeoutInMilliseconds { get; set; } = 3000;
    }
}