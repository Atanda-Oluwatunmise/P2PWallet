using Microsoft.EntityFrameworkCore;
using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<UserViewModel> AddNewUser(UserDto user)
        {
            //Initializing a collection
            List<UserViewModel> users = new List<UserViewModel>();

            //instantiating the User constructor
            var newuser = new User()
            {
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Password = user.Password
            };

            //Adding the new instance
            await _dataContext.Users.AddAsync(newuser);
            await _dataContext.SaveChangesAsync();

            return null;


        }
    }
}
