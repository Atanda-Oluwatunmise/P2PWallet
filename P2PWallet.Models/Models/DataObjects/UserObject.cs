using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Models.DataObjects
{

        public class UserDto
        {
            public string Username { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
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
            public string Currency { get; set; } = string.Empty;
        }

        public class AccountViewModel
        {
            public int UserId { get; set; }
            public string AccountNumber { get; set; } = string.Empty;
            public double Balance { get; set; }
            public string Currency { get; set; } = string.Empty; 
        }

        //public class ServiceResponse<T>
        //{
        //    public bool Status { get; set; }
        //    public string StatusMessage { get; set; } = string.Empty;
        //    public T? Data { get; set; }

        //}

}
