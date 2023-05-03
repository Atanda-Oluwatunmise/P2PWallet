using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using P2PWallet.Services.Interface;

namespace P2PWallet.Services.Services
{
    public class MailService : IMailService
    {
        private readonly EmailConfiguration _settings;
        private readonly IWebHostEnvironment _hostEnvironment;
        public MailService(EmailConfiguration settings, IWebHostEnvironment hostEnvironment)
        {
            _settings = settings;
            _hostEnvironment = hostEnvironment;
        }

        private async Task<bool> SendMail(string to, string subject, string body)
        {
            try
            {
                var mail = new MailMessage();
                mail.IsBodyHtml = true;
                mail.From = new MailAddress(_settings.From, _settings.DisplayName);
                mail.To.Add(to);
                mail.Subject = subject;
                mail.Body = body;

                var client = new SmtpClient();
                client.EnableSsl = _settings.UseSSL ? true : false;
                client.Host = _settings.Host;
                client.Port = _settings.Port;
                client.Credentials = new NetworkCredential(_settings.UserName, _settings.Password);
                client.UseDefaultCredentials = false;
                client.Send(mail);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public async Task<bool> SendAsync(string emailAdd, string name, string transtype, string amountt, string info, string details,  string acnt, string date, string balance, CancellationToken ct = default)
        {
            try
            {
                if (emailAdd == null) return false;
                var subject = "Transaction Alert";
                var PathToFile = _hostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                    + "emailtemplate" + Path.DirectorySeparatorChar.ToString() + "template.html";

                string HtmlBody = "";

                using (StreamReader streamReader = File.OpenText(PathToFile))
                {
                    HtmlBody = streamReader.ReadToEnd();

                    HtmlBody = HtmlBody.Replace("{name}", name);
                    HtmlBody = HtmlBody.Replace("{transtype}", transtype);
                    HtmlBody = HtmlBody.Replace("{amountt}", amountt);
                    HtmlBody = HtmlBody.Replace("{sendinfo}", info);
                    HtmlBody = HtmlBody.Replace("{details}", details);
                    HtmlBody = HtmlBody.Replace("{acnt}", acnt);
                    HtmlBody = HtmlBody.Replace("{date}", date);
                    HtmlBody = HtmlBody.Replace("{balance}", balance);

                }
                await SendMail(emailAdd, subject, HtmlBody); 

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> ResetPasswordEmailAsync(string email, string subjectBody, string emailbody, string emailbodyy, CancellationToken ct = default)
        {
            try
            {
                string HtmlBody = emailbody + emailbodyy;
                await SendMail(email, subjectBody, HtmlBody);
             
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
