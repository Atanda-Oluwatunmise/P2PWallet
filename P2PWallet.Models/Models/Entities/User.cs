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

        }

        [Key]
        public int Id { get; set; }
        public string Username { get; set; }= string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public byte[] Password { get; set; } = new byte[32];
        public byte[] PasswordKey { get; set; } = new byte[32];

        public virtual Account UserAccount { get; set; }
        //public ICollection<Transaction> UserTransaction { get; set; }
        public virtual ICollection<Transaction> UserTransaction { get; set; }
        public virtual ICollection<Transaction> ReceiverTransaction { get; set; }
        public virtual ICollection<Deposit> UserDeposit { get; set; }


        public virtual Pin Userpin { get; set; }
        //public virtual Deposit UserDeposit { get; set; }
    }
}
