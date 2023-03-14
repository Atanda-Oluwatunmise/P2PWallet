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
                var newaccount = await _dataContext.Accounts.FirstOrDefaultAsync(x => x.AccountNumber == transferdto.AccountNumber);

                if (newaccount == null)
                {
                    throw new Exception("Receiver's account number is incorrect.");
                }
                var receipientaccount = new Account()
                {
                    AccountNumber = newaccount.AccountNumber,
                    Balance = newaccount.Balance
                };

                double amount = transferdto.Amount;

                var senderaccount = _httpContextAccessor.HttpContext.User.FindFirstValue("AccountNumber");
                var userAccountNumber = _dataContext.Accounts.Include("User").Where(x => x.AccountNumber == senderaccount).FirstOrDefault();
                if (userAccountNumber != null)
                {
                    double newamount = userAccountNumber.Balance - amount;
                    //balance = newamount;
                    userAccountNumber.Balance = newamount;

                }
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

                //var userId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                var txns = new Transaction()
                {
                    SenderId = userAccountNumber.UserId,
                    RecipientId = receiverAccount.UserId,
                    SenderAccountNumber = userAccountNumber.AccountNumber,
                    NameofSender = userAccountNumber.User.FirstName + " " + userAccountNumber.User.LastName,
                    RecipientAccountNumber = receiverAccount.AccountNumber,
                    NameofRecipient = receiverAccount.User.FirstName + " " + receiverAccount.User.LastName,
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

        public async Task<ServiceResponse<List<TransactionsView>>> DebitTransactionsHistory()
        {
            var response = new ServiceResponse<List<TransactionsView>>();
            List<TransactionsView> transactions = new List<TransactionsView>();

            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggedUserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var loggedUserAccount = await _dataContext.Transactions.Where(x => x.SenderId == loggedUserId).AnyAsync();

                    if (loggedUserAccount != null)
                    {
                        var txns = await _dataContext.Transactions
                            .Where(x => x.SenderId == loggedUserId).ToListAsync();

                        foreach( var txn in txns) 
                        {
                            
                            var data = new TransactionsView()
                            {
                                Name = txn.NameofRecipient,
                                AccountNumber = txn.RecipientAccountNumber,
                                Amount = txn.Amount,
                                Currency = txn.Currency,
                                DateofTransaction = txn.DateofTransaction
                            };
                            
                            transactions.Add(data);
                        }

                    }
                    response.Data = transactions;
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<List<TransactionsView>>> CreditTransactionsHistory()
        {
            var response = new ServiceResponse<List<TransactionsView>>();
            List<TransactionsView> transactions = new List<TransactionsView>();

            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggedUserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var loggedUserAccount = await _dataContext.Transactions.Where(x => x.RecipientId == loggedUserId).AnyAsync();

                    if (loggedUserAccount != null)
                    {
                        var txns = await _dataContext.Transactions
                            .Where(x => x.RecipientId == loggedUserId).ToListAsync();

                        foreach (var txn in txns)
                        {

                            var data = new TransactionsView()
                            {
                                Name = txn.NameofSender,
                                AccountNumber = txn.SenderAccountNumber,
                                Amount = txn.Amount,
                                Currency = txn.Currency,
                                DateofTransaction = txn.DateofTransaction
                            };

                            transactions.Add(data);
                        }

                    }
                    response.Data = transactions;
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
