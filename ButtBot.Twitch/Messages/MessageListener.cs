using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using ButtBot.Library.Extentions;
using ButtBot.Twitch.Services;
using Serilog;

namespace ButtBot.Twitch.Messages
{
    public class MessageListener : ISingletonDiService
    {
        private readonly ConfigurationService _configurationService;
        private readonly ButtcoinService _buttcoinService;
        private readonly TwitchApi _twitchApi;
        private readonly Random _random;
        private IEnumerable<Regex>? _words;
        private Regex? _word;
        private bool _isLive;

        public MessageListener(ConfigurationService configurationService, ButtcoinService buttcoinService, TwitchApi twitchApi)
        {
            _configurationService = configurationService;
            _buttcoinService = buttcoinService;
            _twitchApi = twitchApi;
            _random = new Random();

            _isLive = twitchApi.IsTargetLive().Result;
            var isLiveTimer = new Timer();
            isLiveTimer.Elapsed += async (sender, args) =>
            {
                _isLive = await twitchApi.IsTargetLive();
            };
            isLiveTimer.Interval = TimeSpan.FromMinutes(5).TotalSeconds;
            isLiveTimer.Enabled = true;
        }

        public async Task OnReceive(string authorId, string message)
        {
            if (_words == null)
            {
                var stringWords = await _configurationService.GetButtWords();
                _words = stringWords.Select(x => new Regex($"\\b{x}\\b"));
                _word = PickWord();
            }

            if (_word.IsMatch(message))
            {
                if (!_isLive)
                {
                    if (Debugger.IsAttached)
                    {
                        Log.Debug("Channel isn't live, but I'm debugging, so I'll allow it");
                    }
                    else
                    {
                        return;
                    }
                }

                var isBruteForce = _words.All(x => x.IsMatch(message));
                await _buttcoinService.MineCoin(authorId, isBruteForce);
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
