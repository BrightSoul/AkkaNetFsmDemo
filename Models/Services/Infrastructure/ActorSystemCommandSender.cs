using System;
using System.Text.Json;
using System.Threading.Tasks;
using Akka.Actor;
using AkkanetFsmDemo.Models.CommandResults;
using AkkanetFsmDemo.Models.Commands;
using AkkanetFsmDemo.Models.Responses;

namespace AkkanetFsmDemo.Models.Services.Infrastructure
{
    public class ActorSystemCommandSender : ICommandSender
    {
        private readonly IActorSystemAccessor actorSystemAccessor;
        private readonly INotificationSender notificationSender;
        public ActorSystemCommandSender(IActorSystemAccessor actorSystemAccessor, INotificationSender notificationSender)
        {
            this.actorSystemAccessor = actorSystemAccessor;
            this.notificationSender = notificationSender;
        }

        public async Task SendCommand(byte[] payload)
        {
            var command = getCommandFromPayload(payload);
            if (command == null) {
                await NotifyError("Command not understood");
                return;
            }
            var commandResult = await actorSystemAccessor.PrimaryCommandHandler.Ask<ICommandResult>(command);
            switch (commandResult)
            {
                case CommandAccepted commandAccepted:
                    //Do nothing, everything went as planned
                    break;
                case CommandRejected commandRejected:
                    //Oh well, let's throw
                    await NotifyError(commandRejected.Reason);
                    break;
                case CommandDiscarded commandDiscarded:
                    //This command is not acceptable at the state the FSM in in
                    await NotifyError("Cannot send this command now");
                    break;
                case ICommandResultWithResponse commandResponse:
                    //TODO: should we notify just the sender? Right now we're notifying everybody
                    //Should use MQTT5 Request/Response pattern
                    //https://www.hivemq.com/blog/mqtt5-essentials-part9-request-response-pattern/
                    await notificationSender.SendNotification(commandResponse.Response);
                    break;
                default:
                    await NotifyError($"Command result not supported: '{commandResult?.GetType().FullName}'");
                    break;
            }
        }

        private async Task NotifyError(string reason)
        {
            await notificationSender.SendNotification(new ErrorResponse(reason));
            throw new InvalidOperationException(reason);
        }

        private ICommand? getCommandFromPayload(byte[] payload)
        {
            try {
                var document = JsonDocument.Parse(payload);
                var productName = document.RootElement.GetProperty("Name").GetString();
                return productName switch
                {
                    nameof(GetCart) => new GetCart(),
                    nameof(AddProduct) => new AddProduct(document.RootElement.GetProperty("ProductName").GetString()),
                    nameof(RemoveProduct) => new RemoveProduct(document.RootElement.GetProperty("ProductName").GetString()),
                    nameof(ConfirmCart) => new ConfirmCart(),
                    _ => null
                };
            } catch (Exception) {
                return null;
            }
        }
    }
}