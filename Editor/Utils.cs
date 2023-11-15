using System;

namespace Editor
{
    public static class Utils
    {
        public static bool IsStartsWithNumber(string input)
        {
            return char.IsNumber(input[0]);
        }
        
        public static string FirstCharToUpper(string input)
        {
            return input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => input[0].ToString().ToUpper() + input.Substring(1)
            };
        }
    }
}