using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ButtBot.Exceptions;
using ButtBot.Extentions;
using ButtBot.Services;
using ButtBot.Utils;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ButtBot.Commands
{
    public class MessageListener : ISingletonDiService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEnumerable<Regex> _words;
        private readonly Random _random;
        private Regex _word;
        private readonly Regex _plusPlus;
        private readonly Emote _emote;

        public MessageListener(IConfiguration config, IServiceScopeFactory scopeFactory)
        {
            _words = config["Bot:Words"]
                .Split(",")
                .Select(x => new Regex($"\\b{x.Trim()}\\b", RegexOptions.Multiline | RegexOptions.IgnoreCase));
            _scopeFactory = scopeFactory;
            _random = new Random();
            _word = PickWord();
            _plusPlus = new Regex("^" + config["Bot:Prefix"] + "(.+)\\+\\+$");
            _emote = Emote.Parse(config["Bot:Emote"]);
        }

        public async Task OnMessageReceived(SocketMessage message)
        {
            if (!(message.Channel is SocketTextChannel))
            {
                return;
            }
            
            // Transfer 1
            var plusPlusMatch = _plusPlus.Match(message.Content);
            if (plusPlusMatch.Success && message.Channel is SocketTextChannel channel)
            {
                var userSearch = plusPlusMatch.Groups[1].Value;
                IUser? toUser;
                if (MentionUtils.TryParseUser(userSearch, out var userId))
                {
                    toUser = channel.GetUser(userId);
                }
                else
                {
                    toUser = (await channel.Guild.SearchUsersAsync(userSearch, 1)).FirstOrDefault();
                }

                if (toUser != null)
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var buttcoinService = scope.ServiceProvider.GetRequiredService<ButtcoinService>();
                        try
                        {
                            await buttcoinService.ActivateAccount(message.Author);
                            var (fromAccount, toAccount) = await buttcoinService.Transfer(message.Author, toUser, 1, "PlusPlus");

                            var embed = EmbedUtils.CreateTransferEmbed((IGuildUser)message.Author, (IGuildUser) toUser, fromAccount, toAccount, "PlusPlus", _emote.Url);
                            await channel.SendMessageAsync(embed: embed.Build());
                        }
                        catch (ButtcoinException ex)
                        {
                            await channel.SendMessageAsync(ex.Message);
                        }
                    }
                }
            }
            // Mine buttcoin
            else if (_word.IsMatch(message.Content))
            {
                _word = PickWord();

                var isBruteForce = _words.All(x => x.IsMatch(message.Content));
                using (var scope = _scopeFactory.CreateScope())
                {
                    var buttcoinService = scope.ServiceProvider.GetRequiredService<ButtcoinService>();
                    await buttcoinService.MineCoin(message.Author.Id.ToString(), isBruteForce);
                }
            }
        }

        private Regex PickWord()
        {
            var word = _words.ElementAt(_random.Next(_words.Count()));
            Log.Debug("Picked new word: {Word}", word);
            return word;
        }
    }
}
