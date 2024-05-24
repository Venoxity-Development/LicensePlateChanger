using GTA;
using GTA.Native;
using LicensePlateChanger.Models;
using System;
using System.Text;

namespace LicensePlateChanger.Utils
{
    internal static class UtilityHelper
    {
        #region Fields
        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());
        #endregion

        #region Transformation
        /// <summary>
        /// Transforms a string by replacing '#' with a random digit, '?' with a random uppercase letter, and '.' with a random letter or number, with 50% probability of being either.
        /// </summary>
        /// <param name="input">The input string to transform</param>
        /// <returns>The transformed string</returns>
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
        #endregion

        #region Plate Checking
        /// <summary>
        /// Checks if a given license plate is already in use.
        /// </summary>
        /// <param name="plate">The license plate to check</param>
        /// <returns>True if the plate is already in use, otherwise false</returns>
        public static bool IsPlateAlreadyUsed(string plate)
        {
            foreach (var kvp in Globals.vehicleLicensePlates)
            {
                if (kvp.Value == plate)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Plate Updates
        /// <summary>
        /// Updates the type of license plate for a vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle whose license plate type will be updated</param>
        /// <param name="newPlateSet">The new plate set containing the updated plate type</param>
        public static void UpdateLicensePlateType(Vehicle vehicle, PlateSet newPlateSet)
        {
            Function.Call<int>(Hash.SET_VEHICLE_NUMBER_PLATE_TEXT_INDEX, vehicle, newPlateSet.plateType);
            Globals.vehicleLicenseClassName[vehicle.Handle] = newPlateSet.plateType;
        }

        /// <summary>
        /// Updates the format of the license plate for a vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle whose license plate format will be updated</param>
        /// <param name="newPlateSet">The new plate set containing the updated plate format</param>
        public static void UpdateLicensePlateFormat(Vehicle vehicle, PlateSet newPlateSet)
        {
            string transformedPlateFormat = TransformString(newPlateSet.plateFormat);
            vehicle.Mods.LicensePlate = transformedPlateFormat;
            Globals.vehicleLicensePlates[vehicle.Handle] = transformedPlateFormat;
        }
        #endregion
    }
}
