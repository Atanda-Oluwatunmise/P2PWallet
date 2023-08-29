using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.Entities
{
    public class StartedChat
    {
        [Key]
        public int Id { get; set; }
        public string StartedChatWith { get; set; }
        public string ReceivedChatFrom { get; set; }
        public bool HasStarted { get; set; }
    }
}
