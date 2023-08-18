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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TimerService _timerService;

        public NotificationHub(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, TimerService timerService)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _timerService = timerService;
        }

        public async Task SendTransactionNotification()
        {
            await Clients.Caller.SendAsync("TestData");
        }

        public async Task SendAlert()
        {
            await Clients.All.SendAsync("TransactionsData", "Hello People");
        }

        public async Task SendMessage(string Username, string Message)
        {
            await Clients.All.SendAsync("ReceiveMessage", Username, Message, DateTime.Now);
        }

        public async Task SendChartData()
        {
            await Clients.All.SendAsync("TransferChartData", "transactionsdata");
            //if (!_timerService.IsTimerStarted)
            //  _timerService.PrepareTimer(() => Clients.All.SendAsync("TransferChartData", DataManager.GetData()));

        }
    }
}
