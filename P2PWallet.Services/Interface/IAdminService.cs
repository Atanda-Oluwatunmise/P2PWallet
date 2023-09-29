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
         Task<ServiceResponse<LoginView>> Login(LoginDto loginreq);
         Task<ServiceResponse<string>> CreateNewAdmins(EmailDto emailDto);
         Task<ServiceResponse<string>> LockUserAccount(LockingUserDto lockingUserDto);
         Task<ServiceResponse<string>> UnlockUserAccount(EmailDto emaildto);
         Task<ServiceResponse<GLAccountView>> CreateGlAccount(GLAccountDTO gLAccount);
         Task<ServiceResponse<List<ListOfLockedUsers>>> GetListOfLockedUsers();
         Task<ServiceResponse<string>> SetWalletCharge(ChargeorRateDTo chargeorRateDTo);
         Task<ServiceResponse<string>> SetWalletRate(ChargeorRateDTo chargeorRateDTo);
         Task<ServiceResponse<List<ListOfGLAccounts>>> GetAllGlAccounts();
         Task<ServiceResponse<List<GLTransactionHistory>>> GetAllGlTransactionsHistory();
         Task<ServiceResponse<List<GLTransactionHistory>>> GetWalletGlTransactionsHistory(CurrencyObj currencyobj);
         Task<ServiceResponse<string>> ResetAdminCredentials(ResetAdminDto resetAdminDto);
         Task<ServiceResponse<string>> DisableAdminAccount(DisableAdminDto disableAdminDto);
         Task<ServiceResponse<string>> ChangeAdminPassword(ResetAdminPasswordDto resetPasswordDto);
         Task<ServiceResponse<AdminAccount>> GetAdminAccountDetail();
         Task<ServiceResponse<List<ListOfAdmins>>> GetAllAdmins();
         Task<ServiceResponse<List<ListOfAdmins>>> GetAllDisabledAdmins();
         Task<ServiceResponse<string>> EnableAdminAccount(DisableAdminDto disableAdminDto);
         public string GenerateGlNumber(string currency);

    }
}
