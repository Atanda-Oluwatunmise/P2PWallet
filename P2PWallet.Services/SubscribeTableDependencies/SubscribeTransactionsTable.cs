using Microsoft.Extensions.Configuration;
using P2PWallet.Models.Models.Entities;
using P2PWallet.Services.Hubs;
using P2PWallet.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableDependency.SqlClient;

namespace P2PWallet.Services.SubscribeTableDependencies
{
    public class SubscribeTransactionsTable
    {
        SqlTableDependency<Notification> tableDependency;
        public INotificationHub _notificationHub;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _configuration;

        public SubscribeTransactionsTable(IConfiguration configuration, INotificationHub notificationHub, INotificationService notificationService)
        {
            _configuration = configuration;
            _notificationHub = notificationHub;
            _notificationService = notificationService;
        }

        public void SubscribeTableDependency()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            tableDependency = new SqlTableDependency<Notification>(connectionString);
            //generate events for table dependency OnChange and OnError
            tableDependency.OnChanged += TableDependency_OnChanged;
            tableDependency.OnError += TableDependency_OnError;
            //start table dependency
            tableDependency.Start();
        }
        private void TableDependency_OnChanged(object sender, TableDependency.SqlClient.Base.EventArgs.RecordChangedEventArgs<Notification> e)
        {
            //when transaction table data gets modified, this event will be fired
            if (e.ChangeType != TableDependency.SqlClient.Base.Enums.ChangeType.None)
            {
                //when there is a change of data, we call hub method to push the data to clients
                _notificationHub.SendAlert();
            }
        }
        private void TableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
        {
           //log error
           Console.WriteLine($"{nameof(Notification)} SqlTableDependency error: {e.Error.Message}");
        }  
 
    }
}
