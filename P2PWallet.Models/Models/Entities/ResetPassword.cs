using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.Entities
{
    public class ResetPassword
    {
        public int Id { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime TokenExpires { get; set; }
        public virtual User ResetUser { get; set; }
    }
}
 