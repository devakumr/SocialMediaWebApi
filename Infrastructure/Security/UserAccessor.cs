using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Security
{
    public class UserAccessor : IUserAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentUserId()
        {
            // Get the current HttpContext
            var context = _httpContextAccessor.HttpContext;

            // If there's no context, return null or throw an exception
            if (context == null) return null;

            // Get the user claims
            var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            return userIdClaim; // Return the user ID from claims
        }
    }
}
