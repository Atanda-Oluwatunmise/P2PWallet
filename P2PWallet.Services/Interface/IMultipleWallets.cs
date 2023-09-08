using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Interface
{
    public interface IMultipleWallets
    {
        Task<ServiceResponse<String>> CreateNewAccountWallet(CurrencyObj currencyObj);
        Task<ServiceResponse<String>> VerifyCurrency(CurrencyObj currencyObj);
        Task<ServiceResponse<List<WalletResponseView>>> VerifyAccount(CurrencyObj currencyObj);
        Task<ServiceResponse<WalletResponseView>> VerifyReceipientAccount(ReceipientCurrencyObj currencyObj);
    }
}
