using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using P2PWallet.Services;
using P2PWallet.Services.Hubs;
using P2PWallet.Services.Interface;
using P2PWallet.Services.Services;

namespace P2PWalletTestsApi
       
{
    public class TransactionServiceTests
    { 

        private readonly TransactionService _sut;
        private readonly Mock<IHubContext<NotificationHub>> _hubContext = new Mock<IHubContext<NotificationHub>>();
        private readonly Mock<DataContext> _dataContext = new Mock<DataContext>();
        private readonly Mock<ILogger<TransactionService>> _ilogger = new Mock<ILogger<TransactionService>>();
        private readonly Mock<IConfiguration> _iconfiguration = new Mock<IConfiguration>();
        private readonly Mock<IHttpContextAccessor> _ihttpContextAccessor = new Mock<IHttpContextAccessor>();
        private readonly Mock<IMailService> _imailService = new Mock<IMailService>();
        private readonly Mock<IWebHostEnvironment> _iwebHostEnvironment = new Mock<IWebHostEnvironment>();
        private readonly Mock<IConverter> _converter = new Mock<IConverter>();
        private readonly Mock<INotificationService> _inotificationService = new Mock<INotificationService>();
        public TransactionServiceTests()
        {
            _sut = new TransactionService(_hubContext.Object, _dataContext.Object, _ilogger.Object, _iconfiguration.Object, _ihttpContextAccessor.Object, _imailService.Object, _iwebHostEnvironment.Object, _converter.Object, _inotificationService.Object);
        }

        [Fact]
        public async Task TransactionHistory_ReturnAllUserTransactions()
        {
            //arrange
            var count = 2;


            //act
            var transaction = await _sut.TransactionsHistory();

            //assert
            Assert.NotEqual(count.ToString(), transaction.Count.ToString());
        }
    }
}