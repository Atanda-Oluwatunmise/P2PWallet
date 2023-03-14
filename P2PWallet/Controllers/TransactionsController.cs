using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
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

        [HttpPut("Transfers"), Authorize]
        public async Task<ServiceResponse<AccountViewModel>> Transfers(TransferDto transferdto)
        {
            var result = await _transactionService.Transfers(transferdto);
            return result;
        }

        [HttpGet("DebitTransactionsHistory"), Authorize]
        public async Task<ServiceResponse<List<TransactionsView>>> DebitTransactionsHistory()
        {
            var result = await _transactionService.DebitTransactionsHistory();
            return result;
        }

        [HttpGet("CreditTransactionsHistory"), Authorize]
        public async Task<ServiceResponse<List<TransactionsView>>> CreditTransactionsHistory()
        {
            var result = await _transactionService.CreditTransactionsHistory();
            return result;
        }

    }
}
