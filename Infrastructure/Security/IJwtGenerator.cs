using System.Collections.Generic;
using System.Security.Claims;

namespace Infrastructure.Security
{
    public interface IJwtGenerator
    {
      
        string GenerateToken(string email); // Add this line
    }
}
