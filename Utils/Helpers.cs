using System;
using System.Text;

namespace LicensePlateChanger.Utils
{
    internal static class Helpers
    {
        /// <summary>
        /// Transforms a string by replacing '#' with a random digit and '?' with a random uppercase letter.
        /// </summary>
        public static string TransformString(string input)
        {
            Random random = new Random();
            StringBuilder result = new StringBuilder();

            foreach (char c in input)
            {
                if (c == '#')
                {
                    int randomNumber = random.Next(10);
                    result.Append(randomNumber);
                }
                else if (c == '?')
                {
                    char randomChar = (char)random.Next('A', 'Z' + 1);
                    result.Append(randomChar);
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }
    }
}
