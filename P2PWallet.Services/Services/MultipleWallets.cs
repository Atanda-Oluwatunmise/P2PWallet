using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        public static GLAccount glAccount = new GLAccount();
        private readonly DataContext _dataContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<MultipleWallets> _logger;
        public MultipleWallets(DataContext dataContext, IHttpContextAccessor httpContextAccessor, ILogger<MultipleWallets> logger) 
        {
            _dataContext = dataContext;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<ServiceResponse<String>> CreateNewAccountWallet(string currency)
        {
            var response = new ServiceResponse<String>();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggeduserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var user = await _dataContext.Users.Include("UserAccount").Include("UserGLAccount").Where(x => x.Id == loggeduserId).FirstOrDefaultAsync();
                    var useraccount = await _dataContext.Accounts.Where(x => x.UserId == loggeduserId).FirstOrDefaultAsync();
                    var userglaccount = await _dataContext.GLAccounts.Include("UserGL").Where(x => x.UserId == loggeduserId).FirstOrDefaultAsync();
                    decimal chargeamount = 5;
                    decimal newBalance = 0;

                    if (user.UserAccount.Count() >= 3) 
                    {
                        throw new Exception("Cannot have more than 3 Wallets");
                    }
                    var accountcount = await _dataContext.Accounts.Where(x => x.UserId == loggeduserId && x.Currency.Contains(currency)).ToListAsync();


                        if (user.UserAccount.Count() < 3)
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

                            if (userglaccount.Currency == useraccount.Currency)
                            {
                                userglaccount.Amount += chargeamount;
                                _dataContext.SaveChanges();
                            }
                            else
                            {
                                var glAccount = new GLAccount()
                                {
                                    Amount = chargeamount,
                                    Currency = useraccount.Currency,
                                    UserId = loggeduserId
                                };
                                _dataContext.GLAccounts.Add(glAccount);
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
                                Currency = currency

                            };

                            _dataContext.Accounts.Add(newaccount);
                            await _dataContext.SaveChangesAsync();

                            response.Data = $"{currency} account created successfully";
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
    }
}
