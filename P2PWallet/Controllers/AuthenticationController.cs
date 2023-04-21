using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using P2PWallet.Services.Interface;
using P2PWallet.Services.Services;
using System.ComponentModel.DataAnnotations;

namespace P2PWallet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController:ControllerBase
    {
       private readonly DataContext _dataContext;
       private readonly IAuthService _authService;
        private readonly IUserServices _userServices;

        public AuthenticationController(DataContext dataContext, IAuthService authService, IUserServices userServices)
        {
            _dataContext = dataContext;
            _authService = authService;
            _userServices = userServices;
        }

        [HttpPost("createpin"), Authorize]
        public async Task<ServiceResponse<string>> CreatePin(PinDto pin)
        {
            var result = await _authService.CreatePin(pin);
            return result;
        }

        [HttpPost("verifypin"), Authorize]
        public async Task<ServiceResponse<string>> VerifyPin(PinDto pin)
        {
            var result = await _authService.VerifyPin(pin);
            return result;
        }

        [HttpGet("validateuser"), Authorize]
        public async Task<ServiceResponse<string>> ValidateUser()
        {
            var result = await _authService.ValidateUser();
            return result;
        }

        [HttpPost("forgotpassword")]
        public async Task<ServiceResponse<string>> ForgotPassword(EmailDto emaill)
        {
            var result = await _authService.ForgotPassword(emaill);    
            return result;
        }

        [HttpPost("resetpassword")]
        public async Task<ServiceResponse<string>> ResetPassword(ResetPasswordRequest request)
        {
            var result = await _authService.ResetPassword(request);    
            return result;
        }

        [HttpPost("forgotpin")]
        public async Task<ServiceResponse<string>> ForgotPin(EmailDto emaill)
        {
            var result = await _authService.ForgotPin(emaill);
            return result;
        }

        [HttpPost("resetpin")]
        public async Task<ServiceResponse<string>> ResetPin(ResetPinRequest request)
        {
            var result = await _authService.ResetPin(request);
            return result;
        }

        [HttpPost("changepassword"), Authorize]
        public async Task<ServiceResponse<string>> ChangePassword(ChangePasswordDto changepassword)
        {

            var result = await _authService.ChangePassword(changepassword);
            return result;
        }
        [HttpPost("changepin"), Authorize]
        public async Task<ServiceResponse<string>> ChangePin(ChangePinDto changepin)
        {
            var result = await _authService.ChangePin(changepin);
            return result;
        }


        [HttpPost("addsecuritydetail"), Authorize]
        public async Task<ServiceResponse<string>> AddSecurityDetails(SecurityQuestionDto details)
        {
            var result = await _authService.AddSecurityDetails(details);
            return result;
        }

        [HttpGet("getsecuritydetail"), Authorize]
        public async Task<ServiceResponse<SecurityViewModel>> GetSecuritydetail()
        {
            var result = await _authService.GetSecuritydetail();
            return result;
        }


    }

}
