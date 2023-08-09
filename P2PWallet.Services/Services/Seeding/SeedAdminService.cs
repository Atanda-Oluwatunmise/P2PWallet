using Aspose.Cells;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Octokit;
using P2PWallet.Models.Models.Entities;
using P2PWallet.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Services.Seeding
{
    public class SeedAdminService
    {
        private readonly DataContext _dataContext;
        private readonly IUserServices _userServices;
        private readonly IConfiguration _configuration;

        public SeedAdminService(DataContext dataContext, IUserServices userServices, IConfiguration configuration)
        {
            _dataContext = dataContext;
            _userServices = userServices;
            _configuration = configuration;

        }
     
        public static void SeedSuperAdmin(DataContext dataContext)
        {
            var password = "PassWord10$#";
            byte[] passwordKey = null;
            byte[] passwordHash = null;
            using (var hmac = new HMACSHA512())
            {
                passwordKey = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }

            if (dataContext.SuperAdmins.Any())
            {
                return; //DB has been seeded with module data 
            }
            var admin = new SuperAdmin[]
            {
                new SuperAdmin(){ Name = "Admin", Password = passwordHash, PasswordKey = passwordKey}
            };
            dataContext.ChangeTracker.Entries().Where(e => e.State == EntityState.Added);
            dataContext.SuperAdmins.AddRange(admin);
            dataContext.Database.OpenConnection();
            try
            {
                dataContext.SaveChanges();
            }
            finally
            {
                dataContext.Database.CloseConnection();
            }
        }
    }
}
