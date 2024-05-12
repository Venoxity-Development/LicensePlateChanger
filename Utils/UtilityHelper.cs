using System;
using System.Text;

namespace LicensePlateChanger.Utils
{
    internal static class UtilityHelper
    {
        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());

        /// <summary>
        /// Transforms a string by replacing '#' with a random digit, '?' with a random uppercase letter, and '.' with a random letter or number, with 50% probability of being either.
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
                else if (c == '.')
                {
                    if (Random.Next(2) == 0) // 50% chance of being either a letter or a number
                    {
                        int randomNumber = Random.Next(10);
                        result.Append(randomNumber);
                    }
                    else
                    {
                        char randomChar = GetUniqueRandomChar();
                        result.Append(randomChar);
                    }
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
