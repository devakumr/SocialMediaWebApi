using Domain.Auth.Account;
using Infrastructure.Utility.EmailUtility;
using Infrastructure.Utility;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Persistence;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Application.Auth.Account
{


    public class Register
    {
        public class Command : IRequest<ApiResponse<string>>
        {
            public UserModel param { get; set; }
        }

        public class Handler : IRequestHandler<Command, ApiResponse<string>>
        {
            private readonly DataContext _context;
            private readonly IEmailService _emailService;
            private readonly IDistributedCache _distributedCache;

            public Handler(DataContext context, IEmailService emailService, IDistributedCache distributedCache)
            {
                _context = context;
                _emailService = emailService;
                _distributedCache = distributedCache;
            }

            public async Task<ApiResponse<string>> Handle(Command request, CancellationToken cancellationToken)
            {
                var userRequest = request.param;

                if (!IsValidEmail(userRequest.Email) || string.IsNullOrEmpty(userRequest.Password))
                {
                    return ApiResponseHelper.CreateErrorResponse("INVALID_INPUT", "Invalid email or password.");
                }

                if (await EmailExistsAsync(userRequest.Email))
                {
                    return ApiResponseHelper.CreateErrorResponse("EMAIL_EXISTS", "Email already exists.");
                }

                if (!IsValidPassword(userRequest.Password))
                {
                    return ApiResponseHelper.CreateErrorResponse("WEAK_PASSWORD", "Password does not meet complexity requirements.");
                }

                try
                {
                    userRequest.Email = SanitizationUtility.SanitizeInput(userRequest.Email);
                    userRequest.FirstName = SanitizationUtility.SanitizeInput(userRequest.FirstName);
                    userRequest.LastName = SanitizationUtility.SanitizeInput(userRequest.LastName);
                    userRequest.Password = SanitizationUtility.SanitizeInput(userRequest.Password);
                    userRequest.Gender = SanitizationUtility.SanitizeInput(userRequest.Gender);
                }
                catch (Exception ex)
                {
                    return ApiResponseHelper.CreateErrorResponse("SANITIZATION_ERROR", $"Sanitization error: {ex.Message}");
                }

                try
                {
                    var otp = GenerateOtp(6);
                    string emailResultMessage = await SendOtpEmail(userRequest.Email, otp, userRequest.FirstName);

                    if (!string.IsNullOrEmpty(emailResultMessage))
                    {
                        return ApiResponseHelper.CreateErrorResponse("EMAIL_SENDING_ERROR", $"Error sending OTP email: {emailResultMessage}");
                    }






                    var encryptedUserData = EncryptionUtility.Encrypt(JsonConvert.SerializeObject(userRequest));
                    var encryptedRegirationOtp = EncryptionUtility.Encrypt(otp);

                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    };

                    await _distributedCache.SetStringAsync(userRequest.Email + "_UserData", encryptedUserData, cacheOptions);
                    await _distributedCache.SetStringAsync(userRequest.Email + "_Otp", encryptedRegirationOtp, cacheOptions);

                    return ApiResponseHelper.CreateSuccessResponse(userRequest.Email, "OTP sent successfully. Please verify your email.");
                }
                catch (Exception ex)
                {
                    return ApiResponseHelper.CreateErrorResponse("OTP_ERROR", $"Error generating or sending OTP: {ex.Message}");
                }
            }

            private async Task<bool> EmailExistsAsync(string email)
            {
                return await _context.Users.AnyAsync(u => u.Email == email);
            }

            private string GenerateOtp(int length)
            {
                const string chars = "0123456789";
                Random random = new Random();
                return new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }

            private async Task<string> SendOtpEmail(string email, string otp, string customerName)
            {
                try
                {
                    var mailRequest = new MailRequest
                    {
                        Email = email,
                        EmailSubject = "OTP Verification",
                        EmailBody = GenerateEmailBody(customerName, otp)
                    };
                    await _emailService.SendEmail(mailRequest);
                    return null;
                }
                catch (SmtpException smtpEx)
                {
                    return $"Email sending failed: {smtpEx.Message}";
                }
                catch (Exception ex)
                {
                    return $"An error occurred while sending email: {ex.Message}";
                }
            }

            private string GenerateEmailBody(string customerName, string otp)
            {
                return $"<div style='width:100%;background-color:#f4f4f4;padding:20px;'><h1>Hello {customerName},</h1><p>Thank you for registering.</p><p>Please use the following OTP to complete your registration:</p><h2>{otp}</h2><p>If you did not request this, please ignore this email.</p></div>";
            }

            private bool IsValidEmail(string email)
            {
                try
                {
                    var addr = new MailAddress(email);
                    return addr.Address == email;
                }
                catch
                {
                    return false;
                }
            }

            private bool IsValidPassword(string password)
            {
                return password.Length >= 8 && password.Any(char.IsUpper) && password.Any(char.IsLower) && password.Any(char.IsDigit);
            }
        }
    }
}
