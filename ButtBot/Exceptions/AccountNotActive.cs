namespace ButtBot.Exceptions
{
    public class AccountNotActive : ButtcoinException
    {
        public AccountNotActive() : base("The target user doesn't have an active account!")
        {
        }
    }
}
