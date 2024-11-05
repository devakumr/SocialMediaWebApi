using System.Collections.Generic;
using System.Security.Claims;

namespace Infrastructure.Security
{
    public interface IJwtGenerator
    {
      
        string GenerateToken(string userId); // Add this line
    }
}
