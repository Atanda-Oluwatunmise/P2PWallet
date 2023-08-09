using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using P2PWallet.Services.Interface;
using P2PWallet.Services.Services;

namespace P2PWallet.Services.Services.Seeding
{
    public class SeedingService
    {
        private readonly IUserServices _userServices;
        public SeedingService(IUserServices userServices)
        {
            _userServices = userServices;
        }

        public static void DataSeeding(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            SeedData(serviceScope.ServiceProvider.GetService<DataContext>());
        } 
        public static void AdminSeeding(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            SeedAdmin(serviceScope.ServiceProvider.GetService<DataContext>());
        }
        private static void SeedData(DataContext dataContext)
        {
            SecurityCurrencyService.SeedCurrencies(dataContext);
        } 
        private static void SeedAdmin(DataContext dataContext)
        {
            SeedAdminService.SeedSuperAdmin(dataContext);
        }
    }
}
