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
        public GLAccount()
        {
            GLAccountTransactions = new HashSet<GLTransaction>();
        }


        [Key]
        public int Id { get; set; }
        public string GLName { get; set; } = string.Empty;
        public string GLNumber { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        public virtual ICollection<GLTransaction> GLAccountTransactions { get; set; }

    }
}
