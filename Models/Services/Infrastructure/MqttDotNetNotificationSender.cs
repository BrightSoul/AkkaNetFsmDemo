using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AkkanetFsmDemo.Models.DomainEvents;
using AkkanetFsmDemo.Models.Options;
using AkkanetFsmDemo.Models.Responses;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;

namespace AkkanetFsmDemo.Models.Services.Infrastructure
{
    public class MqttDotNetNotificationSender : INotificationSender
    {
        private readonly IOptionsMonitor<MqttOptions> options;

        public MqttDotNetNotificationSender(IOptionsMonitor<MqttOptions> options)
        {
            this.options = options;
        }

        public async Task SendNotification(object data, CancellationToken cancellationToken = default(CancellationToken))
        {          
            using var mqttClient = CreateClient();
            var mqttClientOptions = CreateClientOptions();
            var result = await mqttClient.ConnectAsync(mqttClientOptions, cancellationToken);
            if (result.ResultCode != MqttClientConnectResultCode.Success)
            {
                throw new InvalidOperationException("Couldn't connect to MQTT server");
            }
            var payload = JsonSerializer.Serialize(data);
            await mqttClient.PublishAsync(options.CurrentValue.NotificationTopicName, payload);
            await mqttClient.DisconnectAsync(new MqttClientDisconnectOptions()
            {
                ReasonCode = MqttClientDisconnectReason.NormalDisconnection
            }, cancellationToken);

        }

        private IMqttClientOptions CreateClientOptions()
        {
            var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithClientId(options.CurrentValue.ClientName)
                    .WithWebSocketServer(options.CurrentValue.WebSocketServer, new MqttClientOptionsBuilderWebSocketParameters())
                    .WithTls() //TODO: Use certificate based auth here https://github.com/chkr1011/MQTTnet/wiki/Client#certificate-based-authentication
                    .WithCleanSession()
                    .Build();
            return mqttClientOptions;
        }

        private IMqttClient CreateClient()
        {
            var factory = new MqttFactory();
            var mqttClient = factory.CreateMqttClient();
            return mqttClient;
        }
    }
}