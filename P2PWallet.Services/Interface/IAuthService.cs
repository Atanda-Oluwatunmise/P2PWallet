using P2PWallet.Models.Models.DataObjects;
using P2PWallet.Models.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Interface
{
    public interface IAuthService
    {
        public Task<ServiceResponse<string>> CreatePin(PinDto pin);
        public Task<ServiceResponse<string>> VerifyPin(PinDto pin);

    }
}
