using System;

namespace AkkanetFsmDemo.Models.CommandResults
{
    public class CommandFailed : ICommandResult
    {
        public CommandFailed(string reason)
        {
            this.Reason = reason;
        }
        public string Reason { get; private set; }
    }
}