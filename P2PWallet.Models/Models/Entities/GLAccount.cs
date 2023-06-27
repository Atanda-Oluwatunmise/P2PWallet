using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.Entities
{
    public class GLAccount
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public int? UserId { get; set; }
        public string? Currency { get; set; } = string.Empty;
        public decimal? Amount { get; set; }

        public virtual User UserGL { get; set; }
    }
}
