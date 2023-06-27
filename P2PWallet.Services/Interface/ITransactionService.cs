using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public Task<ServiceResponse<List<TransactionsView>>> RecentTransactions();
        public Task<ServiceResponse<List<TransactionsView>>> UserTransactionsByDate(DateDto dateDto);
        public Task<ActionResult> GenerateHistory(ControllerBase controller, TransactionHistoryDto trasactionDto);
        // public Task<ActionResult> GenerateHistoryForEmail(ControllerBase controller, DateDto trasactionDto);
        public Task<ServiceResponse<string>> SendHistoryToEmail(DateDto dateDto);
        //public Task<IFormFile> GenerateExcelFile(ControllerBase controller, DateDto dateDto);
        public Task<ServiceResponse<string>> SendExcelToEmail(DateDto dateDto);


    }
}
