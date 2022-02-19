using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Authentication.Core.Utilities
{
    public static class HashUtil
    {
        public static string HashPassword(string password, byte[] salt)
        {
            var hash = GetHash(password, salt);
            var hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);
            return Convert.ToBase64String(hashBytes);
        }

        public static byte[] GetSalt()
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
            return salt;
        }

        public static bool Validate(string password, string userHashedPassword)
        {
            var hashBytes = Convert.FromBase64String(userHashedPassword);
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            var hash = GetHash(password, salt);

            for (var i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static byte[] GetHash(string password, byte[] salt)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            return pbkdf2.GetBytes(20);
        }
    }
}