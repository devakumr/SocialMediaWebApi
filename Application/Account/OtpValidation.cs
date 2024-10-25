using Application.Auth.Account;
using Domain.Auth.Account;
using Infrastructure.Utility;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Application.Auth.Account
{
    public class OtpValidation
    {
        public class Command : IRequest<ApiResponse<string>>
        {
            public OtpValidationModel Param { get; set; }
        }

        public class Handler : IRequestHandler<Command, ApiResponse<string>>
        {
            private readonly DataContext _context;
            private readonly IDistributedCache _distributedCache;

            public Handler(DataContext context, IDistributedCache distributedCache)
            {
                _context = context;
                _distributedCache = distributedCache;
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
                    // Construct the cache keys for OTP and user data
                    var otpCacheKey = $"{providedEmail}_Otp";
                    var userCacheKey = $"{providedEmail}_UserData";

                    // Retrieve and decrypt OTP from cache
                    var encryptedOtp = await _distributedCache.GetStringAsync(otpCacheKey);
                    if (encryptedOtp == null)
                    {
                        return ApiResponseHelper.CreateErrorResponse("OTP_NOT_FOUND", "OTP not found or expired.");
                    }
                    var cachedOtp = EncryptionUtility.Decrypt(encryptedOtp);

                    if (providedOtp != cachedOtp)
                    {
                        return ApiResponseHelper.CreateErrorResponse("INVALID_OTP", "Invalid OTP.");
                    }

                    // Retrieve and decrypt user data from cache
                    var encryptedUserData = await _distributedCache.GetStringAsync(userCacheKey);
                    if (encryptedUserData == null)
                    {
                        return ApiResponseHelper.CreateErrorResponse("USER_NOT_FOUND", "User data not found or expired.");
                    }
                    var userData = EncryptionUtility.Decrypt(encryptedUserData);
                    var user = JsonConvert.DeserializeObject<UserModel>(userData);

                    // Validate the user information
                    if (user != null && providedEmail == user.Email)
                    {
                        user.Password= EncryptionUtility.Encrypt(user.Password);    
                        // Add the user to the database
                        _context.Users.Add(user);
                        await _context.SaveChangesAsync(cancellationToken);

                        // Clean up the cache after successful registration
                        await _distributedCache.RemoveAsync(otpCacheKey, cancellationToken);
                        await _distributedCache.RemoveAsync(userCacheKey, cancellationToken);

                        return ApiResponseHelper.CreateSuccessResponse("USER_REGISTERED", "User registered successfully.");
                    }

                    return ApiResponseHelper.CreateErrorResponse("REGISTRATION_FAILED", "Failed to register the user.");
                }
                catch (Exception ex)
                {
                    return ApiResponseHelper.CreateErrorResponse("VALIDATION_ERROR", $"An error occurred during OTP validation: {ex.Message}");
                }
            }
        }
    }
}
