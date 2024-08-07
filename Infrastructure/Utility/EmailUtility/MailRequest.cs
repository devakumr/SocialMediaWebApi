using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Utility.EmailUtility
{
    public class MailRequest
    {

        public string Email { get; set; }

        public string EmailSubject { get; set; }

        public string EmailBody { get; set; }
    }
}
