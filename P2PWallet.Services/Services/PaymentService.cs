using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;
using System.Text.Unicode;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using P2PWallet.Services.Interface;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.DataObjects.WebHook;

namespace P2PWallet.Services.Services
{
    public class PaymentService:IPaymentService
    {
        public static User user = new User();
        public static Account account = new Account();
        private readonly DataContext _dataContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        static readonly HttpClient client = new HttpClient();
        private readonly IMailService _mailService;


        public PaymentService(DataContext dataContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IMailService mailService)
        {
            _dataContext = dataContext;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _mailService = mailService;
        }

        public static string ReferenceGenerator()
        {
            Random random = new Random();
            char[] chars =
                           "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            int size = 25;
            byte[] data = new byte[size];

            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);

            }
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();


        }

        public async Task<ServiceResponse<PaystackRequestView>> InitializePayment(DepositDto deposit)
        {
            var response = new ServiceResponse<PaystackRequestView>();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var userId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var userAccount = await _dataContext.Users.Include("UserAccount").Include("UserDeposit").Where(x => x.Id == userId).FirstOrDefaultAsync();
                    var accountdetails = await _dataContext.Accounts.Include("User").Where(x => x.UserId == userId).FirstOrDefaultAsync();

                    if (userAccount != null)
                    {
                        if (deposit.Amount <= 0)
                        {
                            throw new Exception("Amount cannot be 0");
                        }
                        var data = new PaystackRequestDto
                        {
                            email = userAccount.Email,
                            currency = accountdetails.Currency,
                            amount = Convert.ToInt32(deposit.Amount * 100),
                            reference = ReferenceGenerator().ToString()
                        };

                        var secKey = _configuration.GetSection("Payment:Secret_Key").Value!;
                        var uri = _configuration.GetSection("Payment:URI").Value!;

                        client.DefaultRequestHeaders.Add("Accept", "application/json");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secKey);
                        var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8);
                        using HttpResponseMessage request = await client.PostAsync(uri, content);


                        request.EnsureSuccessStatusCode();
                        string responseBody = await request.Content.ReadAsStringAsync();


                        if (request.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception("Status code is not 200");
                        }

                        var respData = JsonConvert.DeserializeObject<PaystackRequestView>(responseBody);
                        response.Data = respData;

                        if (respData != null)
                        {
                            var newresp = new Deposit()
                            {
                                UserId = userAccount.Id,
                                UserName = userAccount.Username,
                                Currency = data.currency,
                                Amount = deposit.Amount,
                                TxnRef = data.reference,
                                Email = data.email,
                                CreatedAt = DateTime.Now,
                                Status = "pending",
                                
                            };

                            await _dataContext.Deposit.AddAsync(newresp);
                            await _dataContext.SaveChangesAsync();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
            } 
            return response;

        }

        public async Task<ServiceResponse<WebhookDto>> PayStackWebHook(WebhookDto eventData)
        {
            var response = new ServiceResponse<WebhookDto>();
            WebhookDto webhookresponse = new WebhookDto();

            try
            {
                var paymentinfo = await _dataContext.Deposit.Where(x => x.TxnRef == eventData.data.reference).FirstOrDefaultAsync();
                var payerAccount = await _dataContext.Users.Include("UserAccount").Where(x => x.Id == paymentinfo.UserId).FirstOrDefaultAsync();
                var userAccount = await _dataContext.Accounts.Include("User").Where(x => x.UserId == paymentinfo.UserId).FirstOrDefaultAsync();

                if (payerAccount == null)
                {
                    throw new Exception("Error");
                }

                if (paymentinfo.Status == "Successful")
                {
                    throw new Exception("Error");
                }

                if (!(eventData.@event == "charge.success") || !(eventData.data.reference == paymentinfo.TxnRef))
                {
                    paymentinfo.Status = "Failed";
                    paymentinfo.CreatedAt = DateTime.Now;
                }

                response.Data = webhookresponse;

                paymentinfo.Status = "Successful";
                paymentinfo.Bank = eventData.data.authorization.bank;
                paymentinfo.CardType = eventData.data.authorization.card_type;
                paymentinfo.Channel = eventData.data.channel;
                paymentinfo.CustomerCode = eventData.data.customer.customer_code;
                paymentinfo.CreatedAt = DateTime.Now;

                userAccount.Balance = userAccount.Balance + paymentinfo.Amount;

                await _dataContext.SaveChangesAsync();


                var txnDeposit = new Transaction()
                {
                    RecipientId = payerAccount.Id,
                    RecipientAccountNumber = userAccount.AccountNumber,
                    Reference = eventData.data.reference,
                    Amount = paymentinfo.Amount,
                    Currency = paymentinfo.Currency,
                    DateofTransaction = paymentinfo.CreatedAt
                };

                var deposit_mail = payerAccount.Email;
                var deposit_Name = payerAccount.Username;
                var deposit_Amount = txnDeposit.Amount;
                var deposit_Balance = userAccount.Balance;
                var DepositInfo = "was deposited to";
                var Deptrantype = "Deposit";
                var Depositdetails = payerAccount.FirstName + " " + payerAccount.LastName;
                var DepositAcount = "Via PAYSTACK";
                var DepositDate = txnDeposit.DateofTransaction;

                await _dataContext.Transactions.AddAsync(txnDeposit);
                await _dataContext.SaveChangesAsync();
                await _mailService.SendAsync(deposit_mail, deposit_Name, Deptrantype, deposit_Amount.ToString(), DepositInfo, Depositdetails, DepositAcount, DepositDate.ToString("yyyy-MM-dd"), deposit_Balance.ToString());

            }
            catch (Exception ex) 
            { 
                response.Status = false;
                response.StatusMessage = ex.Message;
            }

            return response;
        }
    }
}
