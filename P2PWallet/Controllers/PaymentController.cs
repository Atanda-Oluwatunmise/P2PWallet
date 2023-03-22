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
        public class PaymentController : ControllerBase
        {
            private readonly DataContext _dataContext;
            private readonly IPaymentService _paymentService;

            public PaymentController(DataContext dataContext, IPaymentService paymentService)
            {
                _dataContext = dataContext;
            _paymentService = paymentService;
            }

        [HttpPost("initializepayment"), Authorize]
        public async Task<ServiceResponse<PaystackRequestView>> InitializePayment(DepositDto deposit)
        {
            var result = await _paymentService.InitializePayment(deposit);
            return result;
        }
    }
}
