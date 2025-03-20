using Domain.DtoClass;
using Infrastructure.Security;
using Infrastructure.Utility;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Auth.Account
{
    public class UserLoginBL
    {
        public class Command : IRequest<ApiResponse<string>>
        {
            public required UserLoginDto Param { get; set; }
        }

        public class Handler : IRequestHandler<Command, ApiResponse<string>>
        {
            private readonly UserManager<IdentityUser> _userManager;
            private readonly SignInManager<IdentityUser> _signInManager;
            private readonly IJwtGenerator _jwtGenerator;

            public Handler(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IJwtGenerator jwtGenerator)
            {
                _userManager = userManager;
                _signInManager = signInManager;
                _jwtGenerator = jwtGenerator;
            }

            public async Task<ApiResponse<string>> Handle(Command request, CancellationToken cancellationToken)
            {
                var userRequest = request.Param;

                // Validate input fields
                if (string.IsNullOrWhiteSpace(userRequest.Email) || string.IsNullOrWhiteSpace(userRequest.Password))
                {
                    return ApiResponseHelper.CreateErrorResponse("VALIDATION_ERROR", "Email and password are required.");
                }

                // Find user by email
                var user = await _userManager.FindByEmailAsync(userRequest.Email);
                if (user == null)
                {
                    return ApiResponseHelper.CreateErrorResponse("LOGIN_ERROR", "Invalid email or password.");
                }

                // Ensure user is verified
                if (!user.EmailConfirmed)
                {
                    return ApiResponseHelper.CreateErrorResponse("ACCOUNT_NOT_VERIFIED", "Your account is not verified. Please check your email.");
                }

                // Validate password using SignInManager
                var signInResult = await _signInManager.CheckPasswordSignInAsync(user, userRequest.Password, lockoutOnFailure: false);
                if (!signInResult.Succeeded)
                {
                    return ApiResponseHelper.CreateErrorResponse("LOGIN_ERROR", "Invalid email or password.");
                }

                try
                {
                    // Generate JWT token with user claims
                    var token = _jwtGenerator.GenerateToken(user.Id);

                    return ApiResponseHelper.CreateSuccessResponse(token, "Login successful");
                }
                catch (Exception ex)
                {
                    return ApiResponseHelper.CreateErrorResponse("TOKEN_GENERATION_ERROR", $"Error generating token: {ex.Message}");
                }
            }
        }
    }
}
