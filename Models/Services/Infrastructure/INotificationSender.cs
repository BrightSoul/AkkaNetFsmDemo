using System.Threading;
using System.Threading.Tasks;

namespace AkkanetFsmDemo.Models.Services.Infrastructure
{
    public interface INotificationSender
    {
        Task SendNotification(object payload, CancellationToken token = default(CancellationToken));
    }
}