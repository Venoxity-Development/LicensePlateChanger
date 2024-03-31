using System;
using System.Text;

namespace LicensePlateChanger.Utils
{
    internal static class Helpers
    {
        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());

        /// <summary>
        /// Transforms a string by replacing '#' with a random digit and '?' with a random uppercase letter.
        /// </summary>
        public static string TransformString(string input)
        {
            StringBuilder result = new StringBuilder();

            foreach (char c in input)
            {
                if (c == '#')
                {
                    int randomNumber = Random.Next(10); 
                    result.Append(randomNumber);
                }
                else if (c == '?')
                {
                    char randomChar = GetUniqueRandomChar();
                    result.Append(randomChar);
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        private static char GetUniqueRandomChar()
        {
            char randomChar;
            do
            {
                randomChar = (char)Random.Next('A', 'Z' + 1);
            } while (randomChar == 'I' || randomChar == 'O'); // Exclude 'I' and 'O' to avoid confusion with '1' and '0'
            return randomChar;
        }
    }
}
