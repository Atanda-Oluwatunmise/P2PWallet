using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.Entities
{
    public class GLTransaction
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("SystemGL")]
        public int GlId { get; set; }
        public string? GlAccount { get; set;} = string.Empty;
        public string? Currency { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Amount { get; set;}
        public string? Type { get; set;} = string.Empty;
        public string? Reference { get; set;} = string.Empty;
        public string? Narration { get; set; } = string.Empty;
        public DateTime? Date { get; set; }

        public virtual GLAccount SystemGL { get; set; }

    }
}
