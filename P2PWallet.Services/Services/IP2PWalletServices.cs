using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Services
{
    public interface IP2PWalletServices
    {
        public Task<ServiceResponse<UserViewModel>> Register(UserDto user);
        public Task<ServiceResponse<string>> Login(LoginDto loginreq);
        public Task<bool> UserAlreadyExists(string userName);
        public Task<bool> EmailAlreadyExists(string emailName);
        public bool VerifyPasswordHash(string password, byte[] passwordKey, byte[] passwordHash);
        //public string GetMyName();
        public string GetMyAccountNumber();


    }
}
