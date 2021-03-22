namespace ButtBot.Library.Exceptions
{
    public class NotEnoughFundsException : ButtcoinException
    {
        public NotEnoughFundsException() : base("You lack the required funds to make this transaction!")
        {
        }
    }
}