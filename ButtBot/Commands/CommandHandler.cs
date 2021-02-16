using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ButtBot.Extentions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Context;

namespace ButtBot.Commands
{
    public class CommandHandler : ISingletonDiService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly string _botPrefix;
        private readonly IServiceProvider _services;
        private readonly MessageListener _messageListener;
        private readonly IServiceScopeFactory _scopeFactory;

        public CommandHandler(DiscordSocketClient discord, CommandService commands, IConfiguration config, IServiceProvider services, MessageListener messageListener, IServiceScopeFactory scopeFactory)
        {
            _discord = discord;
            _commands = commands;
            _botPrefix = config["Bot:Prefix"];
            _services = services;
            _messageListener = messageListener;
            _scopeFactory = scopeFactory;
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(
                assembly: Assembly.GetEntryAssembly(),
                services: _services
            );
            
            _discord.MessageReceived += DiscordOnMessageReceived;
            _discord.Ready += DiscordOnReady;
            _commands.CommandExecuted += OnCommandExecutedAsync;
        }

        private async Task DiscordOnMessageReceived(SocketMessage message)
        {
            using (LogContext.PushProperty("Author", message.Author))
            {
                using (LogContext.PushProperty("AuthorId", message.Author.Id))
                {
                    using (LogContext.PushProperty("Message", message.Content))
                    {
                        using (LogContext.PushProperty("MessageId", message.Id))
                        {
                            using (LogContext.PushProperty("Channel", message.Channel.Name))
                            {
                                using (LogContext.PushProperty("ChannelId", message.Channel.Id))
                                {
                                    using (LogContext.PushProperty("Guild", (message.Channel as IGuildChannel)?.Guild.Name))
                                    {
                                        using (LogContext.PushProperty("GuildId", (message.Channel as IGuildChannel)?.Guild.Id))
                                        {
                                            await _messageListener.OnMessageReceived(message);
                                            await HandleCommandAsync(message);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private Task DiscordOnReady()
        {
            Log.Information("Discord is Ready!");
            return Task.CompletedTask;
        }

        private async Task HandleCommandAsync(SocketMessage socketMessage)
        {
            if (!(socketMessage is SocketUserMessage msg) || msg.Author.IsBot)
            {
                return;
            }

            var argPos = 0;
            if (!msg.HasStringPrefix(_botPrefix, ref argPos))
            {
                return;
            }

            using (var scope = _scopeFactory.CreateScope())
            {
                var context = new CommandContext(_discord, msg);
                await _commands.ExecuteAsync(
                    context: context,
                    argPos: argPos,
                    services: scope.ServiceProvider
                );   
            }
        }

        private async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // We don't care about unknown commands
            if (result.Error == CommandError.UnknownCommand)
            {
                return;
            }

            if (!result.IsSuccess && !string.IsNullOrEmpty(result.ErrorReason))
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
                Log.Error("Error {ErrorType}: '{CommandName}', {ExceptionMessage}", result.Error, command.Value?.Name, result.ErrorReason);
            }
        }
    }
}
