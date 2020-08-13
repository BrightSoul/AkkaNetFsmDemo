using System.Collections.Generic;
using System.Linq;
using AkkanetFsmDemo.Models.Dto;

namespace AkkanetFsmDemo.Models.Responses
{
    public class ErrorResponse : IResponse
    {
        public ErrorResponse(string reason)
        {
            Reason = reason;
        }
        public string Reason { get; set; }

        public string Name => "Error";
    }
}