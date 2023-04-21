//using Microsoft.AspNetCore.Builder;
//using Microsoft.Extensions.DependencyInjection;

//namespace P2PWallet.Services.Services.Seeding
//{
//    public static class SeedingService
//    {
//        public static void DataSeeding(IApplicationBuilder app)
//        {
//            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
//            SeedData(serviceScope.ServiceProvider.GetService<DataContext>());
//        }
//        private static void SeedData(DataContext dataContext)
//        {
//            SecurityQuestionsService.SeedSecurityQuestions(dataContext);
//        }
//    }
//}
