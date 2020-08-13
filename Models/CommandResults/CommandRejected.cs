using System;

namespace AkkanetFsmDemo.Models.CommandResults
{
    public class CommandRejected : ICommandResult
    {
        public CommandRejected(string reason)
        {
            this.Reason = reason;
        }
        public string Reason { get; private set; }
    }
}