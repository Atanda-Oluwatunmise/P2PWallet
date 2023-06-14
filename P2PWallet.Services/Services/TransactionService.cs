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
using MailKit.Security;
using Microsoft.Extensions.Hosting;
using MimeKit;
using System.Runtime;
using Microsoft.AspNetCore.Hosting;
using MailKit.Net.Smtp;
using System.Net.Mail;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;
using Org.BouncyCastle.Cms;
using Microsoft.AspNetCore.Mvc;
using PdfSharpCore.Pdf;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using PdfSharpCore;
using DinkToPdf.Contracts;
using DinkToPdf;

namespace P2PWallet.Services.Services
{
    public class TransactionService: ITransactionService
    {
        public static User user = new User();
        public static Account account = new Account();
        private readonly DataContext _dataContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMailService _mailService;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IConverter _converter;
        static readonly HttpClient client = new HttpClient();

        public TransactionService(DataContext dataContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IMailService mailService, IWebHostEnvironment hostEnvironment, IConverter converter)
        {
            _dataContext = dataContext;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _mailService = mailService;
            _hostEnvironment = hostEnvironment;
            _converter = converter;
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

                //Debit Info
                var debit_mail = userAccountNumber.User.Email;
                var debit_Name = userAccountNumber.User.Username;
                var debit_Amount = amount;
                var debit_Balance = userAccountNumber.Balance;
                var Dtrantype = "Debit";
                var DebitInfo = "has been sent to";
                var Ddetails = receiverAccount.User.FirstName + " " + receiverAccount.User.LastName;
                var DAcc = receiverAccount.AccountNumber;
                var DDate = txns.DateofTransaction;

                //Credit Info
                var credit_mail = receiverAccount.User.Email;
                var credit_Name = receiverAccount.User.Username;
                var credit_Amount = amount;
                var credit_Balance = receiverAccount.Balance;
                var CreditInfo = "was received from";
                var Ctrantype = "Credit";
                var Cdetails = userAccountNumber.User.FirstName + " " + userAccountNumber.User.LastName;
                var CAcc = userAccountNumber.AccountNumber;
                var CDate = txns.DateofTransaction;


                await _mailService.SendAsync(debit_mail, debit_Name, Dtrantype, debit_Amount.ToString(), DebitInfo, Ddetails, DAcc, DDate.ToString("yyyy-MM-dd"), debit_Balance.ToString());
                await _mailService.SendAsync(credit_mail, credit_Name, Ctrantype, credit_Amount.ToString(), CreditInfo, Cdetails, CAcc, CDate.ToString("yyyy-MM-dd"), credit_Balance.ToString());
                //await _mailService.CreditMail(credit_mail, credit_Name, credit_Amount, credit_Balance, CDate);
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
            }
            return response;
        }

        public async Task<List<TransactionsView>> TransactionsHistory()
        {
            List<TransactionsView> transactions = new List<TransactionsView>();

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
            }


            return transactions;
        }
        public async Task<ServiceResponse<List<TransactionsView>>> RecentTransactions()
        {
            var response = new ServiceResponse<List<TransactionsView>>();

            try
            {
                    var data = await TransactionsHistory();
                    response.Data = data.OrderByDescending(x => x.DateofTransaction).Take(3).ToList();
                
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
            }
            return response;
        }


        public async Task<ServiceResponse<List<TransactionsView>>> UserTransactionsByDate(DateDto dateDto)
        {
            DateTime fromDate = DateTime.Parse(dateDto.startDate);
            DateTime toDate = DateTime.Parse(dateDto.endDate);
            var response = new ServiceResponse<List<TransactionsView>>();
            List<TransactionsView> newtransactions = new List<TransactionsView>();
            List<TransactionsView> transactionsHolder = new List<TransactionsView>();

            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggedUserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    if (loggedUserId != null)
                    {
                        var data = await TransactionsHistory();


                        if (toDate < fromDate)
                        {
                            throw new Exception("endDate must be greater than or equal to startDate");
                        }

                        if (fromDate <= toDate)
                        {
                            for (var day = fromDate.Date; day.Date <= toDate.Date; day = day.AddDays(1))
                            {
                                newtransactions = data.Where(x => x.DateofTransaction.Date == day.Date).ToList();
                                if(newtransactions.Count != 0 && newtransactions != null)
                                {
                                    transactionsHolder.AddRange(newtransactions);
                                }

                            }
                            if (transactionsHolder.Count == 0)
                            {
                                response.Data = null;
                            }
                            else
                            {
                                response.Data = transactionsHolder.OrderBy(x => x.DateofTransaction).ToList();
                            }
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

        public async Task<ServiceResponse<List<TransactionsView>>> UserTransactionsHistory()
        {
            var response = new ServiceResponse<List<TransactionsView>>();
            List<TransactionsView> transactions = new List<TransactionsView>();

            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var data = await TransactionsHistory();
                    response.Data = data.OrderBy(x => x.DateofTransaction).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
            }
            return response;
        }

        public byte[] GeneratePdf(string htmlContent)
        {
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 },
                DocumentTitle = "Transactions History"
            };

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = htmlContent,
                Page = "",
                WebSettings = { DefaultEncoding = "utf-8" },
                HeaderSettings = { FontSize = 12, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 },
                FooterSettings = { FontSize = 12, Line = true, Right = "© " + DateTime.Now.Year }
            };

            var document = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = {objectSettings}
            };

            return _converter.Convert(document);
        }

        public async Task<ActionResult> GenerateHistory(ControllerBase controller)
        {

            var loggedUserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

            var txns = await _dataContext.Users.Include("UserAccount").Where(x => x.Id == loggedUserId).FirstOrDefaultAsync();
            var date = DateTime.Now.ToShortDateString();
            var url = _hostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                   + "transactionImage" + Path.DirectorySeparatorChar.ToString() + "bankifyimg.png";
            var PathToFile = _hostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                   + "transactionStatementTemplate" + Path.DirectorySeparatorChar.ToString() + "transactionsTemp.html";
            var data = await TransactionsHistory();

            StringBuilder sb = new StringBuilder();
            foreach (var txn in data)
            {
                sb.Append("<tr>");
                sb.Append("<td>");
                sb.Append("{S/N}");
                sb.Append("</td>");
                sb.Append("<td>");
                sb.Append(txn.DateofTransaction);
                sb.Append("</td>");
                sb.Append("<td colspan='3'>");
                sb.Append(txn.ReceiverInfo);
                sb.Append("</td>");
                sb.Append("<td>");
                sb.Append("{Debit");
                sb.Append("</td>");
                sb.Append("<td>");
                sb.Append("{Credit}");
                sb.Append("</td>");
            }

            string htmlContent = "";

            using (StreamReader streamreader = File.OpenText(PathToFile))
            {
                htmlContent = streamreader.ReadToEnd();
                htmlContent = htmlContent.Replace("{StatementDate}", date);
                htmlContent = htmlContent.Replace("{REFERENCENO}", date);
                htmlContent = htmlContent.Replace("{imgurl}", url);
                htmlContent = htmlContent.Replace("{CustomerName}", $"{txns.FirstName} {txns.LastName} ");
                htmlContent = htmlContent.Replace("{CustomerAddress}", txns.Address);
                htmlContent = htmlContent.Replace("{CustomerPhone}", txns.PhoneNumber);
                htmlContent = htmlContent.Replace("{CustomerEmail}", txns.Email);
                htmlContent = htmlContent.Replace("{Currency}", txns.UserAccount.Currency);
                htmlContent = htmlContent.Replace("{CurrentBalance}", txns.UserAccount.Balance.ToString());
                htmlContent = htmlContent.Replace("{Transactions}", sb.ToString());
            }

            byte[] pdfBytes = GeneratePdf(htmlContent);
            return controller.File(pdfBytes, "application/pdf", "generated.pdf");


            
        }
    }
}
