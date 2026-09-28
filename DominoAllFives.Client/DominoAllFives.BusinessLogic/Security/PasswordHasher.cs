using System;
using System.Security.Cryptography;

namespace DominoAllFives.BusinessLogic.Security
{
    /// <summary>
    /// Provides password hashing and verification using PBKDF2-HMAC-SHA256.
    /// </summary>
    public static class PasswordHasher
    {
        private const string Algorithm = "PBKDF2-SHA256";
        private const int Iterations = 100000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException(
                    "Password cannot be null or empty.",
                    nameof(password));
            }

            byte[] salt = new byte[SaltSize];

            using (RandomNumberGenerator randomNumberGenerator =
                RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(salt);
            }

            byte[] hash;

            using (Rfc2898DeriveBytes deriveBytes =
                new Rfc2898DeriveBytes(
                    password,
                    salt,
                    Iterations,
                    HashAlgorithmName.SHA256))
            {
                hash = deriveBytes.GetBytes(HashSize);
            }

            return string.Format(
                "{0}${1}${2}${3}",
                Algorithm,
                Iterations,
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }

        public static bool VerifyPassword(
            string password,
            string storedPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(storedPasswordHash))
            {
                return false;
            }

            string[] parts = storedPasswordHash.Split('$');

            if (parts.Length != 4 ||
                parts[0] != Algorithm ||
                !int.TryParse(parts[1], out int iterations))
            {
                return false;
            }

            try
            {
                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] expectedHash = Convert.FromBase64String(parts[3]);

                byte[] actualHash;

                using (Rfc2898DeriveBytes deriveBytes =
                    new Rfc2898DeriveBytes(
                        password,
                        salt,
                        iterations,
                        HashAlgorithmName.SHA256))
                {
                    actualHash = deriveBytes.GetBytes(expectedHash.Length);
                }

                return SlowEquals(expectedHash, actualHash);
            }
            catch (FormatException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private static bool SlowEquals(
            byte[] firstHash,
            byte[] secondHash)
        {
            if (firstHash.Length != secondHash.Length)
            {
                return false;
            }

            int difference = 0;

            for (int index = 0; index < firstHash.Length; index++)
            {
                difference |= firstHash[index] ^ secondHash[index];
            }

            return difference == 0;
        }
    }
}