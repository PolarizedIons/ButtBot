namespace ButtBot.Exceptions
{
    public class SamePersonException : ButtcoinException
    {
        public SamePersonException() : base("You cannot do that to yourself!")
        {
        }
    }
}