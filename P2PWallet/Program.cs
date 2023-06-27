global using P2PWallet;
global using P2PWallet.Services;
using Microsoft.EntityFrameworkCore;
using P2PWallet.Services.Services;
using P2PWallet.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.AspNetCore.Authentication;
using P2PWallet.Services.Interface;
using NLog;
using Microsoft.Extensions.Configuration;
using MailKit;
using IMailService = P2PWallet.Services.Interface.IMailService;
using MailService = P2PWallet.Services.Services.MailService;
using P2PWallet.Models.Models.Entities;
using P2PWallet.Models.Models.DataObjects;
using Microsoft.AspNetCore.Identity;
using FluentValidation.AspNetCore;
using System.Reflection;
using FluentValidation;
using DinkToPdf;
using DinkToPdf.Contracts;
using NLog;
using NLog.Web;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using P2PWallet.Services.Services.Seeding;
//using FluentValidation.AspNetCore;
//using System.Reflection;
var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

try
{

    var builder = WebApplication.CreateBuilder(args);
    //LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));

    // Add services to the container.

    builder.Services.AddControllers()
                    .AddFluentValidation(options =>
                    {
                        // Validate child properties and root collection elements
                        options.ImplicitlyValidateChildProperties = true;
                        options.ImplicitlyValidateRootCollectionElements = true;
                        // Automatic registration of validators in assembly
                        options.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
                    });
    builder.Services.AddTransient<IValidator<ChangePasswordDto>, ChangePasswordValidator>();
    builder.Services.AddTransient<IValidator<ChangePinDto>, ChangePinValidator>();
    builder.Services.AddTransient<IValidator<ResetPasswordRequest>, ResetPasswordValidator>();
    builder.Services.AddTransient<IValidator<ResetPinRequest>, ResetPinValidator>();
    builder.Services.AddTransient<IValidator<PinDto>, CreatePinValidator>();
    builder.Services.AddTransient<IValidator<EditViewModel>, EditDetailValidator>();
    builder.Services.AddTransient<IValidator<SecurityQuestionDto>, SecurityQuestionValidator>();

    builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("oauth2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey
        });

        options.OperationFilter<SecurityRequirementsOperationFilter>();
    });
    builder.Services.AddScoped<IUserServices, UserServices>();
    builder.Services.AddScoped<ITransactionService, TransactionService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IPaymentService, PaymentService>();
    builder.Services.AddScoped<ILoggerManager, LoggerManager>();
    builder.Services.AddScoped<IMailService, MailService>();
    builder.Services.AddScoped<IMultipleWallets, MultipleWallets>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


    builder.Services.Configure<DataProtectionTokenProviderOptions>(opts => opts.TokenLifespan = TimeSpan.FromHours(10));
    var emailConfig = builder.Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
    builder.Services.AddSingleton(emailConfig);



    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                    .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    SeedingService.DataSeeding(app);
    app.UseHttpsRedirection();

    app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}

catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}
