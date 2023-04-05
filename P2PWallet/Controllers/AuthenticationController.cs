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

        [HttpPost("forgot-password")]
        public async Task<ServiceResponse<string>> ForgotPassword(string email)
        {
            var result = await _authService.ForgotPassword(email);    
            return result;
        }

        [HttpPost("change-password"), Authorize]
        public async Task<ServiceResponse<string>> ChangePassword(ChangePasswordDto changepassword)
        {

            var result = await _authService.ChangePassword(changepassword);
            return result;
        }
        [HttpPost("change-pin"), Authorize]
        public async Task<ServiceResponse<string>> ChangePin(ChangePinDto changepin)
        {
            var result = await _authService.ChangePin(changepin);
            return result;
        }

    }

}
