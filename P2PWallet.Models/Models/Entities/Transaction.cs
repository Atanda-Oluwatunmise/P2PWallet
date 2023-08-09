using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.Entities
{
    public class Transactions
    {
        [Key]
        public int Id { get; set; }

        //[ForeignKey("Users")]
        public int? SenderId { get; set; }
        //[ForeignKey("Users")]
        public int? RecipientId { get; set; }
        public string? SenderAccountNumber { get; set; } = string.Empty;
        public string? RecipientAccountNumber { get; set;} = string.Empty;
        public string Reference { get;set; } = string.Empty;
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime DateofTransaction { get; set; } = DateTime.Now;

        [ForeignKey("SenderId")]
        public virtual User SenderUser { get; set; }
        [ForeignKey("RecipientId")]
        public virtual User ReceiverUser { get; set; }
    }
}
