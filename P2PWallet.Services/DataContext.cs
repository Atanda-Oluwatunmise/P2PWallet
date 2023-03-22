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
        { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<User>(entity =>
            //{
            //    entity.HasMany<Transaction>()
            //    .WithOne(c => c.User);
            //});

            //modelBuilder.Entity<Transaction>(entity =>
            //{
            //    entity.HasOne(d => d.User)
            //    .WithMany(p => p.UserTransaction)
            //    .HasForeignKey(d => d.SenderId)
            //    .HasConstraintName("FK_Users_Transactions");
            //});

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasOne(d => d.SenderUser)
                .WithMany(p => p.UserTransaction)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.ReceiverUser)
                .WithMany(p => p.ReceiverTransaction)
                .HasForeignKey(d => d.RecipientId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            });
        }
           public DbSet<Deposit> Deposit { get; set; }
           public DbSet<Pin> Pin { get; set; }
           public DbSet<Account> Accounts { get; set; }
           public DbSet<User> Users { get; set; }
           public DbSet<Transaction> Transactions { get; set; }

    }
}
