using Application.Auth.Account;
using Infrastructure.Utility;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.DtoClass;

namespace Application.Auth.Account
{
    public class OtpValidationBL
    {
        public class Command : IRequest<ApiResponse<string>>
        {
            public OtpValidationDto Param { get; set; }
        }

        public class Handler : IRequestHandler<Command, ApiResponse<string>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<ApiResponse<string>> Handle(Command request, CancellationToken cancellationToken)
            {
                var userOtp = request.Param;
                var providedEmail = userOtp.Email;
                var providedOtp = userOtp.Otp;

                if (string.IsNullOrEmpty(providedEmail) || string.IsNullOrEmpty(providedOtp))
                {
                    return ApiResponseHelper.CreateErrorResponse("INVALID_INPUT", "Email or OTP cannot be null or empty.");
                }

                try
                {
                    // Retrieve user from Users table
                    var user = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == providedEmail, cancellationToken);

                    if (user == null)
                    {
                        return ApiResponseHelper.CreateErrorResponse("USER_NOT_FOUND", "User not found.");
                    }

                    // Retrieve the latest unused OTP from UserOTP table using UserId
                    var userOtpRecord = await _context.UserOTP
                        .Where(o => o.UserId == user.Id && !o.IsUsed)
                        .OrderByDescending(o => o.CreatedAt) // Get latest OTP
                        .FirstOrDefaultAsync(cancellationToken);

                    if (userOtpRecord == null || userOtpRecord.Otp != providedOtp)
                    {
                        return ApiResponseHelper.CreateErrorResponse("INVALID_OTP", "Incorrect or expired OTP.");
                    }

                    // Check if OTP is expired
                    if (OtpHelper.IsExpired(userOtpRecord.ExpirationTime))
                    {
                        return ApiResponseHelper.CreateErrorResponse("EXPIRED_OTP", "OTP has expired.");
                    }


                    // Mark user as verified
                    user.UserStatus = 1; // 1 = Verified
                    _context.Users.Update(user);

                    // Mark OTP as used instead of deleting
                    userOtpRecord.IsUsed = true;
                    _context.UserOTP.Update(userOtpRecord);

                    await _context.SaveChangesAsync(cancellationToken);

                    return ApiResponseHelper.CreateSuccessResponse("VERIFIED", "OTP verification successful.");
                }
                catch (Exception ex)
                {
                    return ApiResponseHelper.CreateErrorResponse("VALIDATION_ERROR", $"An error occurred: {ex.Message}");
                }
            }
        }
    }
}
