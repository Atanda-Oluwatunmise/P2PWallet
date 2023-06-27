﻿using P2PWallet.Models.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Interface
{
    public interface IMultipleWallets
    {
        public Task<ServiceResponse<String>> CreateNewAccountWallet(string currency);

    }
}
