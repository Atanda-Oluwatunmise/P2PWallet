using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Interface
{
    public interface ITransactionService
    {
        public Task<ServiceResponse<AccountViewModel>> Transfers(TransferDto transferdto);
        public Task<ServiceResponse<List<TransactionsView>>> UserTransactionsHistory();



    }
}
