using Domain.Auth.Account;
using Infrastructure.Security; // Import the security namespace for JWT
using Infrastructure.Utility;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Auth.Account
{
    public class UserLogin
    {
        public class Command : IRequest<ApiResponse<string>>
        {
            public required UserLoginModel Param { get; set; }
        }

        public class Handler : IRequestHandler<Command, ApiResponse<string>>
        {
            private readonly DataContext _context;
            private readonly IJwtGenerator _jwtGenerator; // Add JWT generator interface

            public Handler(DataContext context, IJwtGenerator jwtGenerator)
            {
                _context = context;
                _jwtGenerator = jwtGenerator; // Initialize the JWT generator
            }

            public async Task<ApiResponse<string>> Handle(Command request, CancellationToken cancellationToken)
            {
                var userRequest = request.Param;

                // Validate email and password fields
                if (string.IsNullOrWhiteSpace(userRequest.Email) || string.IsNullOrWhiteSpace(userRequest.Password))
                {
                    return ApiResponseHelper.CreateErrorResponse("VALIDATION_ERROR", "Email and password are required.");
                }

                // Sanitize email and password inputs
                userRequest.Email = SanitizationUtility.SanitizeInput(userRequest.Email);
                userRequest.Password = SanitizationUtility.SanitizeInput(userRequest.Password);

                // Check if the user is registered in the database
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userRequest.Email, cancellationToken);

                if (user == null)
                {
                    return ApiResponseHelper.CreateErrorResponse("LOGIN_ERROR", "Invalid email or password.");
                }

                var userpassword = EncryptionUtility.Decrypt(user.Password);
                // If the user is found but the password is incorrect
                if (userpassword != userRequest.Password)
                {
                    return ApiResponseHelper.CreateErrorResponse("LOGIN_ERROR", "Invalid email or password.");
                }

                try
                {
                    // Generate a JWT token for the user upon successful login
                    var token = _jwtGenerator.GenerateToken(user.Id?.ToString());// You may use user.Id for unique ID

                    if (token == null)
                    {
                        return ApiResponseHelper.CreateErrorResponse("TOKEN_GENERATION_ERROR", "Token generation failed.");
                    }

                    // Return the token in the success response
                    return ApiResponseHelper.CreateSuccessResponse(token, "Login successful");
                }
                catch (Exception ex)
                {
                    // Log the exception (you can use a logging framework here)
                    // For example: _logger.LogError(ex, "Error generating token for user {Email}", user.Email);

                    return ApiResponseHelper.CreateErrorResponse("TOKEN_GENERATION_ERROR", "Error generating token");
                }
            }
        }
    }
}
