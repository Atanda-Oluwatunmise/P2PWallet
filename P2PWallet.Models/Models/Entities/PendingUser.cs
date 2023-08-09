using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.Entities
{
    public class PendingUser
    {
        [Key]
        public int Id { get; set; }
        public string? FullName { get; set; }
        public bool Pending { get; set; }
    }
}
