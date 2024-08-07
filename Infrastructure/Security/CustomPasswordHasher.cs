using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Security
{
    public class CustomPasswordHasher<T> : IPasswordHasher<T> where T : class
    {
        private const int Iterations = 100_000; // Number of iterations for PBKDF2
        private const int SaltSize = 16; // Size of the salt in bytes
        private const int HashSize = 32; // Size of the hash in bytes

        public string HashPassword(T user, string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            }

            // Generate a salt
            var salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Generate the hash using PBKDF2 with SHA-256
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                var hash = pbkdf2.GetBytes(HashSize);

                // Combine salt and hash into one array
                var hashBytes = new byte[SaltSize + HashSize];
                Array.Copy(salt, 0, hashBytes, 0, SaltSize);
                Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

                // Convert to base64
                return Convert.ToBase64String(hashBytes);
            }
        }

        public PasswordVerificationResult VerifyHashedPassword(T user, string hashedPassword, string providedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(providedPassword))
            {
                return PasswordVerificationResult.Failed;
            }

            try
            {
                // Extract salt and hash from the stored hash
                var hashBytes = Convert.FromBase64String(hashedPassword);
                if (hashBytes.Length != SaltSize + HashSize)
                {
                    return PasswordVerificationResult.Failed;
                }

                var salt = new byte[SaltSize];
                var storedHash = new byte[HashSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);
                Array.Copy(hashBytes, SaltSize, storedHash, 0, HashSize);

                // Compute the hash with the provided password
                using (var pbkdf2 = new Rfc2898DeriveBytes(providedPassword, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    var computedHash = pbkdf2.GetBytes(HashSize);

                    // Compare the computed hash with the stored hash
                    for (int i = 0; i < HashSize; i++)
                    {
                        if (storedHash[i] != computedHash[i])
                        {
                            return PasswordVerificationResult.Failed;
                        }
                    }

                    return PasswordVerificationResult.Success;
                }
            }
            catch
            {
                return PasswordVerificationResult.Failed;
            }
        }
    }
}
