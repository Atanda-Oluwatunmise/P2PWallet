using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.Entities
{

    public class User
    {
        public User()
        {
            //UserAccount = new HashSet<Account>();
            UserTransaction = new HashSet<Transaction>();
            ReceiverTransaction = new HashSet<Transaction>();
            UserDeposit = new HashSet<Deposit>();
            UserSecurityQuestion = new HashSet<SecurityQuestion>();
            UserResetPassword = new HashSet<ResetPassword>();
            UserResetPin = new HashSet<ResetPin>();
            Userpin = new HashSet<Pin>();
            //UserImageDetail = new HashSet<ImageDetail>();

        }

        [Key]
        public int Id { get; set; }
        public string? Username { get; set; }= string.Empty;
        public string? FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;
        public byte[]? Password { get; set; } = new byte[32];
        public byte[]? PasswordKey { get; set; } = new byte[32];
        public string? VerificationToken { get; set; } = string.Empty;
        public string? UserToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; } = string.Empty;
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public DateTime? UserVerifiedAt { get; set; }


        public virtual Account UserAccount { get; set; }
        public virtual ImageDetail UserImageDetail { get; set; }
        public virtual ICollection<Transaction> UserTransaction { get; set; }
        public virtual ICollection<Transaction> ReceiverTransaction { get; set; }
        public virtual ICollection<Deposit> UserDeposit { get; set; }
        public virtual ICollection<SecurityQuestion> UserSecurityQuestion { get; set; }
        public virtual ICollection<ResetPassword> UserResetPassword { get; set; }
        public virtual ICollection<ResetPin> UserResetPin { get; set; }
        public virtual ICollection<Pin> Userpin { get; set; }
        
        //public virtual Pin Userpin { get; set; }

    }

}