using System.Threading.Tasks;
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
        private readonly string[] _channelsToJoin;

        public TwitchService(IConfiguration config, MessageListener messageListener)
        {
            _messageListener = messageListener;
            var credentials = new ConnectionCredentials(config["Bot:Username"], config["Bot:AccessToken"]);
            var wsClient = new WebSocketClient();
            _client = new TwitchClient(wsClient);
            _client.Initialize(credentials);

            _channelsToJoin = config["Bot:ChannelNames"].Split(",");

            _client.OnJoinedChannel += ClientOnOnJoinedChannel;
            _client.OnMessageReceived += ClientOnOnMessageReceived;
        }

        public void Start()
        {
            _client.Connect();
            foreach (var channel in _channelsToJoin)
            {
                // If I don't wait, it doesn't join all channels >.>
                Task.Delay(1000).Wait();
                Log.Debug("Joining {Channel}", channel);
                _client.JoinChannel(channel);
            }
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
            _messageListener.OnReceive(e.ChatMessage.RoomId, e.ChatMessage.UserId, e.ChatMessage.Message).Wait();
        }
    }
}
