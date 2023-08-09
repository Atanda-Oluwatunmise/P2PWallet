using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.Entities
{
    public class SuperAdmin
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] Password { get; set; } = new byte[32];
        public byte[] PasswordKey { get; set; } = new byte[32];
        public string? UserToken { get; set; } = string.Empty;
    }
}
