using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using RePlay.Application.Interfaces;
using RePlay.Infrastructure.Data;
using RePlay.Infrastructure.Repositories;
using RePlay.Infrastructure.Services;

namespace RePlay.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Database Seeder
        services.AddScoped<DbSeeder>();

        // Configure JWT Settings
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        // Configure Email Settings
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

        // Configure File Upload Settings
        services.Configure<FileUploadSettings>(configuration.GetSection("FileUpload"));

        // Configure Stripe Settings
        services.Configure<StripeSettings>(configuration.GetSection("StripeSettings"));

        // Register Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IToyService, ToyService>();
        services.AddScoped<IFileUploadService, FileUploadService>();
        services.AddScoped<ITradeService, TradeService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IReturnService, ReturnService>();

        // Memory Cache for tokens
        services.AddMemoryCache();

        // JWT Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }
}
