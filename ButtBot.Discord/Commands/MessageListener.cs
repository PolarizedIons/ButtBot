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
        private readonly ConfigurationService _configurationService;
        private readonly Random _random;
        private readonly Regex _plusPlus;
        private readonly string _logoUrl;
        private readonly string _botPrefix;
        private IEnumerable<Regex>? _words;
        private Regex? _word;
        private readonly Emote _emote;

        public MessageListener(IConfiguration config, ButtcoinService buttcoinService, ConfigurationService configurationService)
        {
            _buttcoinService = buttcoinService;
            _configurationService = configurationService;
            _random = new Random();
            _botPrefix = config["Bot:Prefix"];
            _plusPlus = new Regex("^" + _botPrefix + "(.+)\\+\\+$");
            _logoUrl = config["Bot:LogoUrl"];
            _emote = Emote.Parse(config["Bot:RawEmoji"]);
        }

        public async Task OnMessageReceived(SocketMessage message)
        {
            if (!(message.Channel is SocketTextChannel) || message.Author.IsBot)
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

                        var embed = EmbedUtils.CreateTransferEmbed((IGuildUser)message.Author, (IGuildUser) toUser, fromAccount, toAccount, 1, "PlusPlus", _logoUrl).Build();

                        await DmUtils.SendDm(message.Author, embed);
                        await DmUtils.SendDm(toUser, embed);

                        await message.AddReactionAsync(_emote);
                        return;
                    }
                    catch (ButtcoinException ex)
                    {
                        await channel.SendMessageAsync(ex.Message);
                        return;
                    }
                }
            }

            // Fetch words if we don't already have it
            if (_words == null)
            {
                var stringWords = await _configurationService.GetButtWords();
                _words = stringWords.Select(x => new Regex($"\\b{x}\\b"));
                _word = PickWord();
            }

            // Mine buttcoin
            if (!message.Content.StartsWith(_botPrefix) && _word.IsMatch(message.Content))
            {
                var isBruteForce = _words.All(x => x.IsMatch(message.Content));
                await _buttcoinService.MineCoin(message.Author.Id.ToString(), isBruteForce);
                _word = PickWord();
            }
        }

        private Regex PickWord()
        {
            if (_words == null)
            {
                throw new NullReferenceException();
            }

            var word = _words.ElementAt(_random.Next(_words.Count()));
            Log.Debug("Picked new word: {Word}", word.ToString().Trim("\\b"));
            return word;
        }
    }
}
