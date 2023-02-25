using Microsoft.EntityFrameworkCore;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Services
{
    public class P2PWalletServices: IP2PWalletServices
    {
        private readonly DataContext _dataContext;

        public P2PWalletServices(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<bool> EmailAlreadyExists(string emailName)
        {
            return await _dataContext.Users.AnyAsync(x => x.Email == emailName);
        }

        public async Task<UserViewModel> Register(UserDto user)
        {
            //Initializing a collection
            List<UserViewModel> users = new List<UserViewModel>();

            //Adding two variables of type byte
            byte[] passwordHash, passwordKey;

            using (var hmac = new HMACSHA512())
            {
                passwordKey = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(user.Password));
            }

            //instantiating the User constructor
            var newuser = new User()
            {
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Password = passwordHash,
                PasswordKey = passwordKey 
            };

            //Adding the new instance
            await _dataContext.Users.AddAsync(newuser);
            await _dataContext.SaveChangesAsync();
            return null;
        }

        public async Task<bool> UserAlreadyExists(string userName)
        {
            return await _dataContext.Users.AnyAsync(x => x.Username == userName);
        }
    }
}
