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
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasOne(d => d.SenderUser)
                .WithMany(p => p.UserTransaction)
                .HasForeignKey(d => d.SenderId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.ReceiverUser)
                .WithMany(p => p.ReceiverTransaction)
                .HasForeignKey(d => d.RecipientId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Deposit>(entity =>
            {
                entity.HasOne(e => e.DepositUser)
                .WithMany(f => f.UserDeposit)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Pin>(entity =>
            {
                entity.HasOne(e => e.PinUser)
                .WithMany(f => f.Userpin)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<SecurityQuestion>(entity =>
            {
                entity.HasOne(e => e.UserSecurity)
                .WithMany(f => f.UserSecurityQuestion)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            });
            
            modelBuilder.Entity<ResetPassword>(entity =>
            {
                entity.HasOne(e => e.ResetUser)
                .WithMany(f => f.UserResetPassword)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            }); 
            
            modelBuilder.Entity<ResetPin>(entity =>
            {
                entity.HasOne(e => e.ResetUserPin)
                .WithMany(f => f.UserResetPin)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<ImageDetail>(entity =>
            {
                entity.HasOne(e => e.UserImage)
                .WithOne(f => f.UserImageDetail)
                .HasForeignKey<ImageDetail>(c => c.ImageUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            });
        }
           public DbSet<Deposit> Deposit { get; set; }
           public DbSet<Pin> Pin { get; set; }
           public DbSet<Account> Accounts { get; set; }
           public DbSet<User> Users { get; set; }
           public DbSet<Transaction> Transactions { get; set; }
           public DbSet<SecurityQuestion> SecurityQuestions { get; set; }
           public DbSet<ResetPassword> ResetPasswords { get; set; }
           public DbSet<ResetPin> ResetPins { get; set; }
           public DbSet<ImageDetail> ImageDetails { get; set; }

    }
}
