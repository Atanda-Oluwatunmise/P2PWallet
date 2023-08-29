using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.Entities
{
    public class Chat
    {
        [Key]
        public int Id { get; set; }
        public int? SenderUserId { get; set; }
        public int? ReceiverUserId { get; set; }
        public string SenderUsername { get; set; }
        public string ReceiverUsername { get; set; }
        public string Message { get; set; }
        public DateTime DateofChat { get; set; }
    }
}
