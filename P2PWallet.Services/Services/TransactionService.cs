using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Services
{
    public class TransactionService: ITransactionService
    {
        public static User user = new User();
        public static Account account = new Account();
        private readonly DataContext _dataContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TransactionService(DataContext dataContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _dataContext = dataContext;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
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

                    userAccountNumber.Balance = userAccountNumber.Balance - amount;

                    var data = new AccountViewModel
                    {
                        AccountNumber = userAccountNumber.AccountNumber,
                        Balance = userAccountNumber.Balance,
                        Currency = userAccountNumber.Currency
                    };
                    response.Data = data;


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
                    Amount = amount,
                    Currency = userAccountNumber.Currency,
                    DateofTransaction = DateTime.UtcNow
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

                        var txns = await _dataContext.Transactions.Include("ReceiverUser")
                            .Where(x => x.SenderId == loggedUserId).ToListAsync();


                        foreach (var txn in txns)
                        {

                            var debitdata = new TransactionsView()
                            {
                                Name = txn.ReceiverUser.FirstName + " " + txn.ReceiverUser.LastName,
                                AccountNumber = txn.RecipientAccountNumber,
                                Amount = txn.Amount,
                                Currency = txn.Currency,
                                TransType = "DEBIT",
                                DateofTransaction = txn.DateofTransaction
                            };
                            transactions.Add(debitdata);
                        }


                        var trxns = await _dataContext.Transactions.Include("SenderUser")
                            .Where(x => x.RecipientId == loggedUserId).ToListAsync();


                        foreach (var txn in trxns) { 
                            var creditdata = new TransactionsView()
                            {
                                Name = txn.SenderUser.FirstName + " " + txn.SenderUser.LastName,
                                AccountNumber = txn.SenderAccountNumber,
                                Amount = txn.Amount,
                                Currency = txn.Currency,
                                TransType = "CREDIT",
                                DateofTransaction = txn.DateofTransaction
                            };
                            transactions.Add(creditdata);
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
