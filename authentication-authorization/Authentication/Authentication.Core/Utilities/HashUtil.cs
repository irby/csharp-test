using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Authentication.Core.Utilities
{
    public static class HashUtil
    {
        private const int HashSize = 64;
        private const int SaltSize = 32;
        
        public static string HashPassword(string password)
        {
            var salt = GetSalt();
            var hash = GetHash(password, salt);
            var hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);
            return Convert.ToBase64String(hashBytes);
        }

        public static bool Validate(string password, string userHashedPassword)
        {
            var hashBytes = Convert.FromBase64String(userHashedPassword);
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            var hash = GetHash(password, salt);

            for (var i = 0; i < HashSize; i++)
            {
                if (hashBytes[i + SaltSize] != hash[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static byte[] GetHash(string password, byte[] salt)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            return pbkdf2.GetBytes(HashSize);
        }
        
        private static byte[] GetSalt()
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[SaltSize]);
            return salt;
        }
    }
}