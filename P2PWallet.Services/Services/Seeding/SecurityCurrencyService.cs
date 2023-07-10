using Microsoft.EntityFrameworkCore;
using P2PWallet.Models.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Services.Seeding
{
    public class SecurityCurrencyService
    {
       private readonly DataContext _dataContext;

        public SecurityCurrencyService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }


        public static void SeedCurrencies(DataContext dataContext)
        {
            if (dataContext.CurrenciesWallets.Any())
            {
                return; //DB has been seeded with module data 
            }
            var currencyList = new CurrenciesWallet[]
            {
                new CurrenciesWallet(){Currencies = "NGN", ChargeAmount = 0},
                new CurrenciesWallet(){Currencies = "USD", ChargeAmount = 765},
                new CurrenciesWallet(){Currencies = "AED", ChargeAmount = 210},
                new CurrenciesWallet(){Currencies = "CAD", ChargeAmount = 580},
                new CurrenciesWallet(){Currencies = "EUR", ChargeAmount = 837},
                new CurrenciesWallet(){Currencies = "GBP", ChargeAmount = 973}
            };
            dataContext.ChangeTracker.Entries().Where(e => e.State == EntityState.Added);
            dataContext.CurrenciesWallets.AddRange(currencyList);
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
