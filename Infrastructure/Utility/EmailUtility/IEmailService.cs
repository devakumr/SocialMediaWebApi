using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Utility.EmailUtility
{
    public interface IEmailService
    {
        Task SendEmail(MailRequest mailRequest);
    }
}
