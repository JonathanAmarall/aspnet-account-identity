using Identity.Models;
using System.Threading.Tasks;

namespace Identity.Services
{
    namespace AspEmail.Services
    {
        public interface IEmailService
        {
            Task SendEmailAsync(EmailRequest emailRequest);
        }
    
    }
}
