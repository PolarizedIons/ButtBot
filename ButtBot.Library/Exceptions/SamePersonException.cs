using ButtBot.Library.Exceptions;

namespace ButtBot.Discord.Exceptions
{
    public class SamePersonException : ButtcoinException
    {
        public SamePersonException() : base("You cannot do that to yourself!")
        {
        }
    }
}