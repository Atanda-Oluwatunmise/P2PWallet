using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Interface
{
    public interface INotificationHub
    {
        Task SendTransactionNotification();
        Task SendAlert();
        Task SendMessage(string Username, string Message);
        Task SendChartData();

    }
}
