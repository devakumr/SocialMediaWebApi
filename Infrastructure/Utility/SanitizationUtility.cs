using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Utility
{
    public static class SanitizationUtility
    {
        public static string SanitizeInput(string input)
        {
            return input.Replace("<", "&lt;")
                        .Replace(">", "&gt;")
                        .Replace("&", "&amp;")
                        .Replace("\"", "&quot;")
                        .Replace("'", "&#39;");
        }
    }
}
