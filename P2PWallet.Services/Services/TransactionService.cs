using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using P2PWallet.Services.Interface;
using HttpClient = System.Net.Http.HttpClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
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

namespace P2PWallet.Services.Services
{
    public class TransactionService: ITransactionService
    {
        public static User user = new User();
        public static Account account = new Account();
        private readonly DataContext _dataContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        static readonly HttpClient client = new HttpClient();

        public TransactionService(DataContext dataContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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

        public async Task<ServiceResponse<AccountViewModel>> Transfers(TransferDto transferdto)
        {
            var response = new ServiceResponse<AccountViewModel>();
            try
            {
                var recieverUser = await _dataContext.Users.FirstOrDefaultAsync(x => x.UserAccount.AccountNumber == transferdto.AccountSearch 
                                                                            || x.Username == transferdto.AccountSearch
                                                                            || x.Email == transferdto.AccountSearch);

                if (recieverUser == null)
                {
                    throw new Exception("Receiver's details is incorrect.");
                }

                var receiverAcc = await _dataContext.Accounts.Include("User").Where(x => x.UserId == recieverUser.Id).FirstOrDefaultAsync();

                var receipientaccount = new Account()
                {
                    AccountNumber = receiverAcc.AccountNumber,
                    Balance = receiverAcc.Balance
                };

                double amount = transferdto.Amount;

                var senderaccount = _httpContextAccessor.HttpContext.User.FindFirstValue("AccountNumber");
                var userAccountNumber = _dataContext.Accounts.Include("User").Where(x => x.AccountNumber == senderaccount).FirstOrDefault();

                if (userAccountNumber == null || userAccountNumber == receiverAcc)
                {
                    throw new Exception("Account number is invalid");
                }


                if (amount > userAccountNumber.Balance)
                {
                    throw new Exception("Insufficient Account Balance");
                }

                if (amount < 0 || amount == 0)
                {
                    throw new Exception("Amount cannot be less than or equal to 0");
                }

                if (senderaccount != null || userAccountNumber.Balance == amount || amount < userAccountNumber.Balance)
                {

                    userAccountNumber.Balance = userAccountNumber.Balance - amount;

                    var data = new AccountViewModel
                    {
                        AccountNumber = userAccountNumber.AccountNumber,
                        Balance = userAccountNumber.Balance,
                        Currency = userAccountNumber.Currency
                    };
                    response.Data = data;
                }

                _dataContext.Accounts.Update(userAccountNumber);
                await _dataContext.SaveChangesAsync();


                double receipientamount = receipientaccount.Balance + amount;
                double newBalance = receipientamount;
                var receiverAccount = _dataContext.Accounts.Include("User").Where(x => x.AccountNumber == receipientaccount.AccountNumber).FirstOrDefault();
                receiverAccount.AccountNumber = receipientaccount.AccountNumber;
                receiverAccount.Balance = newBalance;

                _dataContext.Accounts.Update(receiverAccount);
                await _dataContext.SaveChangesAsync();

                var txns = new Transaction()
                {
                    SenderId = userAccountNumber.UserId,
                    RecipientId = receiverAccount.UserId,
                    SenderAccountNumber = userAccountNumber.AccountNumber,
                    RecipientAccountNumber = receiverAccount.AccountNumber,
                    Reference = ReferenceGenerator(),
                    Amount = amount,
                    Currency = userAccountNumber.Currency,
                    DateofTransaction = DateTime.Now
                };

                await _dataContext.Transactions.AddAsync(txns);
                await _dataContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<List<TransactionsView>>> UserTransactionsHistory()
        {
            var response = new ServiceResponse<List<TransactionsView>>();
            List<TransactionsView> transactions = new List<TransactionsView>();

            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggedUserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                        if (loggedUserId != null)
                        {
                            var txns = await _dataContext.Transactions.Include("ReceiverUser").Include("SenderUser")
                                .Where(x => x.SenderId == loggedUserId).ToListAsync();

                            foreach (var txn in txns)
                            {
                                var debitdata = new TransactionsView()
                                {
                                    SenderInfo = txn.SenderUser.FirstName + " " + txn.SenderUser.LastName + " - " + txn.SenderAccountNumber,
                                    Currency = txn.Currency,
                                    TxnAmount = txn.Amount,
                                    TransType = "DEBIT",
                                    ReceiverInfo = txn.ReceiverUser.FirstName + " " + txn.ReceiverUser.LastName + " - " + txn.RecipientAccountNumber,
                                    DateofTransaction = txn.DateofTransaction
                                };
                                transactions.Add(debitdata);
                            }

                            var trxns = await _dataContext.Transactions.Include("SenderUser").Include("ReceiverUser")
                                .Where(x => x.RecipientId == loggedUserId).ToListAsync();

                            foreach (var txn in trxns)
                            {
                                if (txn.SenderId == null)
                                {
                                    var creditdata = new TransactionsView()
                                    {
                                        SenderInfo = "PAYSTACK_FUNDING",
                                        Currency = txn.Currency,
                                        TxnAmount = txn.Amount,
                                        TransType = "CREDIT",
                                        ReceiverInfo = txn.ReceiverUser.FirstName + " " + txn.ReceiverUser.LastName + " - " + txn.RecipientAccountNumber,
                                        DateofTransaction = txn.DateofTransaction
                                    };
                                    transactions.Add(creditdata);
                            }
                                if (txn.SenderId != null)
                                    {
                                        var creditdata = new TransactionsView()
                                        {
                                            SenderInfo = txn.SenderUser.FirstName + " " + txn.SenderUser.LastName + " - " + txn.SenderAccountNumber,
                                            Currency = txn.Currency,
                                            TxnAmount = txn.Amount,
                                            TransType = "CREDIT",
                                            ReceiverInfo = txn.ReceiverUser.FirstName + " " + txn.ReceiverUser.LastName + " - " + txn.RecipientAccountNumber,
                                            DateofTransaction = txn.DateofTransaction
                                        };
                                        transactions.Add(creditdata);
                                    }
                            }
                        }
                        response.Data = transactions.OrderByDescending(x => x.DateofTransaction).ToList();
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
