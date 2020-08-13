using System.Threading.Tasks;
using Akka.Persistence;
using AkkanetFsmDemo.Models.Commands;

namespace AkkanetFsmDemo.Models.Services.Infrastructure
{
    public interface ICommandSender
    {
        Task SendCommand (byte[] payload);
    }
}