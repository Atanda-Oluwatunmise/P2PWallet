using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Interface
{
    public interface IAdminService
    {
        public Task<ServiceResponse<LoginView>> Login(LoginDto loginreq);
        public Task<ServiceResponse<string>> CreateNewAdmins(EmailDto emailDto);
        public Task<ServiceResponse<string>> LockUserAccount(LockingUserDto lockingUserDto);
        public Task<ServiceResponse<string>> UnlockUserAccount(EmailDto emaildto);
        public Task<ServiceResponse<GLAccountView>> CreateGlAccount(GLAccountDTO gLAccount);
        public Task<ServiceResponse<List<ListOfLockedUsers>>> GetListOfLockedUsers();
        public Task<ServiceResponse<string>> SetWalletCharge(ChargeorRateDTo chargeorRateDTo);
        public Task<ServiceResponse<string>> SetWalletRate(ChargeorRateDTo chargeorRateDTo);
        public Task<ServiceResponse<List<ListOfGLAccounts>>> GetAllGlAccounts();
        public Task<ServiceResponse<List<GLTransactionHistory>>> GetAllGlTransactionsHistory();
        public Task<ServiceResponse<List<GLTransactionHistory>>> GetWalletGlTransactionsHistory(CurrencyObj currencyobj);
        public Task<ServiceResponse<string>> ResetAdminCredentials(ResetAdminDto resetAdminDto);
        public Task<ServiceResponse<string>> DisableAdminAccount(DisableAdminDto disableAdminDto);
        public Task<ServiceResponse<string>> ChangeAdminPassword(ResetPasswordDto resetPasswordDto);
        public Task<ServiceResponse<AdminAccount>> GetAdminAccountDetail();


        public string GenerateGlNumber(string currency);

    }
}
