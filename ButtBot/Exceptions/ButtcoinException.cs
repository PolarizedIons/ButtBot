using System;

namespace ButtBot.Exceptions
{
    public class ButtcoinException : Exception
    {
        public ButtcoinException(string message) : base(message)
        {
        }
    }
}
