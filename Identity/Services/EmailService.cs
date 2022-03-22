using Identity.Models;
using Identity.Setup;
using Microsoft.Extensions.Options;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Identity.Services
{
    namespace AspEmail.Services
    {
        public class EmailService : IEmailService
        {
            private readonly EmailConfig _emailConfig;
            public EmailService(IOptions<EmailConfig> emailConfig)
            {
                _emailConfig = emailConfig.Value;
            }
            public async Task SendEmailAsync(EmailRequest mailRequest)
            {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();

                message.From = new MailAddress(_emailConfig.Mail, _emailConfig.DisplayName);
                message.To.Add(new MailAddress(mailRequest.ToEmail));
                message.Subject = mailRequest.Subject;

                if (mailRequest.Attachments != null)
                {
                    foreach (var file in mailRequest.Attachments)
                    {
                        if (file.Length > 0)


                            using (var ms = new MemoryStream())
                            {
                                file.CopyTo(ms);
                                var fileBytes = ms.ToArray();
                                Attachment att = new Attachment(new MemoryStream(fileBytes), file.FileName);
                                message.Attachments.Add(att);
                            }
                    }
                }

                message.IsBodyHtml = false;
                message.Body = mailRequest.Body;
                smtp.Port = _emailConfig.Port;
                smtp.Host = _emailConfig.Host;
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(_emailConfig.UserName, _emailConfig.Password);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                await smtp.SendMailAsync(message);
            }
        }
    
    }

}
