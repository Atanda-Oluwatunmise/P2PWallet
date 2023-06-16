using Microsoft.AspNetCore.Http;
using P2PWallet.Models.Models.DataObjects;


namespace P2PWallet.Services.Interface
{
    public interface IMailService
    {
        public Task<bool> SendAsync(string emailAdd, string name, string transtype, string amountt, string info, string details,  string acnt, string date, string balance, CancellationToken ct = default);
        public Task<bool> ResetPasswordEmailAsync(string email, string subjectBody, string emailbody, string emailbodyy, CancellationToken ct = default);
        public Task<bool> SendStatementToEmail(string email, string subjectBody, string emailBody, IFormFile formFile, CancellationToken ct = default);


    }
}
