using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using P2PWallet.Services.Interface;
using P2PWallet.Services.Services;

namespace P2PWallet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {

        private readonly DataContext _dataContext;
        private readonly ITransactionService _transactionService;
        public TransactionsController(DataContext dataContext, ITransactionService transactionService)
        {
            _dataContext = dataContext;
            _transactionService = transactionService;

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
        public async Task<ServiceResponse<List<TransactionsView>>> UserTransactionsByDate(DateDto dateDto)
        {
            var result = await _transactionService.UserTransactionsByDate(dateDto);
            return result;
        }

        [HttpGet("generatepdf"), Authorize]
        public async Task<ActionResult> GenerateHistory()
        {
            var result = await _transactionService.GenerateHistory(this);
            return result;
        }


    }
}
