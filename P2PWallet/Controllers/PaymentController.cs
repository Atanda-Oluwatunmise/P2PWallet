using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.DataObjects.WebHook;
using P2PWallet.Models.Models.Entities;
using P2PWallet.Services.Interface;
using P2PWallet.Services.Services;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Aspose.Pdf.Operators;
using System.Security.Policy;
using Microsoft.AspNetCore.Http.Headers;

namespace P2PWallet.Api.Controllers
{

        [Route("api/[controller]")]
        [ApiController]
        public class PaymentController : ControllerBase
        {
            private readonly DataContext _dataContext;
            private readonly IPaymentService _paymentService;
            //private readonly ILogger _logger;
            private readonly IConfiguration _configuration;

            public PaymentController(DataContext dataContext, IPaymentService paymentService, IConfiguration configuration)
            {
                _dataContext = dataContext;
                _paymentService = paymentService;
                //_logger = logger;
                _configuration = configuration;              
            }

        [HttpPost("initializepayment"), Authorize]
        public async Task<ServiceResponse<PaystackRequestView>> InitializePayment(DepositDto deposit)
        {
            var result = await _paymentService.InitializePayment(deposit);
            return result;
        }

        [HttpPost("WebHook")] 
        public async Task<IActionResult> PayStackWebHook(object obj)  
        {
                //do the iP whitelisting

                //do the signature validation
                var webhookEvent = JsonConvert.DeserializeObject<WebhookDto>(obj.ToString());

                var seckey = _configuration.GetSection("Payment:Secret_Key").Value!;
                String result = "";


                var reqHeader = HttpContext.Request.Headers;
                //AllRequestHeaders.AddRange(reqHeader);

                var reqBody = new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

                byte[] secretkeyBytes = Encoding.UTF8.GetBytes(seckey);

                byte[] inputBytes = Encoding.UTF8.GetBytes(obj.ToString());
                using (var hmac = new HMACSHA512(secretkeyBytes))
                {
                    byte[] hashValue = hmac.ComputeHash(inputBytes);
                    result = BitConverter.ToString(hashValue).Replace("-", string.Empty); ;
                }
                Console.WriteLine(result);

                reqHeader.TryGetValue("x-paystack-signature", out StringValues xpaystackSignature);

                if (!result.ToLower().Equals(xpaystackSignature))
                {
                    return BadRequest();
                }

                await _paymentService.PayStackWebHook(webhookEvent);
                return Ok();
        }

    }
}
