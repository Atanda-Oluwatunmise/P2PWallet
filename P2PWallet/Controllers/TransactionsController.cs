using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using P2PWallet.Services.Hubs;
using P2PWallet.Services.Interface;
using P2PWallet.Services.Migrations;
using P2PWallet.Services.Services;

namespace P2PWallet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {

        private readonly DataContext _dataContext;
        private readonly ITransactionService _transactionService;
        private readonly TimerService _timerService;
        private readonly IHubContext<NotificationHub> _hub;

        public TransactionsController(DataContext dataContext, ITransactionService transactionService, TimerService timerService, IHubContext<NotificationHub> hub)
        {
            _dataContext = dataContext;
            _transactionService = transactionService;
            _timerService = timerService;
            _hub = hub;
        }

        [HttpPut("transfers"), Authorize]
        public async Task<ServiceResponse<AccountViewModel>> Transfers(TransferDto transferdto)
        {
            var result = await _transactionService.Transfers(transferdto);
            return result;
        }

        [HttpGet("transactionshistory"), Authorize]
        public async Task<ServiceResponse<List<TransactionsView>>> UserTransactionsHistory()
        {
            var result = await _transactionService.UserTransactionsHistory();
            return result;
        }

        [HttpGet("recenttransactions"), Authorize]
        public async Task<ServiceResponse<List<TransactionsView>>> RecentTransactions()
        {
            var result = await _transactionService.RecentTransactions();
            return result;
        }

        [HttpPost("usertransactionsbydate"), Authorize]
        public async Task<ServiceResponse<List<TransactionsView>>> UserTransactionsByDate(NewDateDto dateDto)
        {
            var result = await _transactionService.UserTransactionsByDate(dateDto);
            return result;
        }

        [HttpPost("generatepdf"), Authorize]
        public async Task<ActionResult> GenerateHistory(TransactionHistoryDto trasactionDto)
        {
            var result = await _transactionService.GenerateHistory(this, trasactionDto);
            return result;
        }

        [HttpPost("generateemailpdf"), Authorize]
        public async Task<ServiceResponse<string>> SendHistoryToEmail(DateDto dateDto)
        {
            var result = await _transactionService.SendHistoryToEmail(dateDto);
            return result;
        }

        [HttpPost("generateexcel"), Authorize]
        public async Task<ServiceResponse<string>> SendExcelToEmail(DateDto dateDto)
        {
            var result = await _transactionService.SendExcelToEmail(dateDto);
            return result;
        }     
        
        [HttpPost("downloadexcel"), Authorize]
        public async Task<ActionResult> DownloadExcelFile(DateDto dateDto)
        {
            var result = await _transactionService.DownloadExcelFile(this, dateDto);
            return result;
        }


        [HttpPost("convertcurrency"), Authorize]
        public ConverterView CurrencyConverter(ConverterDto converterDto)
        {
            var result =  _transactionService.CurrencyConverter(converterDto);
            return result;
        }

        [HttpPost("fundforeignwallet"), Authorize]
        public async Task<ServiceResponse<string>> FundForeignWallet(ConverterDto converterDto)
        {
            var result = await _transactionService.FundForeignWallet(converterDto);
            return result;
        }

        [HttpPost("foreignwallettransfers"), Authorize]
        public async Task<ServiceResponse<string>> ForeignTransfers(ForeignTransferDto foreignTransferDto)
        {
            var result = await _transactionService.ForeignTransfers(foreignTransferDto);
            return result;
        }

        [HttpGet("getchartdata"), Authorize]
        public IActionResult Get()
        {
            if (!_timerService.IsTimerStarted)
                _timerService.PrepareTimer(() => _hub.Clients.All.SendAsync("TransferChartData", DataManager.GetData()));
            return Ok(new { Message = "Request Completed" });
        }

    }
}
