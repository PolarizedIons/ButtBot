namespace ButtBot.Library.Extentions
{
    public static class StringExtentions
    {
        public static string Trim(this string value, string toTrim)
        {
            var start = value.StartsWith(toTrim) ? toTrim.Length : 0;
            var end = value.EndsWith(toTrim) ? value.Length - toTrim.Length : value.Length;
            return value.Substring(start, end - start);
        }
    }
}
