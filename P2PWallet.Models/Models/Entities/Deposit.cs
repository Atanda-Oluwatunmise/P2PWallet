﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.Entities
{
    public class Deposit
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public string TxnRef { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Channel { get; set; } = string.Empty;
        public string? CardType { get; set; } = string.Empty;
        public string? Bank { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? CustomerCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    
        public virtual User DepositUser { get; set; }
    }
}
