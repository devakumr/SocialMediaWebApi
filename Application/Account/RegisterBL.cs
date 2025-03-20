
using Infrastructure.Utility.EmailUtility;
using Infrastructure.Utility;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Persistence;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Domain.DtoClass;

namespace Application.Auth.Account
{
    public class RegisterBL
    {
        public class Command : IRequest<ApiResponse<string>>
        {
            public RegisterRequestDto param { get; set; }
        }

        public class Handler : IRequestHandler<Command, ApiResponse<string>>
        {
            private readonly UserManager<UserModel> _userManager;
            private readonly IEmailService _emailService;
            private readonly DataContext _dbContext;

            public Handler(UserManager<UserModel> userManager, IEmailService emailService, DataContext dbContext)
            {
                _userManager = userManager;
                _emailService = emailService;
                _dbContext = dbContext;
            }

            public async Task<ApiResponse<string>> Handle(Command request, CancellationToken cancellationToken)
            {
                var userRequest = request.param;

                // 🔍 Check if email already exists
                if (await _userManager.FindByEmailAsync(userRequest.Email) != null)
                {
                    return ApiResponseHelper.CreateErrorResponse("EMAIL_EXISTS", "Email is already in use.");
                }

                // 🔍 Validate password complexity
                if (!IsValidPassword(userRequest.Password))
                {
                    return ApiResponseHelper.CreateErrorResponse("WEAK_PASSWORD", "Password does not meet complexity requirements.");
                }

                // ✅ Sanitize input fields
                try
                {
                    userRequest.Email = SanitizationUtility.SanitizeInput(userRequest.Email);
                    userRequest.FirstName = SanitizationUtility.SanitizeInput(userRequest.FirstName);
                    userRequest.LastName = SanitizationUtility.SanitizeInput(userRequest.LastName);
                    userRequest.Gender = SanitizationUtility.SanitizeInput(userRequest.Gender);
                }
                catch (Exception ex)
                {
                    return ApiResponseHelper.CreateErrorResponse("SANITIZATION_ERROR", $"Sanitization error: {ex.Message}");
                }

                // 🔑 Generate OTP
                var newUser = new UserModel
                {
                    Email = userRequest.Email,
                    UserName = userRequest.Email,
                    FirstName = userRequest.FirstName,
                    LastName = userRequest.LastName,
                    Gender = userRequest.Gender,
                    UserStatus = 0, // 0 = Unverified
                    CreatedAt = DateTime.UtcNow
                };

                // 🔐 Create user in Identity
                var result = await _userManager.CreateAsync(newUser, userRequest.Password);
                if (!result.Succeeded)
                {
                    return ApiResponseHelper.CreateErrorResponse("REGISTRATION_FAILED", string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                // 🔢 Generate OTP
                var otp = OtpHelper.GenerateOtp(6);

                // ✅ Store OTP in `UserOTP` table
                var userOTP = new UserOTPModel
                {
                    UserId = newUser.Id,
                    Otp = otp,
                    ExpirationTime = DateTime.UtcNow.AddMinutes(5) // OTP expires in 5 mins
                };

                await _dbContext.UserOTP.AddAsync(userOTP);
                await _dbContext.SaveChangesAsync();

                // 🔐 Create user in Identity with hashed password
                var result1 = await _userManager.CreateAsync(newUser, userRequest.Password);
                if (!result1.Succeeded)
                {
                    return ApiResponseHelper.CreateErrorResponse("REGISTRATION_FAILED", string.Join(", ", result1.Errors.Select(e => e.Description)));
                }

                // 📩 Send OTP Email
                string emailResultMessage = await SendOtpEmail(userRequest.Email, otp, userRequest.FirstName);
                if (!string.IsNullOrEmpty(emailResultMessage))
                {
                    return ApiResponseHelper.CreateErrorResponse("EMAIL_SENDING_ERROR", $"Error sending OTP email: {emailResultMessage}");
                }

                return ApiResponseHelper.CreateSuccessResponse(userRequest.Email, "OTP sent successfully. Please verify your email.");
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
                    return null; // No error
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

            // 📧 Generate OTP Email Body
            private string GenerateEmailBody(string customerName, string otp)
            {
                return $@"
                    <div style='width:100%;background-color:#f4f4f4;padding:20px;'>
                        <h1>Hello {customerName},</h1>
                        <p>Thank you for registering.</p>
                        <p>Please use the following OTP to complete your registration:</p>
                        <h2>{otp}</h2>
                        <p>If you did not request this, please ignore this email.</p>
                    </div>";
            }

            // 🔑 Password Complexity Check
            private bool IsValidPassword(string password)
            {
                return password.Length >= 8
                    && password.Any(char.IsUpper)
                    && password.Any(char.IsLower)
                    && password.Any(char.IsDigit);
            }
        }
    }
}
