using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using P2PWallet.Services;
using P2PWallet.Services.Services;


namespace P2PWallet.Api.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    { 

        private readonly DataContext _dataContext;
        private readonly IP2PWalletServices _p2PWalletServices;
        public UserController(DataContext dataContext, IP2PWalletServices p2PWalletServices)
        {
            _dataContext = dataContext;
            _p2PWalletServices = p2PWalletServices;

        }
        //[HttpGet, Authorize]
        //public ActionResult<string> GetMe() {
        //    var userName = _p2PWalletServices.GetMyName();
        //    return Ok(userName);
        //}

        [HttpPost("Register")]
        public async Task<ServiceResponse<UserViewModel>> Register(UserDto user)
        {
           var result =  await _p2PWalletServices.Register(user);
           return result;
           
        }
            
        [HttpPost("Login")]
        public async Task<ServiceResponse<string>> Login(LoginDto loginreq)
        {
             var result = await _p2PWalletServices.Login(loginreq);
             return result;
        }

        [HttpGet("AccountNumber"), Authorize]
        public ActionResult<string> GetNumber()
        {
            var accountNumber = _p2PWalletServices.GetMyAccountNumber();
            return Ok(accountNumber);
        }



    }
}
