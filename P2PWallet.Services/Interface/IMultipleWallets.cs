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
        public Task<ServiceResponse<String>> CreateNewAccountWallet(CurrencyObj currencyObj);
        public Task<ServiceResponse<String>> VerifyCurrency(CurrencyObj currencyObj);
        public Task<ServiceResponse<List<WalletResponseView>>> VerifyAccount(CurrencyObj currencyObj);


    }
}
