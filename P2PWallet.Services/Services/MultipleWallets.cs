using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using P2PWallet.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Services
{
    public class MultipleWallets:IMultipleWallets
    {
        public static Account account = new Account();
        public static WalletCharge glAccount = new WalletCharge();
        private readonly DataContext _dataContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<MultipleWallets> _logger;
        private readonly ITransactionService _transactionService;
        public MultipleWallets(DataContext dataContext, IHttpContextAccessor httpContextAccessor, ILogger<MultipleWallets> logger, ITransactionService transactionService) 
        {
            _dataContext = dataContext;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _transactionService = transactionService;
        }

        public async Task<ServiceResponse<String>> CreateNewAccountWallet(CurrencyObj currencyObj)
        {
            var response = new ServiceResponse<String>();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggeduserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var user = await _dataContext.Users.Include("UserAccount").Include("UserWalletCharge").Where(x => x.Id == loggeduserId && x.KycVerified == true).FirstOrDefaultAsync();
                    var useraccount = await _dataContext.Accounts.Where(x => x.UserId == loggeduserId).FirstOrDefaultAsync();
                    var userwalletcharge = await _dataContext.WalletCharges.Include("UserWalletCharge").Where(x => x.UserId == loggeduserId).FirstOrDefaultAsync();
                    var currenciesList = await _dataContext.CurrenciesWallets.Where(x => x.Currencies == currencyObj.Currency).FirstOrDefaultAsync();

                    decimal chargeamount = (decimal)currenciesList.ChargeAmount;
                    decimal newBalance = 0;

                    if (user == null)
                    {
                        throw new Exception("Cannot create foreign wallets, account is not upgraded.");
                    }

                    if (user.UserAccount.Count() >= 4) 
                    {
                        throw new Exception("Cannot have more than 3 Wallets");
                    }
                    var accountcount = await _dataContext.Accounts.Where(x => x.UserId == loggeduserId && x.Currency.Contains(currencyObj.Currency)).ToListAsync();
                    var glaccount = await _dataContext.GLAccounts.Where(x => x.Currency.ToLower() == "ngn").FirstOrDefaultAsync();

                        if (user.UserAccount.Count() < 4)
                    {
                        if(useraccount.Balance < chargeamount)
                        {
                            throw new Exception("Insufficient Balance");
                        }

                        if(accountcount.Count != 0)
                        {
                            throw new Exception("This wallet account exists already");
                        }
                        if (useraccount.Balance >= chargeamount)
                        {
                            useraccount.Balance = useraccount.Balance - chargeamount;
                            _dataContext.SaveChanges();

                            var newtransaction = new Transactions()
                            {
                                SenderId = user.Id,
                                SenderAccountNumber = useraccount.AccountNumber,
                                RecipientAccountNumber = glaccount.GLNumber,
                                Reference = _transactionService.ReferenceGenerator(),
                                Amount= chargeamount,
                                Currency = useraccount.Currency,
                                DateofTransaction = DateTime.Now
                            };
                            await _dataContext.Transactions.AddAsync(newtransaction);
                            await _dataContext.SaveChangesAsync();

                            glaccount.Balance += chargeamount;
                            _dataContext.SaveChanges();

                            if (userwalletcharge.Currency == useraccount.Currency)
                            {
                                userwalletcharge.Amount += chargeamount;
                                _dataContext.SaveChanges();
                            }
                            else
                            {
                                userwalletcharge.Amount = chargeamount;
                                userwalletcharge.Currency = useraccount.Currency;
                                await _dataContext.SaveChangesAsync();
                            }

                            string userAccounNumber = string.Empty;
                            string startWith = "0101";
                            Random generator = new Random();
                            string r = generator.Next(0, 999999).ToString("D6");
                            userAccounNumber = startWith + r;

                            var newaccount = new Account()
                            {
                                UserId = loggeduserId,
                                AccountNumber = userAccounNumber,
                                Balance = newBalance,
                                Currency = currencyObj.Currency.ToUpper()

                            };

                            _dataContext.Accounts.Add(newaccount);
                            await _dataContext.SaveChangesAsync();

                            response.Data = $"{currencyObj.Currency} account created successfully";
                        }
                    }
                }
            }
            catch (Exception ex) 
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"An error just occured...... {ex.Message}");
            }

            return response;
        }

        public async Task<ServiceResponse<String>> VerifyCurrency(CurrencyObj currencyObj)
        {
            var response = new ServiceResponse<String>();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggeduserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var user = await _dataContext.Users.Include("UserAccount").Include("UserWalletCharge").Where(x => x.Id == loggeduserId).FirstOrDefaultAsync();
                    if(user != null )
                    {
                        var currenciesList = await _dataContext.CurrenciesWallets.Where(x => x.Currencies == currencyObj.Currency).FirstOrDefaultAsync();
                        if(currenciesList != null)
                        {
                            response.Data = currenciesList.ChargeAmount.ToString();
                        }
                        else
                        {
                            response.Data = "Wallet currency is not available";
                            _logger.LogError($"Error Message... {response.Data}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"An Error just occurred.....{ex.Message}");
            }
            return response;
        }

        public async Task<ServiceResponse<List<WalletResponseView>>> VerifyAccount(CurrencyObj currencyObj)
        {
            var response = new ServiceResponse<List<WalletResponseView>>();
            List<WalletResponseView> accountDetails = new List<WalletResponseView>();

            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggeduserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var user = await _dataContext.Accounts.Include("User").Where(x => x.UserId == loggeduserId).FirstOrDefaultAsync();
                    if (user != null)
                    {
                        var accountCurrency = await _dataContext.Accounts.Where(x => x.UserId == loggeduserId && x.Currency.ToLower() == currencyObj.Currency.ToLower()).FirstOrDefaultAsync();
                        if (accountCurrency == null)
                        {
                            throw new Exception("Wallet Account does not exist");
                        }
                        var data = new WalletResponseView()
                        {
                            Currency = accountCurrency.Currency,
                            AccountNumber = accountCurrency.AccountNumber,
                            Balance = accountCurrency.Balance
                        };
                        accountDetails.Add(data);
                        response.Data = accountDetails;

                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"An Error just occured.... {ex.Message}");
            }
            return response;
        }
    }
}
