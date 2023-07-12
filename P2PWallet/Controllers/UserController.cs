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
        private readonly IMultipleWallets _multipleWallets;
        public UserController(DataContext dataContext, IUserServices userServices, IMultipleWallets multipleWallets)
        {
            _dataContext = dataContext;
            _userServices = userServices;
            _multipleWallets = multipleWallets;
        }

        [HttpPost("register")]
        public async Task<ServiceResponse<UserViewModel>> Register(UserDto user)
        {
           var result =  await _userServices.Register(user);
           return result;
           
        }
            
        [HttpPost("login")]
        public async Task<ServiceResponse<LoginView>> Login(LoginDto loginreq)
        {
             var result = await _userServices.Login(loginreq);
             return result;
        }

        [HttpPost("userdetails")]
        public async Task<ServiceResponse<List<SearchAccountDetails>>> GetUserDetails(UserSearchDto userSearch)
        {
            var result = await _userServices.GetUserDetails(userSearch);
            return result;
        }
        
        [HttpPost("foreignuserdetails")]
        public async Task<ServiceResponse<List<SearchAccountDetails>>> GetUserForeignDetails(ForeignUserSearchDto userSearch)
        {
            var result = await _userServices.GetUserForeignDetails(userSearch);
            return result;
        }


        [HttpGet("accountdetails"), Authorize]
        public async Task<ServiceResponse<List<AccountDetails>>> GetMyAccountNumber()
        {
            var result = await _userServices.GetMyAccountNumber();
            return result;
        }

        [HttpPut("editinfo"), Authorize]
        public async Task<ServiceResponse<List<EditViewModel>>> EditUserInfo(EditViewModel request)
        {
            var result = await _userServices.EditUserInfo(request);
            return result;
        }

        [HttpPost("uploadimage"), Authorize]
        public async Task<ServiceResponse<string>> SaveImage([FromForm]ImageViewmodel imageview)
        {
            var result = await _userServices.SaveImage(imageview);
            return result;
        }

        [HttpGet("displayimage"), Authorize]
        public async Task<DisplayViewmodel> DisplayImage()
        {
            var result = await _userServices.DisplayImage();
            return result;
        }
        [HttpDelete("deleteimage"), Authorize]
        public async Task<ServiceResponse<string>> DeleteImage()
        {
            var result = await _userServices.DeleteImage();
            return result;
        }

        [HttpPost("createnewwallet"), Authorize]
        public async Task<ServiceResponse<String>> CreateNewAccountWallet(CurrencyObj currencyObj)
        {
            var result = await _multipleWallets.CreateNewAccountWallet(currencyObj);
            return result;
        }

        [HttpPost("verifycurrency"), Authorize]
        public async Task<ServiceResponse<String>> VerifyCurrency(CurrencyObj currencyObj)
        {
            var result = await _multipleWallets.VerifyCurrency(currencyObj);
            return result;
        }

        [HttpPost("verifyaccount"), Authorize]
        public async Task<ServiceResponse<List<WalletResponseView>>> VerifyAccount(CurrencyObj currencyObj)
        {
            var result = await _multipleWallets.VerifyAccount(currencyObj);
            return result;
        }
        //[HttpGet("verifyimagestatus"), Authorize]
        //public async Task<bool> VerifyImageStatus()
        //{
        //    var result = await _userServices.VerifyImageStatus();
        //    return result;
        //}



        //[HttpPost("refreshtoken"), Authorize]
        //public async Task<ServiceResponse<TokenApiDto>> Refresh(TokenApiDto tokenApiDto)
        //{
        //    var result = await _userServices.Refresh(tokenApiDto);
        //    return result;
        //}

    }
}
