using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.Entities
{
    public class Account
    {
        [Key]
        public int UserId { get; set; }
        public string AccountNumber { get; set; } =string.Empty;
        public double Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
}
