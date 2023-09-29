using Microsoft.AspNetCore.Mvc;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Interface
{
    public interface IAuthService
    {
        public Task<ServiceResponse<string>> CreatePin(PinDto pin);
        public Task<ServiceResponse<string>> VerifyPin(PinDto pin);
        public Task<ServiceResponse<string>> ValidateUser();
        public Task<ServiceResponse<string>> ForgotPassword(EmailDto emaill);
        Task<ServiceResponse<string>> ForgotPasswordAng(EmailDto emaill);

        public Task<ServiceResponse<string>> ForgotPin(EmailDto emaill);
        public Task<ServiceResponse<string>> ChangePassword(ChangePasswordDto changepassword);
        public Task<ServiceResponse<string>> ChangePin(ChangePinDto changepin);
        //public Task<List<SecurityQuestionView>> GetAllQuestions();
        public Task<ServiceResponse<string>> AddSecurityDetails(SecurityQuestionDto details);
        public Task<ServiceResponse<string>> ResetPassword(ResetPasswordRequest request);
        public Task<ServiceResponse<string>> ResetPin(ResetPinRequest request);
        public Task<ServiceResponse<SecurityViewModel>> GetSecuritydetail();


    }
}
