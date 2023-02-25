using P2PWallet.Models.Models.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Services
{
    public interface IP2PWalletServices
    {
        public Task<UserViewModel> AddNewUser(UserDto user);
    }
}
