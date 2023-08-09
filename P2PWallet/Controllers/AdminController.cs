using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using P2PWallet.Services.Interface;

namespace P2PWallet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController: ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IAdminService _adminService;

        public AdminController(DataContext dataContext, IAdminService adminService)
        {
            _dataContext = dataContext;
            _adminService = adminService;
        }

        [HttpPost("adminlogin")]
        public async Task<ServiceResponse<LoginView>> Login(LoginDto loginreq)
        {
            var result = await _adminService.Login(loginreq);
            return result;
        }

        [HttpPost("createadmin"), Authorize]
        public async Task<ServiceResponse<string>> CreateNewAdmins(EmailDto emailDto)
        {
            var result = await _adminService.CreateNewAdmins(emailDto);
            return result;
        }

        [HttpPost("lockuser"), Authorize]
        public async Task<ServiceResponse<string>> LockUserAccount(LockingUserDto lockingUserDto)
        {
            var result = await _adminService.LockUserAccount(lockingUserDto);
            return result;
        }  
        
        [HttpPost("unlockuser"), Authorize]
        public async Task<ServiceResponse<string>> UnlockUserAccount(EmailDto emaildto)
        {
            var result = await _adminService.UnlockUserAccount(emaildto);
            return result;
        }

        [HttpPost("createnewgl"), Authorize]
        public async Task<ServiceResponse<GLAccountView>> CreateGlAccount(GLAccountDTO gLAccount)
        {
            var result = await _adminService.CreateGlAccount(gLAccount);
            return result;
        }

        [HttpGet("listoflockedusers"), Authorize]
        public async Task<ServiceResponse<List<ListOfLockedUsers>>> GetListOfLockedUsers()
        {
            var result = await _adminService.GetListOfLockedUsers();
            return result;
        }

        [HttpPost("setwalletcharge"), Authorize]
        public async Task<ServiceResponse<string>> SetWalletCharge(ChargeorRateDTo chargeorRateDTo)
        {
            var result = await _adminService.SetWalletCharge(chargeorRateDTo);
            return result;
        } 
        
        [HttpPost("setwalletrate"), Authorize]
        public async Task<ServiceResponse<string>> SetWalletRate(ChargeorRateDTo chargeorRateDTo)
        {
            var result = await _adminService.SetWalletRate(chargeorRateDTo);
            return result;
        }

        [HttpGet("allglaccounts"), Authorize]
        public async Task<ServiceResponse<List<ListOfGLAccounts>>> GetAllGlAccounts()
        {
            var result = await _adminService.GetAllGlAccounts();
            return result;
        }

        [HttpGet("gltransactionhistory"), Authorize]
        public async Task<ServiceResponse<List<GLTransactionHistory>>> GetAllGlTransactionsHistory()
        {
            var result = await _adminService.GetAllGlTransactionsHistory();
            return result;
        }

        [HttpPost("glcurrencytransactionhistory"), Authorize]
        public async Task<ServiceResponse<List<GLTransactionHistory>>> GetWalletGlTransactionsHistory(CurrencyObj currencyobj)
        {
            var result = await _adminService.GetWalletGlTransactionsHistory(currencyobj);
            return result;
        }

        [HttpPost("generategl")]
        public string GenerateGlNumber(string currency)
        {
            var result = _adminService.GenerateGlNumber(currency);
            return result;
        }

        [HttpPost("resetadmin"), Authorize]
        public async Task<ServiceResponse<string>> ResetAdminCredentials(ResetAdminDto resetAdminDto)
        {
            var result = await _adminService.ResetAdminCredentials(resetAdminDto);
            return result;
        }

        [HttpPost("disableadmin"), Authorize]
        public async Task<ServiceResponse<string>> DisableAdminAccount(DisableAdminDto disableAdminDto)
        {
            var result = await _adminService.DisableAdminAccount(disableAdminDto);
            return result;
        }

        [HttpPost("changeadminpassword")]
        public async Task<ServiceResponse<string>> ChangeAdminPassword(ResetPasswordDto resetPasswordDto)
        {
            var result = await _adminService.ChangeAdminPassword(resetPasswordDto);
            return result;
        }

        [HttpGet("admindetail"), Authorize]
        public async Task<ServiceResponse<AdminAccount>> GetAdminAccountDetail()
        {
            var result = await _adminService.GetAdminAccountDetail();
            return result;
        }


    }
}
