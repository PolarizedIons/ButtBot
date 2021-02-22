using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ButtBot.Discord.Services;
using ButtBot.Discord.Utils;
using ButtBot.Library.Exceptions;
using ButtBot.Library.Extentions;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace ButtBot.Discord.Commands
{
    public class MessageListener : ISingletonDiService
    {
        private readonly ButtcoinService _buttcoinService;
        private readonly IEnumerable<Regex> _words;
        private readonly Random _random;
        private Regex _word;
        private readonly Regex _plusPlus;
        private readonly string _logoUrl;
        private readonly string _botPrefix;

        public MessageListener(IConfiguration config, ButtcoinService buttcoinService)
        {
            _words = config["Bot:Words"]
                .Split(",")
                .Select(x => new Regex($"\\b{x.Trim()}\\b", RegexOptions.Multiline | RegexOptions.IgnoreCase));
            _buttcoinService = buttcoinService;
            _random = new Random();
            _word = PickWord();
            _botPrefix = config["Bot:Prefix"];
            _plusPlus = new Regex("^" + _botPrefix + "(.+)\\+\\+$");
            _logoUrl = config["Bot:LogoUrl"];
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
                    try
                    {
                        await _buttcoinService.ActivateAccount(message.Author);
                        var (fromAccount, toAccount) = await _buttcoinService.Transfer(message.Author, toUser, 1, "PlusPlus");

                        var embed = EmbedUtils.CreateTransferEmbed((IGuildUser)message.Author, (IGuildUser) toUser, fromAccount, toAccount, "PlusPlus", _logoUrl);
                        await channel.SendMessageAsync(embed: embed.Build());
                    }
                    catch (ButtcoinException ex)
                    {
                        await channel.SendMessageAsync(ex.Message);
                    }
                }
            }
            // Mine buttcoin
            else if (!message.Content.StartsWith(_botPrefix) && _word.IsMatch(message.Content))
            {
                _word = PickWord();

                var isBruteForce = _words.All(x => x.IsMatch(message.Content));
                await _buttcoinService.MineCoin(message.Author.Id.ToString(), isBruteForce);
            }
        }

        private Regex PickWord()
        {
            var word = _words.ElementAt(_random.Next(_words.Count()));
            Log.Debug("Picked new word: {Word}", word.ToString().Trim("\\b"));
            return word;
        }
    }
}
