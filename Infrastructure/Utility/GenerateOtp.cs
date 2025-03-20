using System;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Utility
{
    public static class OtpHelper
    {
        private const string Characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// Generates a secure OTP with letters and numbers.
        /// </summary>
        /// <param name="length">Length of the OTP (default is 6).</param>
        /// <returns>Returns a randomly generated OTP.</returns>
        public static string GenerateOtp(int length = 6)
        {
            if (length <= 0)
                throw new ArgumentException("OTP length must be greater than zero.");

            var otp = new StringBuilder(length);
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[length];

                rng.GetBytes(randomBytes);

                for (int i = 0; i < length; i++)
                {
                    otp.Append(Characters[randomBytes[i] % Characters.Length]);
                }
            }

            return otp.ToString();
        }

        /// <summary>
        /// Checks if the OTP is expired.
        /// </summary>
        /// <param name="otpGeneratedTime">Time when the OTP was created.</param>
        /// <param name="validityMinutes">Validity period of the OTP in minutes.</param>
        /// <returns>True if OTP is expired, otherwise false.</returns>
        public static bool IsExpired(DateTime otpGeneratedTime, int validityMinutes = 10)
        {
            return DateTime.UtcNow > otpGeneratedTime.AddMinutes(validityMinutes);
        }
    }
}
