﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using P2PWallet.Services;

#nullable disable

namespace P2PWallet.Services.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20230723204959_NewUpdateandCreateGLTransaction")]
    partial class NewUpdateandCreateGLTransaction
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AccountNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Balance")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.Admin", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Password")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<byte[]>("PasswordKey")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("UserToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Admins");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.CurrenciesWallet", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal?>("ChargeAmount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Currencies")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal?>("Rate")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.ToTable("CurrenciesWallets");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.Deposit", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Bank")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CardType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Channel")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CustomerCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TxnRef")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Deposit");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.GLAccount", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Balance")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("GLName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("GLNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("GLAccounts");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.GLTransaction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal?>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime?>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("GlAccount")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("GlId")
                        .HasColumnType("int");

                    b.Property<string>("Narration")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Reference")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Type")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("GlId");

                    b.ToTable("GLTransactions");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.ImageDetail", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<byte[]>("Image")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("ImageName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ImageUserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ImageUserId")
                        .IsUnique();

                    b.ToTable("ImageDetails");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.LockedUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AccountTier")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("LockingDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("No_of_Accounts")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Reason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("LockedUsers");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.Notifications", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsRead")
                        .HasColumnType("bit");

                    b.Property<string>("NotificationBody")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NotificationTitle")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Reference")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Notifications", t =>
                        {
                            t.HasTrigger("NotificationAlert");
                        });
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.Pin", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<byte[]>("PinKey")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<byte[]>("UserPin")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Pin");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.ResetPassword", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("TokenExpires")
                        .HasColumnType("datetime2");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("ResetPasswords");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.ResetPin", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PinToken")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("PinTokenExpires")
                        .HasColumnType("datetime2");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("ResetPins");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.SecurityQuestion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Answer")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Question")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("SecurityQuestions");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.SuperAdmin", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Password")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<byte[]>("PasswordKey")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("UserToken")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("SuperAdmins");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.Transactions", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("DateofTransaction")
                        .HasColumnType("datetime2");

                    b.Property<string>("RecipientAccountNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("RecipientId")
                        .HasColumnType("int");

                    b.Property<string>("Reference")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SenderAccountNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("SenderId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("RecipientId");

                    b.HasIndex("SenderId");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("IsLocked")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Password")
                        .HasColumnType("varbinary(max)");

                    b.Property<byte[]>("PasswordKey")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RefreshToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("RefreshTokenExpiryTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UserVerifiedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Username")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("VerificationToken")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.WalletCharge", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal?>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Currency")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("WalletCharges");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.Account", b =>
                {
                    b.HasOne("P2PWallet.Models.Models.Entities.User", "User")
                        .WithMany("UserAccount")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.Deposit", b =>
                {
                    b.HasOne("P2PWallet.Models.Models.Entities.User", "DepositUser")
                        .WithMany("UserDeposit")
                        .HasForeignKey("UserId")
                        .IsRequired();

                    b.Navigation("DepositUser");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.GLTransaction", b =>
                {
                    b.HasOne("P2PWallet.Models.Models.Entities.GLAccount", "SystemGL")
                        .WithMany("GLAccountTransactions")
                        .HasForeignKey("GlId")
                        .IsRequired();

                    b.Navigation("SystemGL");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.ImageDetail", b =>
                {
                    b.HasOne("P2PWallet.Models.Models.Entities.User", "UserImage")
                        .WithOne("UserImageDetail")
                        .HasForeignKey("P2PWallet.Models.Models.Entities.ImageDetail", "ImageUserId")
                        .IsRequired();

                    b.Navigation("UserImage");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.Notifications", b =>
                {
                    b.HasOne("P2PWallet.Models.Models.Entities.User", "UserNotification")
                        .WithMany("NotificationforUser")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserNotification");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.Pin", b =>
                {
                    b.HasOne("P2PWallet.Models.Models.Entities.User", "PinUser")
                        .WithMany("Userpin")
                        .HasForeignKey("UserId")
                        .IsRequired();

                    b.Navigation("PinUser");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.ResetPassword", b =>
                {
                    b.HasOne("P2PWallet.Models.Models.Entities.User", "ResetUser")
                        .WithMany("UserResetPassword")
                        .HasForeignKey("UserId")
                        .IsRequired();

                    b.Navigation("ResetUser");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.ResetPin", b =>
                {
                    b.HasOne("P2PWallet.Models.Models.Entities.User", "ResetUserPin")
                        .WithMany("UserResetPin")
                        .HasForeignKey("UserId")
                        .IsRequired();

                    b.Navigation("ResetUserPin");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.SecurityQuestion", b =>
                {
                    b.HasOne("P2PWallet.Models.Models.Entities.User", "UserSecurity")
                        .WithMany("UserSecurityQuestion")
                        .HasForeignKey("UserId")
                        .IsRequired();

                    b.Navigation("UserSecurity");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.Transactions", b =>
                {
                    b.HasOne("P2PWallet.Models.Models.Entities.User", "ReceiverUser")
                        .WithMany("ReceiverTransaction")
                        .HasForeignKey("RecipientId");

                    b.HasOne("P2PWallet.Models.Models.Entities.User", "SenderUser")
                        .WithMany("UserTransaction")
                        .HasForeignKey("SenderId");

                    b.Navigation("ReceiverUser");

                    b.Navigation("SenderUser");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.WalletCharge", b =>
                {
                    b.HasOne("P2PWallet.Models.Models.Entities.User", "UserWalletCharge")
                        .WithMany("UserWalletCharge")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("UserWalletCharge");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.GLAccount", b =>
                {
                    b.Navigation("GLAccountTransactions");
                });

            modelBuilder.Entity("P2PWallet.Models.Models.Entities.User", b =>
                {
                    b.Navigation("NotificationforUser");

                    b.Navigation("ReceiverTransaction");

                    b.Navigation("UserAccount");

                    b.Navigation("UserDeposit");

                    b.Navigation("UserImageDetail")
                        .IsRequired();

                    b.Navigation("UserResetPassword");

                    b.Navigation("UserResetPin");

                    b.Navigation("UserSecurityQuestion");

                    b.Navigation("UserTransaction");

                    b.Navigation("UserWalletCharge");

                    b.Navigation("Userpin");
                });
#pragma warning restore 612, 618
        }
    }
}
