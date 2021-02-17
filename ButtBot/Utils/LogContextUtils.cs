using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Serilog.Context;

namespace ButtBot.Utils
{
    public static class LogContextUtils
    {
        public static async Task WithMessageContext(SocketMessage message, Func<Task> action)
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
                                            await action.Invoke();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
