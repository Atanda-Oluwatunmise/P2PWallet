using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.Entities
{
    public class LockedUser
    {
        [Key]
        public int Id { get; set; }
        public int? UserId { get; set; } 
        public string? Name { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? Reason { get; set; } = string.Empty;
        public string? No_of_Accounts { get; set; } = string.Empty;
        public string? AccountTier { get; set; } = string.Empty;
        public DateTime LockingDate { get; set; }
        
    }
}
