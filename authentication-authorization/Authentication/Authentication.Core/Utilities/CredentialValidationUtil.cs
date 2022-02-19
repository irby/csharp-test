using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Authentication.Core.Utilities
{
    public static class CredentialValidationUtil
    {
        private const int MinimumPasswordLength = 8;
        private const string ValidSpecialCharacters = "!@#$%^&*><()";

        public static bool PasswordContainsUppercase(string input)
        {
            return Regex.IsMatch(input, "[A-Z]");
        }

        public static bool PasswordContainsLowercase(string input)
        {
            return Regex.IsMatch(input, "[a-z]");
        }

        public static bool PasswordContainsNumbers(string input)
        {
            return Regex.IsMatch(input, "[0-9]");
        }

        public static bool PasswordContainsSpecialCharacters(string input)
        {
            return Regex.IsMatch(input, $"[{ValidSpecialCharacters}]");
        }
        
        public static bool PasswordContainsOnlyApprovedCharacters(string input)
        {
            return Regex.IsMatch(input, $"^[A-Za-z0-9 {ValidSpecialCharacters}]+$");
        }
        
        public static bool PasswordIsValidLength(string password)
        {
            return password.Length >= MinimumPasswordLength;
        }
        
        public static bool IsValidPassword(string password)
        {
            return PasswordIsValidLength(password) && PasswordContainsLowercase(password)
                                                   && PasswordContainsUppercase(password) && PasswordContainsNumbers(password)
                                                   && PasswordContainsSpecialCharacters(password) && PasswordContainsOnlyApprovedCharacters(password);

        }
        
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper, RegexOptions.None);

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    string domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (ArgumentException e)
            {
                return false;
            }

            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
        }
    }
}