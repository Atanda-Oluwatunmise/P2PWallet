﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;

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
        public string AccountSearch { get; set; } = string.Empty;
        public double Amount { get; set; }
    }

    public class UserSearchDto
    {
        public string AccountSearch { get; set; } = string.Empty;
    }

    public class AccountViewModel
    {
        public string AccountNumber { get; set; } = string.Empty;
        public double Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
    }

    public class AccountDetails
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phonenumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Balance { get; set; }
    }

    public class SearchAccountDetails
    {
        public string AccountName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
    }

    

    public class TransactionsView
    {
        public string? SenderInfo { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public double TxnAmount { get; set; }
        public string TransType { get; set; } = string.Empty;
        public string ReceiverInfo { get; set; } = string.Empty;
        public DateTime DateofTransaction { get; set; }
    }

    public class LoginView
    {
        public string Name { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    public class PinDto
    {
        public string UserPin { get; set; } = string.Empty;
    }

    public class DepositDto
    {
        public double Amount { get; set; }
    }

    public class PaystackRequestDto
    {
        public string email { get; set; } = string.Empty;
        public string currency { get; set; } = string.Empty;
        public double amount { get; set; }
        public string reference { get; set; } = string.Empty;

    }

    public class PaystackRequestView
    {
        public Boolean Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public Data  data { get; set; }

    }
    public class Data
    {
        public string Authorization_url { get; set; } = string.Empty;
        public string Access_code { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;

    }

    public class ResetPasswordDto
    {
        public string Password { get; set; } = string.Empty;
    }

    public class ChangePasswordDto
    {
        //public string Question { get; set; }
        public string Answer { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.CurrentPassword).NotNull().NotEmpty().Length(1, 20).WithMessage("Please Enter your current password.");
            RuleFor(p => p.NewPassword).NotEmpty().WithMessage("Your password cannot be empty")
                    .MinimumLength(8).WithMessage("Your password length must be at least 8.")
                    .MaximumLength(16).WithMessage("Your password length must not exceed 16.")
                    .Matches(@"[A-Z]+").WithMessage("Your password must contain at least one uppercase letter.")
                    .Matches(@"[a-z]+").WithMessage("Your password must contain at least one lowercase letter.")
                    .Matches(@"[0-9]+").WithMessage("Your password must contain at least one number.")
                    .Matches(@"[\@\!\?\*\.]+").WithMessage("Your password must contain at least one (!? *.).");
            RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage("Please confirm your password.");

        }
    }
    public class CreatePinValidator : AbstractValidator<PinDto>
    {
        public CreatePinValidator()
        {
            RuleFor(x => x.UserPin).NotNull().NotEmpty().Length(4).WithMessage("Your pin length must exceed 4")
                    .Matches(@"[0-9]+").WithMessage("Your pin must contain only numbers.");
        }
    }

    public class ChangePinDto
    {
        public string CurrentPin { get; set; }
        //public string Question { get; set; }
        public string Answer { get; set; }
        public string NewPin { get; set; }
        public string ConfirmPin { get; set; }
    }

    public class EmailDto
    {
        public string Email { get; set; }
    }

    public class ChangePinValidator : AbstractValidator<ChangePinDto>
    {
        public ChangePinValidator()
        {
            RuleFor(x => x.CurrentPin).NotNull().NotEmpty().Length(4).WithMessage("Enter your current Pin");
            RuleFor(x => x.NewPin).NotNull().NotEmpty().Must(x => x.ToString().Length == 4).WithMessage("Must be 4 numeric digits");
            RuleFor(x => x.ConfirmPin).NotNull().NotEmpty().Must(x => x.ToString().Length == 4).WithMessage("Must be 4 numeric digits");
        }
    }

    public class SecurityQuestionDto
    {
        public string Question { get; set; }
        public string Answer { get; set; }
    }

    public class EditViewModel
    {
        public IFormFile ImageFile { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phonenumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class EditDetailValidator : AbstractValidator<EditViewModel>
    {
        public EditDetailValidator()
        {
            RuleFor(x => x.FirstName).NotNull().NotEmpty()
                    .Matches(@"[A-Z]+").WithMessage("Your first name must contain at least one uppercase letter.")
                    .Matches(@"[a-z]+").WithMessage("Your first name must contain at least one lowercase letter.");
            RuleFor(x => x.LastName).NotNull().NotEmpty()
                    .Matches(@"[A-Z]+").WithMessage("Your last name must contain at least one uppercase letter.")
                    .Matches(@"[a-z]+").WithMessage("Your last name must contain at least one lowercase letter.");            
            RuleFor(x => x.Phonenumber).NotNull().NotEmpty()
                    .Matches(@"[0-9]+").WithMessage("Your phonenumber must contain only numbers.");
            RuleFor(x => x.Address).NotNull().NotEmpty().WithMessage("Your address cannot be empty.");
        }
    }

    public class ResetPasswordRequest
    {
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Token { get; set; }
    }
    
    public class ResetPinRequest
    {
        public string Pin { get; set; }
        public string ConfirmPin { get; set; }
        public string Token { get; set; }
    }

    public class ResetPasswordValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordValidator(){
            RuleFor(p => p.Password).NotEmpty().WithMessage("Your password cannot be empty")
                    .MinimumLength(8).WithMessage("Your password length must be at least 8.")
                    .MaximumLength(16).WithMessage("Your password length must not exceed 16.")
                    .Matches(@"[A-Z]+").WithMessage("Your password must contain at least one uppercase letter.")
                    .Matches(@"[a-z]+").WithMessage("Your password must contain at least one lowercase letter.")
                    .Matches(@"[0-9]+").WithMessage("Your password must contain at least one number.")
                    .Matches(@"[\@\!\?\*\.]+").WithMessage("Your password must contain at least one (!? *.).");
            RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage("Please confirm your password.");
            RuleFor(x => x.Token).NotNull().NotEmpty();

        }
    }

     public class ResetPinValidator : AbstractValidator<ResetPinRequest>
        {
        public ResetPinValidator(){
            RuleFor(p => p.Pin).NotEmpty().WithMessage("Your pin cannot be empty")
                    .MaximumLength(4).WithMessage("Your pin length must not exceed 4.")
                    .Matches(@"[0-9]+").WithMessage("Your password must contain at least one number.");
            RuleFor(x => x.ConfirmPin).NotEmpty().WithMessage("Please confirm your pin.");
            RuleFor(x => x.Token).NotNull().NotEmpty();
        }
    }

    public class ImageViewmodel
    {
        public List<IFormFile> ImagePath { get; set; }
    }  
    
    public class DisplayViewmodel
    {
        public byte[] ImagePath { get; set; }
    }

    public class SecurityViewModel
    {
        public string Question { get; set; } = string.Empty;
    }

}
