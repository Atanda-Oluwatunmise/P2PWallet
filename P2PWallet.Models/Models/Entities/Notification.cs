using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.Entities
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; } 
        public int? SenderUserId { get; set; } 
        public decimal? Amount { get; set; } 
        public string? Currency { get; set; } 
        public string NotificationTitle { get; set; } = string.Empty;
        public string? NotificationBody { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string? Reference { get; set; } = string.Empty;
        public bool IsRead { get; set; }

        public virtual User UserNotification { get; set;}
    }
}
