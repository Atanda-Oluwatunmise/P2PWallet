using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using P2PWallet.Services.Interface;

namespace P2PWallet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController:ControllerBase
    {
       private readonly DataContext _dataContext;
       private readonly IAuthService _authService;

        public AuthenticationController(DataContext dataContext, IAuthService authService)
        {
            _dataContext = dataContext;
            _authService = authService;
        }

        [HttpPost("CreatePin"), Authorize]
        public async Task<ServiceResponse<string>> CreatePin(PinDto pin)
        {
            var result = await _authService.CreatePin(pin);
            return result;
        }

        [HttpPost("VerifyPin"), Authorize]
        public async Task<ServiceResponse<string>> VerifyPin(PinDto pin)
        {
            var result = await _authService.VerifyPin(pin);
            return result;
        }

    }


}
