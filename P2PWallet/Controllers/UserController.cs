using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using P2PWallet.Services;
using P2PWallet.Services.Interface;


namespace P2PWallet.Api.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    { 

        private readonly DataContext _dataContext;
        private readonly IUserServices _userServices;
        public UserController(DataContext dataContext, IUserServices userServices)
        {
            _dataContext = dataContext;
            _userServices = userServices;

        }

        [HttpPost("Register")]
        public async Task<ServiceResponse<UserViewModel>> Register(UserDto user)
        {
           var result =  await _userServices.Register(user);
           return result;
           
        }
            
        [HttpPost("Login")]
        public async Task<ServiceResponse<LoginView>> Login(LoginDto loginreq)
        {
             var result = await _userServices.Login(loginreq);
             return result;
        }

        [HttpPost("UserDetails")]
        public async Task<ServiceResponse<List<SearchAccountDetails>>> GetUserDetails(UserSearchDto userSearch)
        {
            var result = await _userServices.GetUserDetails(userSearch);
            return result;
        }


        [HttpGet("AccountDetails"), Authorize]
        public async Task<ServiceResponse<List<AccountDetails>>> GetMyAccountNumber()
        {
            var result = await _userServices.GetMyAccountNumber();
            return result;
        }
    }
}
