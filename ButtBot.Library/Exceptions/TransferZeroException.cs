namespace ButtBot.Library.Exceptions
{
    public class TransferZeroException : ButtcoinException
    {
        public TransferZeroException() : base("Did you really just try that? No.")
        {
        }
    }
}
