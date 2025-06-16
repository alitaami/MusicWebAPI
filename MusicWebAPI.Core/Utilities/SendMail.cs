using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utilities
{
    public class SendMail
    {
        private static readonly string _emailPort = Environment.GetEnvironmentVariable("EMAIL_PORT");
        private static readonly string _emailSender = Environment.GetEnvironmentVariable("EMAIL_SENDER");
        private static readonly string _emailHost = Environment.GetEnvironmentVariable("EMAIL_HOST");
        private static readonly string _emailPass = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");

        public static async Task SendAsync(string to, string subject, string body)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(_emailSender, "MusicWebAPI");
                mail.To.Add(to);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;

                using (SmtpClient smtpServer = new SmtpClient(_emailHost))
                {
                    smtpServer.UseDefaultCredentials = false;
                    smtpServer.Port = int.TryParse(_emailPort, out int port) ? port : 587;
                    smtpServer.Credentials = new NetworkCredential(_emailSender, _emailPass);
                    smtpServer.EnableSsl = true;

                    try
                    {
                        await smtpServer.SendMailAsync(mail);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }
        }
    }
}
