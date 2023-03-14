using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.DataObjects
{

    public class UserDto
    {
        [Required]
        [StringLength(15, ErrorMessage = "Username too long")]
        public string Username { get; set; } = string.Empty;
        [Required]
        [StringLength(20, ErrorMessage = "Name must not exceed")]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        [StringLength(20, ErrorMessage = "Name must not exceed")]
        public string LastName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        //[RegularExpression(".+\\@.+\\..+", ErrorMessage ="Please enter valid email")]
        public string Email { get; set; } = string.Empty;
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required(AllowEmptyStrings =true)]
        public string Address { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class UserViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

    }



    public class AccountDto
    {
        public string AccountNumber { get; set; } = string.Empty;
        public double Balance { get; set; }
    }

    public class TransferDto
    {
        public string AccountNumber { get; set; } = string.Empty;
        public double Amount { get; set; }
    }

    public class AccountViewModel
    {
        public string AccountNumber { get; set; } = string.Empty;
        public double Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
    }

    public class AccountDetails
    {
        public string AccountName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public double Balance { get; set; }
    }

    public class TransactionsViewModel
    {
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public string SenderAccountNumber { get; set; } = string.Empty;
        public string NameofSender { get; set; } = string.Empty;
        public string RecipientAccountNumber { get; set; } = string.Empty;
        public string NameofRecipient { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime DateofTransaction { get; set; }
    }

    public class TransactionsView
    {
        public string Name { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime DateofTransaction { get; set; }
    }

    public class LoginView
    {
        public string Name { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

}
