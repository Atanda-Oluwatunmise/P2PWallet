using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.Entities
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public byte[] Password { get; set; } = new byte[32];
        public byte[] PasswordKey { get; set; } = new byte[32];
        public string? UserToken { get; set; } = string.Empty;
        public bool Disabled { get; set; } = false;
        public bool PasswordChanged { get; set; } = false;
    }
}
