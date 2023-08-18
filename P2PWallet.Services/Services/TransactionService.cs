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
using System.IO;
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
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Aspose.Pdf;
using Aspose.Cells;
using Azure;
using Org.BouncyCastle.Utilities;
using static NPOI.HSSF.Util.HSSFColor;
using NPOI.SS.Util;
using Aspose.Cells.Drawing;
using HorizontalAlignment = NPOI.SS.UserModel.HorizontalAlignment;
using VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment;
using NPOI.HSSF.Util;
using NPOI.XWPF.UserModel;
using ICell = NPOI.SS.UserModel.ICell;
using FillPattern = NPOI.SS.UserModel.FillPattern;
using PictureType = NPOI.SS.UserModel.PictureType;
using Microsoft.Extensions.Logging;
using P2PWallet.Models.Models.DataObjects.WebHook;
using P2PWallet.Services.Migrations;
using NPOI.POIFS.Crypt.Dsig;
using System.Data;
using Microsoft.Data.SqlClient;
using Octokit;
using Notification = P2PWallet.Models.Models.Entities.Notification;
using Account = P2PWallet.Models.Models.Entities.Account;
using User = P2PWallet.Models.Models.Entities.User;
using NPOI.SS.Formula.Functions;
using Microsoft.AspNetCore.SignalR;
using P2PWallet.Services.Hubs;
//using Octokit;

namespace P2PWallet.Services.Services
{
    public class TransactionService: ITransactionService
    {
        private readonly IHubContext<NotificationHub> _hub;
        private string connectionString;
        public static User user = new User();
        public static Account account = new Account();
        private readonly DataContext _dataContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMailService _mailService;
        private readonly INotificationService _notificationService;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IConverter _converter;
        private readonly ILogger _logger;
        static readonly HttpClient client = new HttpClient();

        public TransactionService(IHubContext<NotificationHub> hub, DataContext dataContext, ILogger<TransactionService> logger,  IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IMailService mailService, IWebHostEnvironment hostEnvironment, IConverter converter, INotificationService notificationService)
        {
            _dataContext = dataContext;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _mailService = mailService;
            _hostEnvironment = hostEnvironment;
            _converter = converter;
            _logger = logger;
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            _notificationService = notificationService;
            _hub = hub;
        }

        public List<Transactions> GetTransactions()
        {
            List<Transactions> transactions = new List<Transactions>();
            Transactions transaction;

            var data = GetProductDetailsFromDb();

            foreach (DataRow row in data.Rows)
            {
                transaction = new Transactions()
                {
                    Id = Convert.ToInt32(row["Id"]),
                    SenderId = Convert.ToInt32(row["SenderId"]),
                    RecipientId = Convert.ToInt32(row["RecipientId"]),
                    SenderAccountNumber = row["SenderAccountNumber"].ToString(),
                    RecipientAccountNumber = row["RecipientAccountNumber"].ToString(),
                    Reference = row["Reference"].ToString(),
                    Amount = Convert.ToDecimal(row["Amount"]),
                    Currency = row["Currency"].ToString(),
                    DateofTransaction = (DateTime)row["DateofTransaction"],
                };
                transactions.Add(transaction);
                if (transactions.Count >= 4)
                    break;

            }
            return transactions;
        }
        public DataTable GetProductDetailsFromDb() 
        {
            var query = "SELECT * FROM Transactions";
            DataTable dataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                try
                {
                    connection.Open();
                    using(SqlCommand command =  new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            dataTable.Load(reader);
                        }
                    }
                    return dataTable;
                }
                catch(Exception ex)
                {
                    throw;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public string ReferenceGenerator()
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
                var recieverUser = await _dataContext.Accounts.Include("User").Where(x => x.AccountNumber == transferdto.AccountSearch 
                                                                            || x.User.Username == transferdto.AccountSearch
                                                                            || x.User.Email == transferdto.AccountSearch).FirstOrDefaultAsync();

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

                decimal amount = transferdto.Amount;

                var loggedUserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                var senderaccount = _dataContext.Accounts.Include("User").Where(x => x.UserId == loggedUserId).FirstOrDefault();

                //var senderaccount = _httpContextAccessor.HttpContext.User.FindFirstValue("AccountNumber");
                var userAccountNumber = _dataContext.Accounts.Include("User").Where(x => x.AccountNumber == senderaccount.AccountNumber).FirstOrDefault();

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

                decimal receipientamount = receipientaccount.Balance + amount;
                decimal newBalance = receipientamount;
                var receiverAccount = _dataContext.Accounts.Include("User").Where(x => x.AccountNumber == receipientaccount.AccountNumber).FirstOrDefault();
                receiverAccount.AccountNumber = receipientaccount.AccountNumber;
                receiverAccount.Balance = newBalance;
                _dataContext.Accounts.Update(receiverAccount);
                await _dataContext.SaveChangesAsync();

                var txns = new Transactions()
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



                await _notificationService.CreateNotification(receiverAccount.UserId, senderaccount.UserId, userAccountNumber.Currency, amount);


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

        public int GetUserId()
        {
            int loggedUserId = 0;
            if (_httpContextAccessor.HttpContext != null)
            {
                loggedUserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            }
            return loggedUserId;
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
                        if (txn.RecipientId == txn.SenderId && txn.Currency.ToLower() == "ngn")
                        {
                            var debitdata = new TransactionsView()
                            {
                                SenderInfo = txn.SenderUser.FirstName + " " + txn.SenderUser.LastName + " - " + txn.SenderAccountNumber,
                                Currency = txn.Currency,
                                TxnAmount = txn.Amount,
                                TransType = "DEBIT",
                                ReceiverInfo = "WALLET FUNDING",
                                DateofTransaction = txn.DateofTransaction
                            };
                            transactions.Add(debitdata);
                        }

                            if (txn.RecipientId == null)
                        {
                            var debitdata = new TransactionsView()
                            {
                                SenderInfo = txn.SenderUser.FirstName + " " + txn.SenderUser.LastName + " - " + txn.SenderAccountNumber,
                                Currency = txn.Currency,
                                TxnAmount = txn.Amount,
                                TransType = "DEBIT",
                                ReceiverInfo = "WALLET CHARGE",
                                DateofTransaction = txn.DateofTransaction
                            };
                            transactions.Add(debitdata);
                        }
                        if (txn.RecipientId != null && txn.RecipientId != txn.SenderId)
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
                    }

                    var trxns = await _dataContext.Transactions.Include("SenderUser").Include("ReceiverUser")
                        .Where(x => x.RecipientId == loggedUserId).ToListAsync();

                    foreach (var txn in trxns)
                    {
                        if (txn.SenderId == txn.RecipientId && txn.Currency.ToLower() != "ngn")
                        {
                            var creditdata = new TransactionsView()
                            {
                                SenderInfo = "WALLET FUNDING",
                                Currency = txn.Currency,
                                TxnAmount = txn.Amount,
                                TransType = "CREDIT",
                                ReceiverInfo = txn.ReceiverUser.FirstName + " " + txn.ReceiverUser.LastName + " - " + txn.RecipientAccountNumber,
                                DateofTransaction = txn.DateofTransaction
                            };
                            transactions.Add(creditdata);
                        }
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
                        if (txn.SenderId != null && txn.RecipientId != txn.SenderId)
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


        public async Task<ServiceResponse<List<TransactionsView>>> UserTransactionsByDate(NewDateDto dateDto)
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

        public async Task<List<TransactionsView>> TransactionHistoryForPdf(string currencyObj)
        {
            List<TransactionsView> transactions = new List<TransactionsView>();
            var loggedUserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (loggedUserId != null)
            {
                var txnss = await _dataContext.Transactions.Include("ReceiverUser").Include("SenderUser")
                    .Where(x => x.SenderId == loggedUserId && x.Currency == currencyObj).ToListAsync();

                foreach (var txn in txnss)
                {
                    if (txn.RecipientId == null)
                    {
                        var debitdata = new TransactionsView()
                        {
                            SenderInfo = txn.SenderUser.FirstName + " " + txn.SenderUser.LastName + " - " + txn.SenderAccountNumber,
                            Currency = txn.Currency,
                            TxnAmount = txn.Amount,
                            TransType = "DEBIT",
                            ReceiverInfo = "WALLET CHARGE",
                            DateofTransaction = txn.DateofTransaction
                        };
                        transactions.Add(debitdata);
                    }
                    if (txn.RecipientId != null)
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
                }

                var trxns = await _dataContext.Transactions.Include("SenderUser").Include("ReceiverUser")
                    .Where(x => x.RecipientId == loggedUserId && x.Currency == currencyObj).ToListAsync();

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
                            ReceiverInfo = $"{txn.ReceiverUser.FirstName} {txn.ReceiverUser.LastName}-{txn.RecipientAccountNumber}",
                            DateofTransaction = txn.DateofTransaction
                        };
                        transactions.Add(creditdata);
                    }
                    if (txn.SenderId != null)
                    {
                        var creditdata = new TransactionsView()
                        {
                            SenderInfo = $"{txn.SenderUser.FirstName} {txn.SenderUser.LastName}-{txn.SenderAccountNumber}",
                            Currency = txn.Currency,
                            TxnAmount = txn.Amount,
                            TransType = "CREDIT",
                            ReceiverInfo = $"{txn.ReceiverUser.FirstName} {txn.ReceiverUser.LastName}-{txn.RecipientAccountNumber}",
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
            var accttxns = await _dataContext.Accounts.Include("User").Where(x => x.UserId == loggedUserId).FirstOrDefaultAsync();
            var userFullName = $"{txns.FirstName} {txns.LastName} ";
            var date = DateTime.Now.ToShortDateString();
            var url = _hostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                   + "transactionImage" + Path.DirectorySeparatorChar.ToString() + "bankifyimg.png";
            var PathToFile = _hostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                   + "transactionStatementTemplate" + Path.DirectorySeparatorChar.ToString() + "transactionsTemp.html";
            int sn = 1;

            var chars = "0123456789";
            var stringChars = new char[12];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new string(stringChars);
            var transactions = await TransactionHistoryForPdf(trasactionDto.Currency);
            decimal totalcredit = 0;
            decimal totaldebit = 0;

            foreach (var transaction in transactions )
            {
                if (transaction != null && transaction.TransType.ToLower() == "credit")
                {
                    totalcredit += transaction.TxnAmount;
                }
                else if (transaction != null && transaction.TransType.ToLower() == "debit")
                {
                    totaldebit += transaction.TxnAmount;
                }
            }

            decimal openingbal = (accttxns.Balance - totalcredit) + totaldebit;
            decimal closingbal = (openingbal + totalcredit) - totaldebit;


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
            decimal creditAmount = 0;
            decimal debitAmount = 0;
            StringBuilder sb = new StringBuilder();
            foreach (var txn in transactionsHolder)
            {
                sb.Append("<tr>");
                sb.Append("<td>");
                sb.Append(sn++.ToString());
                sb.Append("</td>");
                sb.Append("<td>");
                sb.Append(txn.DateofTransaction.ToString("dd-MM-yyyy hh:mm:ss tt"));
                sb.Append("</td>");
                sb.Append("<td colspan='2'>");
                if (txn.SenderInfo.Contains(userFullName) && txn.TransType == "DEBIT")
                {
                    sb.Append(txn.SenderInfo);
                    sb.Append("</td>");
                    sb.Append("<td colspan='2'>");
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
                    sb.Append("<td colspan='2'>");
                    sb.Append(txn.ReceiverInfo);
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
                    sb.Append("<td colspan='2'>");
                    sb.Append(txn.ReceiverInfo);
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
                htmlContent = htmlContent.Replace("{Currency}", accttxns.Currency);
                htmlContent = htmlContent.Replace("{CurrentBalance}", accttxns.Balance.ToString());
                htmlContent = htmlContent.Replace("{OpeningBalance}", openingbal.ToString());
                htmlContent = htmlContent.Replace("{ClosingBalance}", closingbal.ToString());
                htmlContent = htmlContent.Replace("{startdate}", startdate.Date.ToString());
                htmlContent = htmlContent.Replace("{enddate}", enddate.Date.ToString());
                htmlContent = htmlContent.Replace("{Transactions}", sb.ToString());
                htmlContent = htmlContent.Replace("{TotalDebit}", debitAmount.ToString());
                htmlContent = htmlContent.Replace("{TotalCredit}", creditAmount.ToString());
            }

            byte[] pdfBytes = GeneratePdf(htmlContent);
            return controller.File(pdfBytes, "application/pdf", "generated.pdf");

        }
        public async Task<IFormFile> GenerateHistoryForEmail(DateDto trasactionDto)
        {
            DateTime startdate = DateTime.Parse(trasactionDto.startDate);
            DateTime enddate = DateTime.Parse(trasactionDto.endDate);
            var frmdate = startdate.Date.ToString("dd-MMM-yyyy");
            var tdate = enddate.Date.ToString("dd-MMM-yyyy");
            List<TransactionsView> newtransactions = new List<TransactionsView>();
            List<TransactionsView> transactionsHolder = new List<TransactionsView>();
            List<TransactionsView> holder = new List<TransactionsView>();

            var loggedUserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var txns = await _dataContext.Users.Include("UserAccount").Where(x => x.Id == loggedUserId).FirstOrDefaultAsync();
            var accttxns = await _dataContext.Accounts.Include("User").Where(x => x.UserId == loggedUserId).FirstOrDefaultAsync();
            var userFullName = $"{txns.FirstName} {txns.LastName} ";
            var date = DateTime.Now.ToShortDateString();
            var url = _hostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                   + "transactionImage" + Path.DirectorySeparatorChar.ToString() + "bankifyimg.png";
            var PathToFile = _hostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                   + "transactionStatementTemplate" + Path.DirectorySeparatorChar.ToString() + "transactionsTemp.html";
            int sn = 1;

            var chars = "0123456789";
            var stringChars = new char[12];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new string(stringChars);
            var transactions = await TransactionHistoryForPdf(trasactionDto.currency);
            decimal totalcredit = 0;
            decimal totaldebit = 0;

            foreach (var transaction in transactions)
            {
                if (transaction != null && transaction.TransType.ToLower() == "credit")
                {
                    totalcredit += transaction.TxnAmount;
                }
                else if (transaction != null && transaction.TransType.ToLower() == "debit")
                {
                    totaldebit += transaction.TxnAmount;
                }
            }
            decimal openingbal = (accttxns.Balance - totalcredit) + totaldebit;
            decimal closingbal = (openingbal + totalcredit) - totaldebit;


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
            decimal creditAmount = 0;
            decimal debitAmount = 0;
            StringBuilder sb = new StringBuilder();
            foreach (var txn in transactionsHolder)
            {
                sb.Append("<tr>");
                sb.Append("<td>");
                sb.Append(sn++.ToString());
                sb.Append("</td>");
                sb.Append("<td>");
                sb.Append(txn.DateofTransaction.ToString("dd-MM-yyyy hh:mm:ss tt"));
                sb.Append("</td>");
                sb.Append("<td colspan='3'>");
                if (txn.SenderInfo.Contains(userFullName) && txn.TransType == "DEBIT")
                {
                    sb.Append(txn.SenderInfo);
                    sb.Append("</td>");
                    sb.Append("<td>");
                    sb.Append(txn.ReceiverInfo);
                    sb.Append("</td>");
                    sb.Append("<td>");
                    sb.Append($"NGN{txn.TxnAmount}");
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
                    sb.Append(txn.ReceiverInfo);
                    sb.Append("</td>");  
                    sb.Append("<td>");
                    sb.Append("-");
                    sb.Append("</td>");
                    sb.Append("<td>");
                    sb.Append($"NGN{txn.TxnAmount}");
                    creditAmount += txn.TxnAmount;
                }
                else
                {
                    sb.Append(txn.SenderInfo);
                    sb.Append("</td>");
                    sb.Append("<td>");
                    sb.Append(txn.ReceiverInfo);
                    sb.Append("</td>");
                    sb.Append("<td>");
                    sb.Append("-");
                    sb.Append("</td>");
                    sb.Append("<td>");
                    sb.Append($"NGN{txn.TxnAmount}");
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
                htmlContent = htmlContent.Replace("{Currency}", accttxns.Currency);
                htmlContent = htmlContent.Replace("{CurrentBalance}", accttxns.Balance.ToString());
                htmlContent = htmlContent.Replace("{OpeningBalance}", openingbal.ToString());
                htmlContent = htmlContent.Replace("{ClosingBalance}", closingbal.ToString());
                htmlContent = htmlContent.Replace("{startdate}", frmdate);
                htmlContent = htmlContent.Replace("{enddate}", tdate);
                htmlContent = htmlContent.Replace("{Transactions}", sb.ToString());
                htmlContent = htmlContent.Replace("{TotalDebit}", debitAmount.ToString());
                htmlContent = htmlContent.Replace("{TotalCredit}", creditAmount.ToString());
            }

            byte[] pdfBytes = GeneratePdf(htmlContent);
            var stream = new MemoryStream(pdfBytes);
            var memorystream = new MemoryStream();
            stream.CopyTo(memorystream);
            memorystream.Position = 0;
            return new FormFile(memorystream, 0, memorystream.Length, null, "generated.pdf")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };
            //return controller.File(pdfBytes, "application/pdf", "generated.pdf");

        }

        public async Task<ServiceResponse<string>> SendHistoryToEmail(DateDto dateDto)
        {

            var response = new ServiceResponse<string>();
            try
            {
                DateTime fromdate = DateTime.Parse(dateDto.startDate);
                DateTime todate = DateTime.Parse(dateDto.endDate);
                var frmdate = fromdate.Date.ToString("dd-MMM-yyyy");
                var tdate = todate.Date.ToString("dd-MMM-yyyy");
                IFormFile fileToAttach = await GenerateHistoryForEmail(dateDto);

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
                                                    $"<h3>Dear {loggedInUser.FirstName},</h3>" +
                                                    $"<h5>Find your attached PDF transactions statement as requested from {frmdate} to {tdate}.</h5>" +
                                                    $"<br>" +
                                                    $"<h5>Best regards.</h5>" +
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

        public async Task<IFormFile> GenerateExcelFile(DateDto dateDto)
        {
            string filename = @"Transaction_Statement.xlsx";

            var excelstream = new MemoryStream();
            excelstream = await CreateExcelFile(dateDto);

            var stream = new MemoryStream();
            excelstream.CopyTo(stream);

            return new FormFile(stream, 0, stream.Length, null, filename)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
        }
        
        public async Task<MemoryStream> CreateExcelFile(DateDto dateDto)
        {
            List<TransactionsView> newtransactions = new List<TransactionsView>();
            List<TransactionsView> transactionsHolder = new List<TransactionsView>();
            var userId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _dataContext.Users.Where(x => x.Id == userId).FirstOrDefaultAsync();
            if (user != null)
            {

                DateTime fromdate = DateTime.Parse(dateDto.startDate);
                DateTime todate = DateTime.Parse(dateDto.endDate);
                var frmdate = fromdate.Date.ToString("dd-MMM-yyyy");
                var tdate = todate.Date.ToString("dd-MMM-yyyy");

                var transactions = await TransactionHistoryForPdf(dateDto.currency);

                if (todate < fromdate)
                {
                    throw new Exception("endDate must be greater than or equal to startDate");
                }

                if (fromdate <= todate)
                {
                    for (var day = fromdate.Date; day.Date <= todate.Date; day = day.AddDays(1))
                    {
                        newtransactions = transactions.Where(x => x.DateofTransaction.Date == day.Date).ToList();
                        if (newtransactions.Count != 0 && newtransactions != null)
                        {
                            transactionsHolder.AddRange(newtransactions);
                        }
                    }
                }
            }
            transactionsHolder = transactionsHolder.OrderBy(x => x.DateofTransaction).ToList();

            //creating the excel file
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Transactions History");
            //creating the row

            //Get the first sheet
            sheet = workbook.GetSheetAt(0);


            //Add picture data to the workbook
            byte[] bytes = File.ReadAllBytes("C:/Users/OluwatunimiseAtanda/OneDrive - Globus Bank Limited/Documents/Projects/P2PWalletApplication/images/bankifyimg.png");
            workbook.AddPicture(bytes, PictureType.PNG);

            //Add a picture shape and set its position
            IDrawing drawing = sheet.CreateDrawingPatriarch();
            IClientAnchor anchor = workbook.GetCreationHelper().CreateClientAnchor();
            anchor.Dx1 = 0;
            anchor.Dy1 = 0;
            anchor.Col1 = 0;
            anchor.Row1 = 0;
            IPicture picture = drawing.CreatePicture(anchor, 0);

            //Automatically adjust the image size
            picture.Resize(2.0, 2.0);

            var header = sheet.CreateRow(4);
                header.CreateCell(0).SetCellValue("SenderInfo");
                header.CreateCell(1).SetCellValue("Currency");
                header.CreateCell(2).SetCellValue("TxnAmount");
                header.CreateCell(3).SetCellValue("TransType");
                header.CreateCell(4).SetCellValue("ReceiverInfo");
                header.CreateCell(5).SetCellValue("DateofTransaction");

            int rowindex = 5;
                foreach (var item in transactionsHolder)
                {
                var row = sheet.CreateRow(rowindex);
                    row.CreateCell(0).SetCellValue(item.SenderInfo);
                    row.CreateCell(1).SetCellValue(item.Currency);
                    row.CreateCell(2).SetCellValue((double)item.TxnAmount);
                    row.CreateCell(3).SetCellValue(item.TransType);
                    row.CreateCell(4).SetCellValue(item.ReceiverInfo);
                    row.CreateCell(5).SetCellValue(item.DateofTransaction.ToString("dd-MM-yyyy hh:mm:ss tt"));
                rowindex++;
                }

            //Create style
            NPOI.XSSF.UserModel.XSSFCellStyle style = (NPOI.XSSF.UserModel.XSSFCellStyle)workbook.CreateCellStyle();

            string myHexColor = "#53277E";

            byte r = Convert.ToByte(myHexColor.Substring(1, 2).ToUpper(), 16);
            byte g = Convert.ToByte(myHexColor.Substring(3, 2), 16);
            byte b = Convert.ToByte(myHexColor.Substring(5, 2), 16);
            
            // Here we create a color from RGB-values
            IColor color = new NPOI.XSSF.UserModel.XSSFColor(new byte[] { r, g, b });

            //Set border style
            style.BorderBottom = BorderStyle.Medium;
            style.BottomBorderColor = HSSFColor.White.Index;
            style.TopBorderColor = HSSFColor.White.Index;
            style.RightBorderColor = HSSFColor.White.Index;
            style.LeftBorderColor = HSSFColor.White.Index;

            //Set font style
            XSSFFont font = (XSSFFont)workbook.CreateFont();
            font.FontHeightInPoints = (short)10;
            font.Color = IndexedColors.White.Index;
            font.FontName = "Arial";
            font.IsBold = false;
            //font.FontHeight = 13;
            font.IsItalic = false;
            font.Boldweight = 700;

            //Set background color
            style.SetFillForegroundColor((XSSFColor)color);

            style.SetFont(font);
            style.FillPattern = FillPattern.SolidForeground;

            for (int i = header.FirstCellNum; i < header.LastCellNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;
                if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;
                for (int j = sheet.FirstRowNum; j < sheet.LastRowNum; j++)
                {
                    for(int x = 0;  x < row.Cells.Count; x++)
                    {
                        var cell = header.Cells[x];
                        if (row.GetCell(x) != null)
                        {
                            ////Apply the style
                            cell.CellStyle = style;
                            sheet.AutoSizeColumn(x);

                        }
                    }
                
                }
            }

            ICell newcell = sheet.CreateRow(0).CreateCell(3);
            newcell.SetCellType(CellType.String);
            newcell.SetCellValue("Supplier Provided Data");

            var summary = sheet.CreateRow(1);
            summary.CreateCell(10).SetCellValue("Report Summary");
            var colStyle = sheet.GetColumnStyle(10);
            colStyle.WrapText = true;
            colStyle.Alignment = HorizontalAlignment.Center;

            //Merge the cell
            CellRangeAddress region = new CellRangeAddress(0, 3, 0, 5);
            sheet.AddMergedRegion(region);

            //merge cell for summary
            CellRangeAddress summaryregion = new CellRangeAddress(5, 10, 7, 13);
            sheet.AddMergedRegion(summaryregion);

            var memoryStream = new MemoryStream();   //creating memoryStream        
                workbook.Write(memoryStream, true);
                var newmemoryStream = new MemoryStream(memoryStream.ToArray());

                return newmemoryStream;
                //var buffer = memoryStream.ToArray();
                //var bufferLength = buffer.Length;
                //return controller.File(buffer, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
            
        }

        public async Task<ActionResult> DownloadExcelFile(ControllerBase controller, DateDto dateDto)
        {
            string filename = @"Transaction_Statement.xlsx";
            var excelstream = await CreateExcelFile(dateDto);
            using (var memoryStream = new MemoryStream()) //creating memoryStream
            {
                excelstream.CopyTo(memoryStream);
                var buffer = memoryStream.ToArray();
                var bufferLength = buffer.Length;
                return controller.File(buffer, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
            }
        }
            public async Task<ServiceResponse<string>> SendExcelToEmail(DateDto dateDto)
            {
            var response = new ServiceResponse<string>();
            try
            {
                DateTime fromdate = DateTime.Parse(dateDto.startDate);
                DateTime todate = DateTime.Parse(dateDto.endDate);
                var frmdate = fromdate.Date.ToString("dd-MMM-yyyy");
                var tdate = todate.Date.ToString("dd-MMM-yyyy");
               

                IFormFile fileToAttach = await GenerateExcelFile(dateDto);


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
                                                    $"<h3>Dear {loggedInUser.FirstName},</h3>" +
                                                    $"<h5>Find your attached Excel transactions statement as requested from {frmdate} to {tdate}.</h5>" +
                                                    $"<br>" +
                                                    $"<h5>Best regards.</h5>" +
                                                    "</body>" +
                                                "</html>";

                        var sendmail = await _mailService.SendStatementToEmail(loggedInUser.Email, subject, MailBody, fileToAttach);
                        if (sendmail != false)
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
                _logger.LogError($"Error occured.....{ex.Message}");
            }
            return response;
        }

    

        public ConverterView CurrencyConverter(ConverterDto converterDto)
        {        
            //decimal usdToNaira = 774.0558M;
            //decimal eurToNaira = 842.7721M;
            //decimal gbpToNaira = 986.5100M;
            decimal equaivalentNaira = 0;
            var rateaccount = _dataContext.CurrenciesWallets.Where(x => x.Currencies.ToLower() ==  converterDto.Currency.ToLower()).FirstOrDefault();
            if (rateaccount != null)
            {
                equaivalentNaira = (decimal)(rateaccount.Rate * converterDto.Amount);
            }

                //if (converterDto.Currency.ToLower() == "usd")
                //{

                //    equaivalentNaira = usdToNaira * converterDto.Amount;
                //}

                //else if (converterDto.Currency.ToLower() == "eur")
                //{
                //    equaivalentNaira = eurToNaira * converterDto.Amount;
                //}

                //else if (converterDto.Currency.ToLower() == "gbp")
                //{
                //    equaivalentNaira = gbpToNaira * converterDto.Amount;
                //}
                var newdata = new ConverterView()
                {
                    Currency = converterDto.Currency,
                    NairaAmount = equaivalentNaira,
                    WalletAmount = converterDto.Amount
                };

      
            //return the values to the user
       
            //data = newdata;
            return newdata;
        }

        public async Task<ServiceResponse<string>> FundForeignWallet(ConverterDto converterDto)
        {
            var response = new ServiceResponse<string>();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var data = CurrencyConverter(converterDto);
                    var loggeduserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var useraccount = await _dataContext.Users.Include("UserAccount").Where(x => x.Id == loggeduserId).FirstOrDefaultAsync();
                    var usernairaaccount = await _dataContext.Accounts.Include("User").Where(x => x.Id == loggeduserId).FirstOrDefaultAsync();
                    var walletacount = await _dataContext.Accounts.Where(x => x.UserId == loggeduserId && x.Currency.Contains(data.Currency)).FirstOrDefaultAsync();
                    var nairaglaccount = await _dataContext.GLAccounts.Where(x => x.Currency.ToLower() == "ngn").FirstOrDefaultAsync();
                    var dollarglaccount = await _dataContext.GLAccounts.Where(x => x.Currency.ToLower() == data.Currency.ToLower()).FirstOrDefaultAsync();

                    if (walletacount == null)
                    {
                        throw new Exception("Foreign Account does not exist");
                    }
                   
                    if (usernairaaccount.Balance < data.NairaAmount)
                    {
                        throw new Exception("Insufficient Balance");
                    }

                    usernairaaccount.Balance -=  data.NairaAmount;
                    nairaglaccount.Balance += data.NairaAmount;

                    var debittransaction = new Transactions()
                    {
                        SenderId = loggeduserId,
                        RecipientId = walletacount.UserId,
                        SenderAccountNumber = usernairaaccount.AccountNumber,
                        RecipientAccountNumber = walletacount.AccountNumber,
                        Reference = ReferenceGenerator(),
                        Amount = data.NairaAmount,
                        Currency = usernairaaccount.Currency.ToUpper(),
                        DateofTransaction = DateTime.Now
                    };
                    await _dataContext.Transactions.AddAsync(debittransaction);
                    //await _dataContext.SaveChangesAsync();

                    dollarglaccount.Balance -= data.WalletAmount;
                    walletacount.Balance += data.WalletAmount;

                    var creditglhistory = new GLTransaction()
                    {
                        GlId = nairaglaccount.Id,
                        GlAccount = nairaglaccount.GLNumber,
                        Currency = nairaglaccount.Currency,
                        Amount = data.NairaAmount,
                        Type = "CREDIT",
                        Reference = ReferenceGenerator(),
                        Narration = $"{usernairaaccount.User.FirstName} credited  GLWallet with {data.NairaAmount}{nairaglaccount.Currency}." ,
                        Date = DateTime.Now,
                    };
                    await _dataContext.GLTransactions.AddAsync(creditglhistory);


                    var debitglhistory = new GLTransaction()
                    {
                        GlId = dollarglaccount.Id,
                        GlAccount = dollarglaccount.GLNumber,
                        Currency = dollarglaccount.Currency,
                        Amount = data.WalletAmount,
                        Type = "DEBIT",
                        Reference = ReferenceGenerator(),
                        Narration = $"Credited {walletacount.User.FirstName} {walletacount.Currency} Wallet with {data.WalletAmount}{walletacount.Currency}",
                        Date = DateTime.Now,
                    };
                    await _dataContext.GLTransactions.AddAsync(debitglhistory);

                    var credittransaction = new Transactions()
                    {
                        SenderId = loggeduserId,
                        RecipientId = walletacount.UserId,
                        SenderAccountNumber = usernairaaccount.AccountNumber,
                        RecipientAccountNumber = walletacount.AccountNumber,
                        Reference = ReferenceGenerator(),
                        Amount = data.WalletAmount,
                        Currency = data.Currency.ToUpper(),
                        DateofTransaction = DateTime.Now
                    };
                    await _dataContext.Transactions.AddAsync(credittransaction);


                    await _dataContext.SaveChangesAsync();

                    response.Data = "Wallet Funding Successful!";

                    }
            }
            catch(Exception ex) 
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"An Error occurred .......{ex.Message}");                
            }
            return response;
        }

        public async Task<ServiceResponse<string>> ForeignTransfers(ForeignTransferDto foreignTransferDto)
        {
            var response = new ServiceResponse<string>();
            var glaccount = await _dataContext.GLAccounts.Where(x => x.Currency.ToLower() == foreignTransferDto.Currency.ToLower()).FirstOrDefaultAsync();

            try
            {
                if(_httpContextAccessor.HttpContext != null)
                {
                    //verify the user logged in
                    var loggedUserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

                    var useraccount = await _dataContext.Users.Include("UserAccount").Where(x => x.Id == loggedUserId && x.KycVerified == true).FirstOrDefaultAsync();

                    if (useraccount == null)
                    {
                        throw new Exception("User is not Kyc verified, please upgrade your account.");
                    }
                    //get access to the accounts table  //get the sender account
                    var loggedInuserAccount = await _dataContext.Accounts.Include("User").Where(x => x.UserId == loggedUserId && x.Currency == foreignTransferDto.Currency).FirstOrDefaultAsync();

                    if (loggedInuserAccount != null)
                    {
                        //get the receiver's account
                        var recieverUser = await _dataContext.Accounts.Include("User").Where(x => x.AccountNumber == foreignTransferDto.AccountSearch
                                                                || x.User.Username == foreignTransferDto.AccountSearch
                                                                || x.User.Email == foreignTransferDto.AccountSearch).FirstOrDefaultAsync();
                        //get the receiver's account
                        var receiverAcc = await _dataContext.Accounts.Include("User").Where(x => x.UserId == recieverUser.UserId && x.Currency == foreignTransferDto.Currency).FirstOrDefaultAsync();

                        //put the checks
                        if (recieverUser == null)
                        {
                            throw new Exception("Receipient's details cannot be found.");
                        }
                        
                        if (receiverAcc == null)
                        {
                            throw new Exception("The foreign wallet does not exist for recipient");
                        }
                        
                        if(loggedInuserAccount == receiverAcc)
                        {
                            throw new Exception("Cannot make transfers to own accounts");
                        }

                        if(loggedInuserAccount.Balance < foreignTransferDto.Amount)
                        {
                            throw new Exception("Insufficient balance");
                        }
                        if(foreignTransferDto.Amount <= 0)
                        {
                            throw new Exception("Cannot transfer amounts less than or equal to 0");
                        }

                        //remove from the sender account
                        loggedInuserAccount.Balance -= foreignTransferDto.Amount;

                        // add to walletglaccount
                        glaccount.Balance += foreignTransferDto.Amount;

                        // deduct from glaccount
                        glaccount.Balance -= foreignTransferDto.Amount;

                        // add to the receipient account
                        receiverAcc.Balance += foreignTransferDto.Amount;

                        // add the transactions to transactions table
                        var newtransaction = new Transactions()
                        {
                            SenderId = loggedInuserAccount.UserId,
                            RecipientId = receiverAcc.UserId,
                            SenderAccountNumber = loggedInuserAccount.AccountNumber,
                            RecipientAccountNumber = receiverAcc.AccountNumber,
                            Reference = ReferenceGenerator(),
                            Amount = foreignTransferDto.Amount,
                            Currency = foreignTransferDto.Currency.ToUpper(),
                            DateofTransaction = DateTime.Now
                        };

                        await _dataContext.Transactions.AddAsync(newtransaction);
                        await _dataContext.SaveChangesAsync();

                        await _notificationService.CreateNotification(receiverAcc.UserId, loggedInuserAccount.UserId, receiverAcc.Currency, foreignTransferDto.Amount);
                        //var notification = new Notification()
                        //{
                        //    UserId = receiverAcc.UserId,
                        //    SenderUserId = loggedInuserAccount.UserId,
                        //    Amount = foreignTransferDto.Amount,
                        //    Currency = 
                        //    NotificationTitle = $"Credit Alert of {receiverAcc.Currency}{foreignTransferDto.Amount}",
                        //    NotificationBody = $"{receiverAcc.User.FirstName} {receiverAcc.User.LastName} credited your account with {receiverAcc.Currency}{foreignTransferDto.Amount}",
                        //    CreatedDate = DateTime.Now,
                        //    Reference = ReferenceGenerator(),
                        //    IsRead = false
                        //};
                        //await _dataContext.Notifications.AddAsync(notification);
                        //await _dataContext.SaveChangesAsync();



                        //Debit Info
                        var debit_mail = loggedInuserAccount.User.Email;
                        var debit_Name = loggedInuserAccount.User.Username;
                        var debit_Amount = foreignTransferDto.Amount;
                        var debit_Balance = loggedInuserAccount.Balance;
                        var Dtrantype = "Debit";
                        var DebitInfo = "has been sent to";
                        var Ddetails = receiverAcc.User.FirstName + " " + receiverAcc.User.LastName;
                        var DAcc = receiverAcc.AccountNumber;
                        var DDate = newtransaction.DateofTransaction;

                        //Credit Info
                        var credit_mail = receiverAcc.User.Email;
                        var credit_Name = receiverAcc.User.Username;
                        var credit_Amount = foreignTransferDto.Amount;
                        var credit_Balance = receiverAcc.Balance;
                        var CreditInfo = "was received from";
                        var Ctrantype = "Credit";
                        var Cdetails = loggedInuserAccount.User.FirstName + " " + loggedInuserAccount.User.LastName;
                        var CAcc = loggedInuserAccount.AccountNumber;
                        var CDate = newtransaction.DateofTransaction;


                        await _mailService.SendAsync(debit_mail, debit_Name, Dtrantype, debit_Amount.ToString(), DebitInfo, Ddetails, DAcc, DDate.ToString("yyyy-MM-dd"), debit_Balance.ToString());
                        await _mailService.SendAsync(credit_mail, credit_Name, Ctrantype, credit_Amount.ToString(), CreditInfo, Cdetails, CAcc, CDate.ToString("yyyy-MM-dd"), credit_Balance.ToString());

                        response.Data = "Transaction Successful";
                    }
                }


                //save changes
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"An Error occurred .......{{ex.Message");
            }
            return response;
        }

    }

}
