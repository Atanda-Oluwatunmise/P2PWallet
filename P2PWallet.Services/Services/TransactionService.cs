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
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        public async Task<List<TransactionsView>> TransactionHistoryForPdf()
        {
            List<TransactionsView> transactions = new List<TransactionsView>();
            var loggedUserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (loggedUserId != null)
            {
                var txnss = await _dataContext.Transactions.Include("ReceiverUser").Include("SenderUser")
                    .Where(x => x.SenderId == loggedUserId).ToListAsync();

                foreach (var txn in txnss)
                {
                    var debitdata = new TransactionsView()
                    {
                        SenderInfo = txn.SenderUser.FirstName + " " + txn.SenderUser.LastName + " - " + txn.SenderAccountNumber,
                        Currency = txn.Currency,
                        TxnAmount = txn.Amount,
                        TransType = "DEBIT",
                        ReceiverInfo = $"SentTo//{txn.ReceiverUser.FirstName} {txn.ReceiverUser.LastName}|{txn.RecipientAccountNumber}",
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
                            ReceiverInfo = $"{txn.ReceiverUser.FirstName} {txn.ReceiverUser.LastName} {txn.RecipientAccountNumber}",
                            DateofTransaction = txn.DateofTransaction
                        };
                        transactions.Add(creditdata);
                    }
                    if (txn.SenderId != null)
                    {
                        var creditdata = new TransactionsView()
                        {
                            SenderInfo = $"SentFrom//{txn.SenderUser.FirstName} {txn.SenderUser.LastName}|{txn.SenderAccountNumber}",
                            Currency = txn.Currency,
                            TxnAmount = txn.Amount,
                            TransType = "CREDIT",
                            ReceiverInfo = $"{txn.ReceiverUser.FirstName} {txn.ReceiverUser.LastName} {txn.RecipientAccountNumber}",
                            DateofTransaction = txn.DateofTransaction
                        };
                        transactions.Add(creditdata);
                    }
                }
            }
            return transactions;
        }

        public async Task<ActionResult> GenerateHistory(ControllerBase controller, TransactionHistoryDto trasactionDto)
        {
            DateTime startdate = DateTime.Parse(trasactionDto.fromDate);
            DateTime enddate = DateTime.Parse(trasactionDto.toDate);
            List<TransactionsView> newtransactions = new List<TransactionsView>();
            List<TransactionsView> transactionsHolder = new List<TransactionsView>();
            List<TransactionsView> holder = new List<TransactionsView>();

            var loggedUserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var txns = await _dataContext.Users.Include("UserAccount").Where(x => x.Id == loggedUserId).FirstOrDefaultAsync();
            var userFullName = $"{txns.FirstName} {txns.LastName} ";
            var date = DateTime.Now.ToShortDateString();
            var url = _hostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                   + "transactionImage" + Path.DirectorySeparatorChar.ToString() + "bankifyimg.png";
            var PathToFile = _hostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                   + "transactionStatementTemplate" + Path.DirectorySeparatorChar.ToString() + "transactionsTemp.html";
            int sn = 1;

            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[12];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new string(stringChars);
            var transactions = await TransactionHistoryForPdf();

            if (enddate < startdate)
                {
                    throw new Exception("endDate must be greater than or equal to startDate");
                }

            if (startdate <= enddate)
            {
                for (var day = startdate.Date; day.Date <= enddate.Date; day = day.AddDays(1))
                {
                    newtransactions = transactions.Where(x => x.DateofTransaction.Date == day.Date).ToList();
                    if (newtransactions.Count != 0 && newtransactions != null)
                    {
                        transactionsHolder.AddRange(newtransactions);
                    }
                }
            }

            if(trasactionDto.txnType.ToLower() == "credit")
            {
                holder.AddRange(transactionsHolder.Where(x => x.TransType == "CREDIT"));
            }

            else if (trasactionDto.txnType.ToLower() == "debit")
            {
                holder.AddRange(transactionsHolder.Where(x => x.TransType == "DEBIT"));
            }

            else
            {
                holder.AddRange(transactionsHolder);
            }

            transactionsHolder = holder.OrderBy(x => x.DateofTransaction).ToList();
            double creditAmount = 0;
            double debitAmount = 0;
            StringBuilder sb = new StringBuilder();
            foreach (var txn in transactionsHolder)
            {
                sb.Append("<tr>");
                sb.Append("<td>");
                sb.Append(sn++.ToString());
                sb.Append("</td>");
                sb.Append("<td>");
                sb.Append(txn.DateofTransaction.Date.ToString("dd-MM-yyyy"));
                sb.Append("</td>");
                sb.Append("<td colspan='3'>");
                if (txn.SenderInfo.Contains(userFullName) && txn.TransType == "DEBIT")
                {
                    sb.Append(txn.ReceiverInfo);
                    sb.Append("</td>");
                    sb.Append("<td>");
                    sb.Append(txn.TxnAmount);
                    sb.Append("</td>");
                    sb.Append("<td>");
                    sb.Append("-");
                    debitAmount += txn.TxnAmount;
                }
                else if (txn.ReceiverInfo.Contains(userFullName) && txn.TransType == "CREDIT")
                {
                    sb.Append(txn.SenderInfo);
                    sb.Append("</td>");
                    sb.Append("<td>");
                    sb.Append("-");
                    sb.Append("</td>");
                    sb.Append("<td>");
                    sb.Append(txn.TxnAmount);
                    creditAmount += txn.TxnAmount;
                }
                else
                {
                    sb.Append(txn.SenderInfo);
                    sb.Append("</td>");
                    sb.Append("<td>");
                    sb.Append("-");
                    sb.Append("</td>");
                    sb.Append("<td>");
                    sb.Append(txn.TxnAmount);
                    creditAmount += txn.TxnAmount;
                }
                sb.Append("</td>");
            }

            string htmlContent = "";
            using (StreamReader streamreader = File.OpenText(PathToFile))
            {
                htmlContent = streamreader.ReadToEnd();
                htmlContent = htmlContent.Replace("{StatementDate}", date);
                htmlContent = htmlContent.Replace("{REFERENCENO}", finalString.ToLower());
                htmlContent = htmlContent.Replace("{imgurl}", url);
                htmlContent = htmlContent.Replace("{CustomerName}", $"{txns.FirstName} {txns.LastName}");
                htmlContent = htmlContent.Replace("{CustomerAddress}", txns.Address);
                htmlContent = htmlContent.Replace("{CustomerPhone}", txns.PhoneNumber);
                htmlContent = htmlContent.Replace("{CustomerEmail}", txns.Email);
                htmlContent = htmlContent.Replace("{Currency}", txns.UserAccount.Currency);
                htmlContent = htmlContent.Replace("{CurrentBalance}", txns.UserAccount.Balance.ToString());
                htmlContent = htmlContent.Replace("{startdate}", startdate.Date.ToString());
                htmlContent = htmlContent.Replace("{enddate}", enddate.Date.ToString());
                htmlContent = htmlContent.Replace("{Transactions}", sb.ToString());
                htmlContent = htmlContent.Replace("{TotalDebit}", debitAmount.ToString());
                htmlContent = htmlContent.Replace("{TotalCredit}", creditAmount.ToString());
            }

            byte[] pdfBytes = GeneratePdf(htmlContent);
            return controller.File(pdfBytes, "application/pdf", "generated.pdf");

        }
        public async Task<ActionResult> GenerateHistoryForEmail (ControllerBase controller, DateDto trasactionDto)
        {
            DateTime startdate = DateTime.Parse(trasactionDto.startDate);
            DateTime enddate = DateTime.Parse(trasactionDto.endDate);
            List<TransactionsView> newtransactions = new List<TransactionsView>();
            List<TransactionsView> transactionsHolder = new List<TransactionsView>();
            List<TransactionsView> holder = new List<TransactionsView>();

            var loggedUserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var txns = await _dataContext.Users.Include("UserAccount").Where(x => x.Id == loggedUserId).FirstOrDefaultAsync();
            var userFullName = $"{txns.FirstName} {txns.LastName} ";
            var date = DateTime.Now.ToShortDateString();
            var url = _hostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                   + "transactionImage" + Path.DirectorySeparatorChar.ToString() + "bankifyimg.png";
            var PathToFile = _hostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                   + "transactionStatementTemplate" + Path.DirectorySeparatorChar.ToString() + "transactionsTemp.html";
            int sn = 1;

            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[12];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new string(stringChars);
            var transactions = await TransactionHistoryForPdf();

            if (enddate < startdate)
                {
                    throw new Exception("endDate must be greater than or equal to startDate");
                }

            if (startdate <= enddate)
            {
                for (var day = startdate.Date; day.Date <= enddate.Date; day = day.AddDays(1))
                {
                    newtransactions = transactions.Where(x => x.DateofTransaction.Date == day.Date).ToList();
                    if (newtransactions.Count != 0 && newtransactions != null)
                    {
                        transactionsHolder.AddRange(newtransactions);
                    }
                }
            }

           

            transactionsHolder = transactionsHolder.OrderBy(x => x.DateofTransaction).ToList();
            double creditAmount = 0;
            double debitAmount = 0;
            StringBuilder sb = new StringBuilder();
            foreach (var txn in transactionsHolder)
            {
                sb.Append("<tr>");
                sb.Append("<td>");
                sb.Append(sn++.ToString());
                sb.Append("</td>");
                sb.Append("<td>");
                sb.Append(txn.DateofTransaction.Date.ToString("dd-MM-yyyy"));
                sb.Append("</td>");
                sb.Append("<td colspan='3'>");
                if (txn.SenderInfo.Contains(userFullName) && txn.TransType == "DEBIT")
                {
                    sb.Append(txn.ReceiverInfo);
                    sb.Append("</td>");
                    sb.Append("<td>");
                    sb.Append(txn.TxnAmount);
                    sb.Append("</td>");
                    sb.Append("<td>");
                    sb.Append("-");
                    debitAmount += txn.TxnAmount;
                }
                else if (txn.ReceiverInfo.Contains(userFullName) && txn.TransType == "CREDIT")
                {
                    sb.Append(txn.SenderInfo);
                    sb.Append("</td>");
                    sb.Append("<td>");
                    sb.Append("-");
                    sb.Append("</td>");
                    sb.Append("<td>");
                    sb.Append(txn.TxnAmount);
                    creditAmount += txn.TxnAmount;
                }
                else
                {
                    sb.Append(txn.SenderInfo);
                    sb.Append("</td>");
                    sb.Append("<td>");
                    sb.Append("-");
                    sb.Append("</td>");
                    sb.Append("<td>");
                    sb.Append(txn.TxnAmount);
                    creditAmount += txn.TxnAmount;
                }
                sb.Append("</td>");
            }

            string htmlContent = "";
            using (StreamReader streamreader = File.OpenText(PathToFile))
            {
                htmlContent = streamreader.ReadToEnd();
                htmlContent = htmlContent.Replace("{StatementDate}", date);
                htmlContent = htmlContent.Replace("{REFERENCENO}", finalString.ToLower());
                htmlContent = htmlContent.Replace("{imgurl}", url);
                htmlContent = htmlContent.Replace("{CustomerName}", $"{txns.FirstName} {txns.LastName}");
                htmlContent = htmlContent.Replace("{CustomerAddress}", txns.Address);
                htmlContent = htmlContent.Replace("{CustomerPhone}", txns.PhoneNumber);
                htmlContent = htmlContent.Replace("{CustomerEmail}", txns.Email);
                htmlContent = htmlContent.Replace("{Currency}", txns.UserAccount.Currency);
                htmlContent = htmlContent.Replace("{CurrentBalance}", txns.UserAccount.Balance.ToString());
                htmlContent = htmlContent.Replace("{startdate}", startdate.Date.ToString());
                htmlContent = htmlContent.Replace("{enddate}", enddate.Date.ToString());
                htmlContent = htmlContent.Replace("{Transactions}", sb.ToString());
                htmlContent = htmlContent.Replace("{TotalDebit}", debitAmount.ToString());
                htmlContent = htmlContent.Replace("{TotalCredit}", creditAmount.ToString());
            }

            byte[] pdfBytes = GeneratePdf(htmlContent);
            return controller.File(pdfBytes, "application/pdf", "generated.pdf");

        }

        public async Task<ServiceResponse<string>> SendHistoryToEmail(ControllerBase controller, DateDto dateDto)
        {
            var response = new ServiceResponse<string>();
            try
            {
                DateTime fromdate = DateTime.Parse(dateDto.startDate);
                DateTime todate = DateTime.Parse(dateDto.endDate);
                var frmdate = fromdate.Date.ToString("dd-MMM-yyyy");
                var tdate = todate.Date.ToString("dd-MMM-yyyy");
                IFormFile fileToAttach = (IFormFile)await GenerateHistoryForEmail(controller, dateDto);

                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggedUserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var loggedInUser = await _dataContext.Users.Where(x => x.Id == loggedUserId).FirstOrDefaultAsync();
                    if (loggedInUser != null)
                    {
                        string subject = "P2PWALLET TRANSACTIONS STATEMENT";
                        string MailBody = "<!DOCKTYPE html>" +
                                                "<html>" +
                                                    "<body>" +
                                                    $"<h3>Dear {loggedInUser.FirstName}</h3>" +
                                                    $"<h3>Find attached your statement as requested from {frmdate} to {tdate}</h3>" +
                                                    "</body>" +
                                                "</html>";

                        var sendmail = await _mailService.SendStatementToEmail(loggedInUser.Email, subject, MailBody, fileToAttach);
                        if( sendmail != false)
                        {
                            response.Data = "Successful";
                        }
                        else
                        {
                            throw new Exception("Mail Service failed");
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
    }
}
