using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Security
{
    public interface IUserAccessor
    {
        string GetCurrentUserId(); // Example method
                                   // Other user access methods
    }
}
