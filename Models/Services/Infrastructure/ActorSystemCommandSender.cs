using System;
using System.Text.Json;
using System.Threading.Tasks;
using Akka.Actor;
using AkkanetFsmDemo.Models.CommandResults;
using AkkanetFsmDemo.Models.Commands;
using AkkanetFsmDemo.Models.Options;
using AkkanetFsmDemo.Models.Responses;
using Microsoft.Extensions.Options;

namespace AkkanetFsmDemo.Models.Services.Infrastructure
{
    public class ActorSystemCommandSender : ICommandSender
    {
        private readonly IActorSystemAccessor actorSystemAccessor;
        private readonly INotificationSender notificationSender;
        private readonly IOptionsMonitor<ActorSystemOptions> options;
        public ActorSystemCommandSender(IActorSystemAccessor actorSystemAccessor, INotificationSender notificationSender, IOptionsMonitor<ActorSystemOptions> options)
        {
            this.actorSystemAccessor = actorSystemAccessor;
            this.notificationSender = notificationSender;
            this.options = options;
        }

        public async Task SendCommand(byte[] payload)
        {
            var command = getCommandFromPayload(payload);
            if (command == null) {
                await NotifyError("Command not understood");
                return;
            }
            ICommandResult? commandResult = await GetCommandResult(command);

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
                case CommandTimedOut commandTimedOut:
                    await NotifyError("The command handler did not respond in a timely fashion");
                    break;
                case CommandFailed commandFailed:
                    await NotifyError($"An error occurred while handling the command: {commandFailed.Reason}");
                    break;
                case ICommandResultWithResponse commandResponse:
                    //TODO: should we notify just the sender? Right now we're notifying everybody
                    //Should use MQTT5 Request/Response pattern
                    //https://www.hivemq.com/blog/mqtt5-essentials-part9-request-response-pattern/
                    await notificationSender.SendNotification(commandResponse.Response);
                    break;
                default:
                    await NotifyError($"Command result not supported: '{commandResult?.GetType().FullName ?? "<null>"}'");
                    break;
            }
        }

        private async Task<ICommandResult?> GetCommandResult(ICommand command)
        {
            try
            {
                var timeout = TimeSpan.FromMilliseconds(options.CurrentValue.AskTimeoutInMilliseconds);
                return await actorSystemAccessor.PrimaryCommandHandler.Ask<ICommandResult?>(command, timeout);
            }
            catch (AskTimeoutException)
            {
                return new CommandTimedOut();
            }
            catch (Exception exc)
            {
                return new CommandFailed(exc.Message);
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