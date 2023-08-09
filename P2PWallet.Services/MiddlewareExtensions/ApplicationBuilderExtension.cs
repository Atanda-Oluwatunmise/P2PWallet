using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using P2PWallet.Services.SubscribeTableDependencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.MiddlewareExtensions
{
    public static class ApplicationBuilderExtension
    {
        public static void UserTransactionTableDependency(IApplicationBuilder app)
        {
            var serviceProvider = app.ApplicationServices;
            var service = serviceProvider.GetRequiredService<SubscribeTransactionsTable>();
            service.SubscribeTableDependency();
        }
    }
}
