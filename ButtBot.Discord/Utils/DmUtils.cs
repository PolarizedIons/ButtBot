using System.Threading.Tasks;
using Discord;
using Discord.Net;

namespace ButtBot.Discord.Utils
{
    public static class DmUtils
    {
        public static async Task SendDm(IUser user, Embed embed)
        {
            try
            {
                await user.SendMessageAsync(embed: embed);
            }
            catch (HttpException ex)
            {
                // Can't send due to privacy error or not allowing us via settings
                if (ex.DiscordCode != 50007)
                {
                    throw;
                }
            }
        }
    }
}
