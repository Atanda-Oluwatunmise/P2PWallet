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

    }
}
