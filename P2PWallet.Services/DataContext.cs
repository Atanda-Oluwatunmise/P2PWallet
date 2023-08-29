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
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasOne(e => e.User)
                .WithMany(a => a.UserAccount)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            });
            
            modelBuilder.Entity<WalletCharge>(entity =>
            {
                entity.HasOne(e => e.UserWalletCharge)
                .WithMany(a => a.UserWalletCharge)
                .HasForeignKey(a => a.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasOne(a => a.UserNotification)
                .WithMany(b => b.NotificationforUser)
                .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Notification>().ToTable(tb => tb.HasTrigger("NotificationAlert"));

            modelBuilder.Entity<Transactions>(entity =>
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
            
            modelBuilder.Entity<GLTransaction>(entity =>
            {
                entity.HasOne(e => e.SystemGL)
                .WithMany(f => f.GLAccountTransactions)
                .HasForeignKey(c => c.GlId)
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

            modelBuilder.Entity <KycDocumentUpload>(entity =>
            {
                entity.HasOne(a => a.UserkycDocumentList)
                .WithMany(b => b.UserKycDocumentUploaded)
                .HasForeignKey(c => c.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Chat>()
                .HasIndex(p => new { p.SenderUsername, p.ReceiverUsername, p.DateofChat})
                .IsDescending();
            
        }
           public DbSet<Deposit> Deposit { get; set; }
           public DbSet<Pin> Pin { get; set; }
           public DbSet<GLAccount> GLAccounts { get; set; }
           public DbSet<Account> Accounts { get; set; }
           public DbSet<User> Users { get; set; }
           public DbSet<Transactions> Transactions { get; set; }
           public DbSet<SecurityQuestion> SecurityQuestions { get; set; }
           public DbSet<ResetPassword> ResetPasswords { get; set; }
           public DbSet<ResetPin> ResetPins { get; set; }
           public DbSet<Notification> Notifications { get; set; }
           public DbSet<ImageDetail> ImageDetails { get; set; }
           public DbSet<CurrenciesWallet> CurrenciesWallets { get; set; }
           public DbSet<WalletCharge> WalletCharges { get; set; }
           public DbSet<SuperAdmin> SuperAdmins { get; set; }
           public DbSet<Admin> Admins { get; set; }
           public DbSet<LockedUser> LockedUsers { get; set; }
           public DbSet<GLTransaction> GLTransactions { get; set; }
           public DbSet<KycDocumentUpload> KycDocumentUploads { get; set; }
           public DbSet<KycDocument> KycDocuments { get; set; }
           public DbSet<DocumentStatusCode> DocumentStatusCodes { get; set; }
           public DbSet<PendingUser> PendingUsers { get; set; }      
           public DbSet<Chat> Chats { get; set; }      
           public DbSet<StartedChat> StartedChats { get; set; }      
    }
}
