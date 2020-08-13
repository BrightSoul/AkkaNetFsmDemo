using System;
using System.Threading.Tasks;
using Akka.Actor;
using AkkanetFsmDemo.Models.DomainEvents;
using AkkanetFsmDemo.Models.Responses;
using AkkanetFsmDemo.Models.Services.Infrastructure;

namespace AkkanetFsmDemo.Models.Actors.EventHandlers
{
    public class NotificationActor : ReceiveActor
    {
        private readonly INotificationSender notificationSender;

        public NotificationActor(INotificationSender notificationSender)
        {
            this.notificationSender = notificationSender;
            ReceiveAnyAsync(Notify);
        }

        private async Task Notify(object domainEvent)
        {
            await notificationSender.SendNotification(new DomainEventResponse(domainEvent));
        }
    }
}