using System;
using System.Linq;

namespace Nass.Helpers
{
    public static class TenantGenerator
    {
        /// <summary>
        /// Generates a tenant/username from customer name and phone.
        /// Example:
        /// Name: "John Smith"
        /// Phone: "6475551234"
        /// Result: "ITH1234"
        /// </summary>
        public static string GenerateTenant(string name, string phone)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty");

            if (string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Phone cannot be empty");

            // Take last 4 characters from name (letters only)
            var nameClean = new string(name.Where(char.IsLetter).ToArray());
            var namePart = nameClean.Length >= 4
                ? nameClean.Substring(nameClean.Length - 4)
                : nameClean;

            // Take last 4 digits from phone
            var phoneDigits = new string(phone.Where(char.IsDigit).ToArray());
            var phonePart = phoneDigits.Length >= 4
                ? phoneDigits.Substring(phoneDigits.Length - 4)
                : phoneDigits;

            return $"{namePart}{phonePart}".ToUpper();
        }

        /// <summary>
        /// Generates a random 4-digit password (1000–9999)
        /// </summary>
        public static string GeneratePassword()
        {
            return Random.Shared.Next(1000, 9999).ToString();
        }
    }
}
