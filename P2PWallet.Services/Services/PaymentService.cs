﻿using System.Text.Json.Nodes;
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

        public PaymentService(DataContext dataContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _dataContext = dataContext;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
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

                    if (userAccount != null)
                    {
                        if (deposit.Amount <= 0)
                        {
                            throw new Exception("Amount cannot be = 0");
                        }
                        var data = new PaystackRequestDto
                        {
                            email = userAccount.Email,
                            currency = userAccount.UserAccount.Currency,
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

                        var newresp = new Deposit()
                        {
                            UserId = userAccount.Id,
                            UserName = userAccount.Username,
                            Currency = data.currency,
                            Amount = deposit.Amount,
                            TxnRef = data.reference,
                            Email = data.email,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _dataContext.Deposit.AddAsync(newresp);
                        await _dataContext.SaveChangesAsync();
 
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
    }
}