using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Persistence;
using AkkanetFsmDemo.Models.Commands;
using MQTTnet.Server;

namespace AkkanetFsmDemo.Models.Services.Infrastructure
{
    public class ApplicationMessageInterceptor : IMqttServerApplicationMessageInterceptor
    {
        private readonly ICommandSender commandSender;
        public INotificationSender NotificationSender { get; }
        public ApplicationMessageInterceptor(ICommandSender commandSender, INotificationSender notificationSender)
        {
            this.NotificationSender = notificationSender;
            this.commandSender = commandSender;
        }
        public async Task InterceptApplicationMessagePublishAsync(MqttApplicationMessageInterceptorContext context)
        {
            //TODO: Authorization. Clients must only be able to post messages to the 'commands' topic
            //Only the "System" client must be able to post to the 'notifications' topic
            if (context.ApplicationMessage.Topic != "commands") {
                context.AcceptPublish = true;
                return;
            }
            try
            {
                await commandSender.SendCommand(context.ApplicationMessage.Payload);
                context.ApplicationMessage.Payload = null;
            }
            catch (Exception)
            {
                //Message was not valid or client could not publish it
                context.AcceptPublish = false;
            }
        }
    }
}