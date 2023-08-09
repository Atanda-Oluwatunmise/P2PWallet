using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using P2PWallet.Models.Models.Entities;
using P2PWallet.Services.Interface;
using P2PWallet.Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Hubs
{
    //
    public class NotificationHub: Hub, INotificationHub
    {
        public static User user = new User();
        private Microsoft.AspNetCore.SignalR.Client.HubConnection hubConnection;
        private readonly IConfiguration _configuration;
       // private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        //private readonly TransactionService transactionService;

        public NotificationHub(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            //_notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
            //notificationService = new NotificationService(configuration);
        }

   
            public async Task SendTransactionNotification()
        {
            //var details = _notificationService.GetTransactions();
            await Clients.Caller.SendAsync("TestData");
            //await Clients.All.
        }

        public async Task SendAlert()
        {
            //var result = string.Empty;
            //if (_httpContextAccessor.HttpContext != null)
            //{
            //    result = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            //}
            //return result;

            //var httpContext = Context.GetHttpContext();
            //var connectionId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            //int userId = Convert.ToInt32(connectionId);
            //bool isRead = false;
            ////get my loggedin Id
            //// get unread notifications 
            //var details = _notificationService.AlertNotifications(userId, isRead);
            ////string details = "list of unread notifications";
            await Clients.All.SendAsync("TransactionsData", "Hello People");
            //await Clients.All.
        }

    }
}
