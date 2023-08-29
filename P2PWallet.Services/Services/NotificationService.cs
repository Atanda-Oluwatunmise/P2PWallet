using Aspose.Pdf.Operators;
using Dapper;
using DinkToPdf.Contracts;
using MailKit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NPOI.HPSF;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using P2PWallet.Services.Hubs;
using P2PWallet.Services.Interface;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TableDependency.SqlClient.Base.Messages;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace P2PWallet.Services.Services
{
    public class NotificationService:INotificationService
    {
        private readonly IHubContext<NotificationHub> _hub;
        public static User user = new User();
        private readonly DataContext _dataContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IHubContext<NotificationHub> hub, DataContext dataContext)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _hub = hub;
            _logger = logger;
            _dataContext = dataContext;
            // _transactionService = transactionService;
            var connectionString = configuration.GetConnectionString("DefaultConnection");
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
            var query = "SELECT * FROM Transactions where DateofTransaction between '2023-07-01' and '2023-07-10'";
            DataTable dataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                dataTable.Load(reader);
                            }
                        }
                    }
                    return dataTable;
                }
                catch (Exception ex)
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


            public List<CreditNotificationView> AlertNotifications(int userNotificationId, bool getUnreadMessages)
        {
            List<CreditNotificationView> notification = new List<CreditNotificationView>();

                using (IDbConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    if (con.State != ConnectionState.Open) con.Open();
                
                    var notifydata = con.Query<CreditNotificationView>($"SELECT NotificationTitle, NotificationBody, CreatedDate, Reference FROM Notifications WHERE UserId ={userNotificationId} AND IsRead={getUnreadMessages}").ToList();
                    
                    if(notifydata != null && notifydata.Count > 0)
                    {
                        notification = notifydata;
                    }
                }
            return notification;
        }

        public async Task SendKycNotifications(string name, string message, string reason)
        {
            try
            {
                var textinfo = CultureInfo.CurrentCulture.TextInfo;
                await _hub.Clients.All.SendAsync("UserKycNotification", name, textinfo.ToTitleCase(message), reason);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"An error occurred {ex.Message}");
            }
        }

        public async Task SendLockedUserNotification(string receipient, string message)
        {
            try
            {
                await _hub.Clients.All.SendAsync("UserLockedNotification",  receipient, message);
            }
            catch(Exception ex)
            {
                _logger.LogInformation($"An error occurred {ex.Message}");
            }
        }


        public async Task SendNotificationAlert(string receipient, string message, string reference)
        {
            try
            {
                await _hub.Clients.All.SendAsync("ReceiveNotification", receipient, message, reference);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"An error occurred {ex.Message}");
            }
        }

        public async Task CreateVerifiedUserKycNotification(int UserId, string Username)
        {
            try
            {
                var userDetail = await _dataContext.Users.Where(x => x.Id == UserId).FirstOrDefaultAsync();
                if (userDetail != null)
                {
                    var notification = new Notification()
                    {
                        UserId = userDetail.Id,
                        NotificationTitle = $"User is Kyc verified",
                        NotificationBody = $"Hi {Username}, your account has been verified!.",
                        CreatedDate = DateTime.Now,
                        //Reference = ReferenceGenerator(),
                        IsRead = false
                    };

                    var notificationExist = await _dataContext.Notifications.Where(x => x.UserId == UserId && x.NotificationBody.ToLower() == notification.NotificationBody).FirstOrDefaultAsync();
                    if (notificationExist == null)
                    {
                        await _dataContext.Notifications.AddAsync(notification);
                        await _dataContext.SaveChangesAsync();
                        _logger.LogInformation($"{userDetail.Username} account is kyc verified!");
                    }
                    else
                    {
                        return;
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"An Error Occurred...{ex.Message}");
            }
        }

        public async Task CreateKycNotification(int UserId, string Username, string Filename, string Reason)
        {
            try
            {
                var userDetail = await _dataContext.Users.Where(x => x.Id == UserId).FirstOrDefaultAsync();

                if (userDetail != null && Reason != null)
                {
                    var notification = new Notification()
                    {
                        UserId = userDetail.Id,
                        NotificationTitle = $"Kyc document was rejected",
                        NotificationBody = $"{Filename} document was rejected by Admin; Reason: {Reason}.",
                        CreatedDate = DateTime.Now,
                        //Reference = ReferenceGenerator(),
                        IsRead = false
                    };
                    await _dataContext.Notifications.AddAsync(notification);
                    await _dataContext.SaveChangesAsync();
                    await SendKycNotifications(Username, Filename, Reason);
                    _logger.LogInformation($"{userDetail.Username} kyc document has been rejected");

                }

                if (userDetail != null && Reason == null)
                {
                    var notification = new Notification()
                    {
                        UserId = userDetail.Id,
                        NotificationTitle = $"Kyc document has been approved",
                        NotificationBody = $"{Filename} document has been approved by the Admin.",
                        CreatedDate = DateTime.Now,
                        //Reference = ReferenceGenerator(),
                        IsRead = false
                    };
                    await _dataContext.Notifications.AddAsync(notification);
                    await _dataContext.SaveChangesAsync();
                    await SendKycNotifications(Username, Filename, Reason);
                    _logger.LogInformation($"{userDetail.Username} kyc document has been approved");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"An Error Occurred...{ex.Message}");
            }
        }

        public async Task CreateLockedUserNotification(int UserId)
        {
            try
            {
                var userDetail = await _dataContext.Users.Where(x => x.Id == UserId).FirstOrDefaultAsync();
                if (userDetail != null)
                {
                    var notification = new Notification()
                    {
                        UserId = userDetail.Id,
                        NotificationTitle = $"Account is Locked",
                        CreatedDate = DateTime.Now,
                        //Reference = ReferenceGenerator(),
                        IsRead = true
                    };
                    await _dataContext.Notifications.AddAsync(notification);
                    await _dataContext.SaveChangesAsync();
                    await SendLockedUserNotification(userDetail.Username, notification.NotificationTitle);
                    _logger.LogInformation($"{userDetail.Username} account is locked");

                }
            }
            catch(Exception ex) 
            {
                _logger.LogError($"An Error Occurred...{ex.Message}");
            }
        }
        public async Task CreateNotification(int ReceiverId, int SenderId, string Currency, decimal Amount)
        {
            try
            {
                var senderDetail = await _dataContext.Users.Where(x => x.Id == SenderId).FirstOrDefaultAsync();
                var receiverDetail = await _dataContext.Users.Where(x => x.Id == ReceiverId).FirstOrDefaultAsync();
                if (receiverDetail != null)
                {

                    var notification = new Notification()
                    {
                        UserId = receiverDetail.Id,
                        SenderUserId = senderDetail.Id,
                        NotificationTitle = $"Credit Alert of {Currency}{Amount}",
                        NotificationBody = $"{senderDetail.FirstName} {senderDetail.LastName} credited your account with {Currency}{Amount}",
                        Amount = Amount,
                        Currency = Currency,
                        CreatedDate = DateTime.Now,
                        Reference = ReferenceGenerator(),
                        IsRead = false
                    };
                    await _dataContext.Notifications.AddAsync(notification);
                    await _dataContext.SaveChangesAsync();
                    await SendNotificationAlert(receiverDetail.Username, notification.NotificationTitle, notification.Reference);
                    _logger.LogInformation($"{receiverDetail.Email} received notification");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An Error Occurred...{ex.Message}");
            }
        }

        public async Task<ServiceResponse<NotificationDto>> RetrieveNotificationdetail(RetirieveNotificationDto retirieveNotificationDto)
        {
            var response = new ServiceResponse<NotificationDto>();
            try
            {
                var notification = await _dataContext.Notifications.Where(x => x.Reference == retirieveNotificationDto.Reference).FirstOrDefaultAsync();
                var accountdetails = await _dataContext.Accounts.Include("User").Where(x => x.UserId == notification.SenderUserId).FirstOrDefaultAsync();
                if (notification == null)
                { 
                    throw new Exception("Notification does not exist");
                }

                if(notification.IsRead == true)
                {
                    throw new Exception("Notification has been read");
                }

                var data = new NotificationDto
                {
                    Sender = $"{accountdetails.User.FirstName} {accountdetails.User.LastName}",
                    AccountNumber = accountdetails.AccountNumber,
                    Amount = (decimal)notification.Amount,
                    Currency = notification.Currency,
                    Date = notification.CreatedDate
                };
                // set the states to read
                notification.IsRead = true;
                await _dataContext.SaveChangesAsync();
                response.Data = data;
            }
           catch(Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"An Error Occurred...{ex.Message}");
            }
            return response;
        }
        public async Task<ServiceResponse<List<NotificationView>>> UnreadNotifications()
        {
            var response = new ServiceResponse<List<NotificationView>>();
            var notificationList = new List<NotificationView>();
            try
            {
                if(_httpContextAccessor.HttpContext != null)
                {
                    var loggedUserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var userAcc = await _dataContext.Users.Where(x => x.Id == loggedUserId).FirstOrDefaultAsync();
                    if(userAcc != null)
                    {
                        var notification = await _dataContext.Notifications.Where(x => x.UserId == userAcc.Id && x.IsRead == false).ToListAsync();

                        foreach(var a in notification.OrderByDescending(x => x.CreatedDate))
                        {
                            var data = new NotificationView
                            {
                                Id =  a.Id,
                                Message = a.NotificationTitle,
                                NotificationBody = a.NotificationBody,
                                Reference = a.Reference
                            };
                            notificationList.Add(data);
                        }
                    }
                    response.Data = notificationList;                
                }
            }
            catch(Exception ex )
            {
                response.Status = false;
                response.StatusMessage = ex.Message;
                _logger.LogError($"An Error Occurred...{ex.Message}");
            }
            return response;
        }

        public async Task<ServiceResponse<string>> SetNonTrasactionsNoitficationsToTrue(MessageDto messageDto)
        {
            var response = new ServiceResponse<string>();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggedUserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var userAcc = await _dataContext.Users.Where(x => x.Id == loggedUserId).FirstOrDefaultAsync();

                    var notification = await _dataContext.Notifications.Where(x => x.UserId == loggedUserId && x.NotificationBody.ToLower() == messageDto.Message.ToLower() && x.Id == messageDto.Id).FirstOrDefaultAsync();
                    if (notification != null)
                    {
                        notification.IsRead = true;
                        await _dataContext.SaveChangesAsync();
                        _logger.LogInformation($"{notification.NotificationBody} notifocation has been read");
                    }
                }
            }
            catch
            {
                response.Status = false;
            }
            return response;
        }

        public async Task<ServiceResponse<string>> GetTransactionsCount()
        {
            var response = new ServiceResponse<string>();
            try
            {
                if(_httpContextAccessor.HttpContext != null)
                {
                    var loggeduser = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
                    if (loggeduser.ToLower() != "admin")
                        throw new Exception("User Unauthorized");
                    var txnsNo = await _dataContext.Transactions.ToListAsync();
                    response.Data = txnsNo.Count().ToString();
                }
            }
            catch(Exception ex)
            {
                response.Status = false;
                response.Data = ex.Message;
                _logger.LogError($"An Error Occurred...{ex.Message}");
            }
            return response;
        }

        public async Task<ServiceResponse<string>> GetAccountsCount()
        {
            var response = new ServiceResponse<string>();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var loggeduser = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
                    if (loggeduser.ToLower() != "admin")
                        throw new Exception("User Unauthorized");
                    var acntNo = await _dataContext.Accounts.ToListAsync();
                    response.Data = acntNo.Count().ToString();
                }
            }
            catch(Exception ex)
            {
                response.Status = false;
                response.Data = ex.Message;
                _logger.LogError($"An Error Occurred...{ex.Message}");
            }
            return response;
        }

    }
}
