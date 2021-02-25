using ButtBot.Library.Extentions;
using ButtBot.Twitch.Messages;
using Microsoft.Extensions.Configuration;
using Serilog;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;

namespace ButtBot.Twitch.Services
{
    public class TwitchService : ISingletonDiService
    {
        private readonly MessageListener _messageListener;
        private readonly TwitchClient _client;

        public TwitchService(IConfiguration config, MessageListener messageListener)
        {
            _messageListener = messageListener;
            var credentials = new ConnectionCredentials(config["Bot:Username"], config["Bot:AccessToken"]);
            var wsClient = new WebSocketClient();
            _client = new TwitchClient(wsClient);
            _client.Initialize(credentials, config["Bot:ChannelName"]);
            
            _client.OnJoinedChannel += ClientOnOnJoinedChannel;
            _client.OnMessageReceived += ClientOnOnMessageReceived;
        }

        public void Start()
        {
            _client.Connect();
        }

        public void Stop()
        {
            _client.Disconnect();
        }

        private void ClientOnOnJoinedChannel(object? sender, OnJoinedChannelArgs e)
        {
            Log.Information("Joined {Channel}!", e.Channel);
        }

        private void ClientOnOnMessageReceived(object? client, OnMessageReceivedArgs e)
        {
            _messageListener.OnReceive(e.ChatMessage.UserId, e.ChatMessage.Message).Wait();
        }
    }
}
