using Infrastructure.Security; // Ensure this namespace exists and has IUserAccessor
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Account
{
    public class validateJwtTokenBL
    {
        public class Command : IRequest<string>
        {
            public ValidateJwtTokenParam Param { get; set; }
        }

        public class Handler : IRequestHandler<Command, string>
        {
            private readonly IUserAccessor _userAccessor;
            private readonly JwtSettings _jwtSettings;

            public Handler(IUserAccessor userAccessor, IOptions<JwtSettings> jwtSettings)
            {
                _userAccessor = userAccessor;
                _jwtSettings = jwtSettings.Value;
            }

            public async Task<string> Handle(Command request, CancellationToken cancellationToken)
            {
                if (string.IsNullOrWhiteSpace(request.Param.Token))
                {
                    throw new ArgumentNullException(nameof(request.Param.Token), "Token cannot be null or empty.");
                }

                if (string.IsNullOrWhiteSpace(_jwtSettings.Secret))
                {
                    throw new ArgumentNullException(nameof(_jwtSettings.Secret), "JWT Secret is not configured.");
                }

                var token = request.Param.Token;

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidAudience = _jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret))
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                SecurityToken validatedToken;

                try
                {
                    ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                    var userIdClaim = principal.FindFirst(ClaimTypes.Email)?.Value;

                    // Return success message along with user ID in a structured format
                    return "valid" ;
                }
                catch (SecurityTokenExpiredException)
                {
                    throw new SecurityTokenException("Token has expired.");
                }
                catch (SecurityTokenInvalidSignatureException)
                {
                    throw new SecurityTokenException("Token has an invalid signature.");
                }
                catch (Exception ex)
                {
                    throw new SecurityTokenException($"Token validation failed: {ex.Message}");
                }
            }
        }

        public class ValidateJwtTokenParam
        {
            public string Token { get; set; }
        }

        public class JwtSettings
        {
            public string Secret { get; set; }
            public string Issuer { get; set; }
            public string Audience { get; set; }
            public int TokenExpiryInMinutes { get; set; }
        }

        // Response class for token validation
      
    }
}
