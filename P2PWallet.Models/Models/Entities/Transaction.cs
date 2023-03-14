﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.Entities
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Users")]
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public string SenderAccountNumber { get; set; } = string.Empty;
        public string NameofSender { get; set; } = string.Empty;
        public string RecipientAccountNumber { get; set;} = string.Empty;
        public string NameofRecipient { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime DateofTransaction { get; set; }

        public virtual User User { get; set; }
    }
}
