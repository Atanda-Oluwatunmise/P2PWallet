using MailKit.Net.Smtp;
using MailKit.Security;
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
        public async Task<bool> SendAsync(string emailAdd, string name, string transtype, string amountt, string info, string details,  string acnt, string date, string balance, CancellationToken ct = default)
        {
            try
            {
                //Initialize a new instance of the MimeKit.MimeMessage class
                var mail = new MimeMessage();

                //Sender
                mail.From.Add(new MailboxAddress(_settings.DisplayName, _settings.From));
                mail.Sender = new MailboxAddress(_settings.DisplayName, _settings.From);

                //Receiver
                mail.To.Add(new MailboxAddress(_settings.DisplayName, emailAdd ?? _settings.To));

                //mail.

                //foreach (var mailAddress in mailData.To)
                //mail.To.Add(MailboxAddress.Parse(mailAddress));

                ////Set Reply to if specified in mail data
                //if (!string.IsNullOrEmpty(mailData.ReplyTo))
                //    mail.ReplyTo.Add(new MailboxAddress(mailData.ReplyToName, mailData.ReplyTo));

                ////BCC
                ////Check if a BCC was supplied in the request
                //if (mailData.Bcc != null)
                //{
                //    foreach (string mailAddress in mailData.Bcc.Where(x => !string.IsNullOrWhiteSpace(x)))
                //        mail.Bcc.Add(MailboxAddress.Parse(mailAddress.Trim()));
                //}

                ////CC
                ////Check if a BCC was supplied in the request
                //if (mailData.Cc != null)
                //{
                //    foreach (string mailAddress in mailData.Cc.Where(x => !string.IsNullOrWhiteSpace(x)))
                //        mail.Cc.Add(MailboxAddress.Parse(mailAddress.Trim()));
                //}

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


                //Content
                //Add content to mime message
                var body = new BodyBuilder();
                mail.Subject = _settings.Subject;
                body.HtmlBody = HtmlBody;
                //body.HtmlBody = PathToFile;
                mail.Body = body.ToMessageBody();
               

                using var smtp = new SmtpClient();

                if (_settings.UseSSL)
                {
                    await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.SslOnConnect, ct);
                }
                await smtp.AuthenticateAsync(_settings.UserName,_settings.Password, ct);
                await smtp.SendAsync(mail,ct);
                await smtp.DisconnectAsync(true, ct);

                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
        
        public async Task<bool> ResetPasswordEmailAsync(string email, string subjectBody, string emailbody, string emailbodyy, CancellationToken ct= default)
        {
            try
            {
                //Initialize a new instance of the MimeKit.MimeMessage class
                var mail = new MimeMessage();

                //Sender
                mail.From.Add(new MailboxAddress(_settings.DisplayName, _settings.From));
                mail.Sender = new MailboxAddress(_settings.DisplayName, _settings.From);

                //Receiver
                mail.To.Add(new MailboxAddress(_settings.DisplayName, email ?? _settings.To));

                string HtmlBody = emailbody + emailbodyy;

                //Content
                //Add content to mime message
                var body = new BodyBuilder();
                mail.Subject = subjectBody;
                body.HtmlBody = HtmlBody;
                //body.HtmlBody = PathToFile;
                mail.Body = body.ToMessageBody();
               

                using var smtp = new SmtpClient();

                if (_settings.UseSSL)
                {
                    await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.SslOnConnect, ct);
                }
                await smtp.AuthenticateAsync(_settings.UserName,_settings.Password, ct);
                await smtp.SendAsync(mail,ct);
                await smtp.DisconnectAsync(true, ct);

                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
