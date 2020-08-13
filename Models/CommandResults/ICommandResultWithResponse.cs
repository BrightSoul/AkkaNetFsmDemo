using System;
using AkkanetFsmDemo.Models.Responses;

namespace AkkanetFsmDemo.Models.CommandResults
{
    public interface ICommandResultWithResponse : ICommandResult
    {
        public IResponse Response { get; }
    }
}