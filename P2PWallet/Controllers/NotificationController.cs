using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using P2PWallet.Services.Interface;
using P2PWallet.Services.Services;

namespace P2PWallet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController:ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly INotificationService _notificationService;
        public NotificationController(DataContext dataContext, INotificationService notificationService) 
        {
            _dataContext = dataContext;
            _notificationService = notificationService;
        }

        [HttpPost("retrievenotification"), Authorize]
        public async Task<ServiceResponse<NotificationDto>> RetrieveNotificationdetail(RetirieveNotificationDto retirieveNotificationDto)
        {
            var result = await _notificationService.RetrieveNotificationdetail(retirieveNotificationDto);
            return result;
        }

        [HttpGet("unreadnotification"), Authorize]
        public async Task<ServiceResponse<List<NotificationView>>> UnreadNotifications()
        {
            var result = await _notificationService.UnreadNotifications();
            return result;
        }

        [HttpPost("setnotificationtotrue"), Authorize]
        public async Task<ServiceResponse<string>> SetNonTrasactionsNoitficationsToTrue(MessageDto messageDto)
        {
            var result = await _notificationService.SetNonTrasactionsNoitficationsToTrue(messageDto);
            return result;
        }

        [HttpGet("txnscount"), Authorize]
        public async Task<ServiceResponse<string>> GetTransactionsCount()
        {
            var result = await _notificationService.GetTransactionsCount();
            return result;
        }

        [HttpGet("acntscount"), Authorize]
        public async Task<ServiceResponse<string>> GetAccountsCount()
        {
            var result = await _notificationService.GetAccountsCount();
            return result;
        }

        [HttpPost("getunreadchats"), Authorize]
        public async Task<ServiceResponse<List<UnreadChats>>> GetUnreadMessages(UnreadChatsDto unreadChatsDto)
        {
            var result = await _notificationService.GetUnreadMessages(unreadChatsDto);
            return result;
        }

        [HttpPost("readunreadchats"), Authorize]
        public async Task ReadUnreadMessages(ReadChats readChats)
        {
           await _notificationService.ReadUnreadMessages(readChats);
        }

        [HttpGet("unreadchatscount"), Authorize]
        public async Task<ServiceResponse<string>> GetUnreadMessagesCount()
        {
            var result = await _notificationService.GetUnreadMessagesCount();
            return result;
        }

    }
}
