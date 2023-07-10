using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.Entities
{
    public class CurrenciesWallet
    {
            [Key]
            public int Id { get; set; }
            public string? Currencies { get; set; }

            [Column(TypeName = "decimal(18,2)")]
            public decimal? ChargeAmount { get; set; }
    }
}
