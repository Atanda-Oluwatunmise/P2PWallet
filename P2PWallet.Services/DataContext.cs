using Microsoft.EntityFrameworkCore;
using P2PWallet.Models.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services
{
    public class DataContext: DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

           public DbSet<Account> Accounts { get; set; }
           public DbSet<User> Users { get; set; }

    }
}
