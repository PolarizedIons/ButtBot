using System;

namespace ButtBot.Library.Extentions
{
    public static class EnumExtentions
    {
        public static string? GetValueName<T>(this T target) where T : struct, IConvertible 
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            return Enum.GetName(typeof(T), target);
        }
    }
}
