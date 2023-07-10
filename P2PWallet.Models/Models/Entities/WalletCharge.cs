using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.Entities
{
    public class WalletCharge
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public int? UserId { get; set; }
        public string? Currency { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Amount { get; set; }

        public virtual User UserWalletCharge { get; set; }
    }
}
