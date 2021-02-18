using Discord;

namespace ButtBot.Extentions
{
    public static class UserExtentions
    {
        public static string GetDisplayName(this IUser user)
        {
            if (user is IGuildUser guildUser && !string.IsNullOrWhiteSpace(guildUser.Nickname))
            {
                return guildUser.Nickname;
            }

            return $"{user.Username}#{user.Discriminator}";
        }
    }
}
