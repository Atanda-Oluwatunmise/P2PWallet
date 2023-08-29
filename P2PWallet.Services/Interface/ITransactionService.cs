using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Interface
{
    public interface ITransactionService
    {
        public Task<ServiceResponse<AccountViewModel>> Transfers(TransferDto transferdto);
        public string ReferenceGenerator();
        public Task<ServiceResponse<List<TransactionsView>>> UserTransactionsHistory();
        public Task<ServiceResponse<List<TransactionsView>>> RecentTransactions();
        public Task<ServiceResponse<List<TransactionsView>>> UserTransactionsByDate(NewDateDto dateDto);
        public Task<ActionResult> GenerateHistory(ControllerBase controller, TransactionHistoryDto trasactionDto);
        public int GetUserId();
        // public Task<ActionResult> GenerateHistoryForEmail(ControllerBase controller, DateDto trasactionDto);
        public Task<ServiceResponse<string>> SendHistoryToEmail(DateDto dateDto);
        //public Task<IFormFile> GenerateExcelFile(ControllerBase controller, DateDto dateDto);
        public Task<ServiceResponse<string>> SendExcelToEmail(DateDto dateDto);
        public Task<ActionResult> DownloadExcelFile(ControllerBase controller, DateDto dateDto);
        public ConverterView CurrencyConverter(ConverterDto converterDto);
        public Task<ServiceResponse<string>> FundForeignWallet(ConverterDto converterDto);
        public Task<ServiceResponse<string>> ForeignTransfers(ForeignTransferDto foreignTransferDto);
        public List<Transactions> GetTransactions();
        public DataTable GetProductDetailsFromDb();
        public Task<ServiceResponse<TxnsView>> TotalTransactions();



    }
}
