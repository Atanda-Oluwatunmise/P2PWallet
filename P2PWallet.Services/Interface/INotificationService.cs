using Microsoft.AspNetCore.Mvc;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Interface
{
    public interface INotificationService
    {
        public List<Transactions> GetTransactions();
        public DataTable GetProductDetailsFromDb();
        public List<CreditNotificationView> AlertNotifications(int userNotificationId, bool getUnreadMessages);
        public Task CreateNotification(int ReceiverId, int SenderId, string Currency, decimal Amount);
        public Task<ServiceResponse<NotificationDto>> RetrieveNotificationdetail(RetirieveNotificationDto retirieveNotificationDto);
        public Task<ServiceResponse<List<NotificationView>>> UnreadNotifications();
        public Task CreateLockedUserNotification(int UserId);
        public Task SendKycNotifications(string name, string message, string reason);
        public Task CreateKycNotification(int UserId, string Username, string Filename, string Reason);
        public Task<ServiceResponse<string>> SetNonTrasactionsNoitficationsToTrue(MessageDto messageDto);
        public Task CreateVerifiedUserKycNotification(int UserId, string Username);
        public Task<ServiceResponse<string>> GetTransactionsCount();
        public Task<ServiceResponse<string>> GetAccountsCount();
        Task ReadUnreadMessages(ReadChats readChats);
        Task<ServiceResponse<List<UnreadChats>>> GetUnreadMessages(UnreadChatsDto unreadChatsDto);
        Task<ServiceResponse<string>> GetUnreadMessagesCount();

    }
}
