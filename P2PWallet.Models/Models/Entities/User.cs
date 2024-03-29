﻿using System;
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
            UserAccount = new HashSet<Account>();
            UserTransaction = new HashSet<Transactions>();
            ReceiverTransaction = new HashSet<Transactions>();
            UserDeposit = new HashSet<Deposit>();
            UserSecurityQuestion = new HashSet<SecurityQuestion>();
            UserResetPassword = new HashSet<ResetPassword>();
            UserResetPin = new HashSet<ResetPin>();
            Userpin = new HashSet<Pin>();
            UserWalletCharge = new HashSet<WalletCharge>();
            NotificationforUser = new HashSet<Notification>();

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
        public bool? IsLocked { get; set; }
        public bool? KycVerified { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public DateTime? UserVerifiedAt { get; set; }
        public string? UserOtp { get; set; }


        public virtual ICollection<Account> UserAccount { get; set; }
        public virtual ImageDetail UserImageDetail { get; set; }
        public virtual ICollection<WalletCharge> UserWalletCharge { get; set; }
        public virtual ICollection<Transactions> UserTransaction { get; set; }
        public virtual ICollection<Transactions> ReceiverTransaction { get; set; }
        public virtual ICollection<Deposit> UserDeposit { get; set; }
        public virtual ICollection<SecurityQuestion> UserSecurityQuestion { get; set; }
        public virtual ICollection<ResetPassword> UserResetPassword { get; set; }
        public virtual ICollection<ResetPin> UserResetPin { get; set; }
        public virtual ICollection<Pin> Userpin { get; set; }
        public virtual ICollection<Notification> NotificationforUser { get; set; }
        //public virtual ICollection <KycDocumentUpload> UsersKycDocument { get; set; }
        
        //public virtual Pin Userpin { get; set; }

    }

}