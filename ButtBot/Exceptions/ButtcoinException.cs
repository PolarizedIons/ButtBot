using System;
using ButtBot.Services;

namespace ButtBot.Exceptions
{
    public class ButtcoinException : Exception
    {
        public ButtcoinException(string message) : base(message)
        {
        }
    }
}
