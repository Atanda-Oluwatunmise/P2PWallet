using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.Entities
{
    public class SecurityQuestion
    {
        public int Id { get; set; }
        [ForeignKey("Users")]
        public int UserId { get; set; } 
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public virtual User UserSecurity { get; set; }
    }

}
