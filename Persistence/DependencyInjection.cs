using Application.Abstractions;
using Application.Abstractions.Idempotency;
using Application.Data;
using Application.Orders.Create;
using Domain.Customers;
using Domain.Orders;
using Domain.Products;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Persistence.Idempotency;
using Persistence.Repositories;
using Persistence.Services;
using System.Text;
using Application.Common.Interfaces.Authentication;
using Infrastructure.Authentication;
using JwtTokenGenerator = Infrastructure.Authentication.JwtTokenGenerator;
using Application.Common.Interfaces.Services;
using Infrastructure.Services;

namespace Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        
        services.AddAuth(configuration).AddDbContext<ApplicationDbContext>(options =>
            options
                .UseNpgsql(configuration.GetConnectionString("Database"))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());    

        services.AddScoped<ICustomerRepository, CustomerRepository>();

        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddScoped<IOrderSummaryRepository, OrderSummariesRepository>();

        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddScoped<ICalculateOrderSummary, CalculateOrderSummary>();

        services.AddScoped<IIdempotencyService, IdempotencyService>();


        return services;
    }
    public static IServiceCollection AddAuth(
        this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = new JwtSettings();
        configuration.Bind(JwtSettings.SectionName, jwtSettings);

        services.AddSingleton(Options.Create(jwtSettings));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Secret))
            });
        return services;
    }
}
