using System;
using AkkanetFsmDemo.Models.Responses;

namespace AkkanetFsmDemo.Models.CommandResults
{
    public class CommandResponse : ICommandResultWithResponse
    {
        public CommandResponse(IResponse response)
        {
            Response = response;
        }
        public IResponse Response { get; }
    }
}