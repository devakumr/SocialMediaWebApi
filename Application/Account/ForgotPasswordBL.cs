using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Auth.Account;
using Infrastructure.Utility;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Domain.DtoClass;
using Infrastructure.Utility.EmailUtility;

namespace Application.Account
{
    public class ForgotPasswordBL
    {
        public class Command : IRequest<ApiResponse<string>>
        {
            public  ForgotPasswordParam Param { get; set; }
        }

        public class Handler : IRequestHandler<Command, ApiResponse<string>>
        {
            private readonly DataContext _context;
            private readonly IEmailService _emailService;

            public Handler(DataContext context, IEmailService emailService)
            {
                _context = context;
                _emailService = emailService;
            }

            public async Task<ApiResponse<string>> Handle(Command request, CancellationToken cancellationToken)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Param.Email, cancellationToken);
                if (user == null)
                {
                    return ApiResponseHelper.CreateErrorResponse("USER_NOT_FOUND", "User with this email not found.");
                }

                // Generate OTP
                string otp = OtpHelper.GenerateOtp();
                DateTime expirationTime = DateTime.UtcNow.AddMinutes(10);

                // Save OTP in the database
                var userOtp = new UserOTPModel
                {
                    UserId = user.Id,
                    Otp = otp,
                    ExpirationTime = expirationTime,
                    IsUsed = false
                };

                _context.UserOTP.Add(userOtp);
                await _context.SaveChangesAsync(cancellationToken);

                // Send OTP via Email
                string subject = "Password Reset OTP";
                string body = $"Your OTP for password reset is: {otp}. It is valid for 10 minutes.";

                await _emailService.SendEmail(new MailRequest
                {
                    Email = request.Param.Email,
                    EmailSubject = subject,
                    EmailBody = body
                });

                return ApiResponseHelper.CreateSuccessResponse("OTP_SENT", "OTP has been sent to your email.");
            }
        }
    }
    public class ForgotPasswordParam
    {
        public string Email { get; set; }
    }
}
