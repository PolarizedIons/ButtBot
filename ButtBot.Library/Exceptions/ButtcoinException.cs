using System;

namespace ButtBot.Library.Exceptions
{
    public class ButtcoinException : Exception
    {
        public ButtcoinException(string message) : base(message)
        {
        }
    }
}
