using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ServerLibrary.Repositories.Implementation;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Application.Extension;
using EmployeeManagementSystem.Infrastructure.Data;
using EmployeeManagementSystem.Application.Contracts;
using EmployeeManagementSystem.Application.Validators;
using FluentValidation.AspNetCore;
using FluentValidation;

namespace EmployeeManagementSystem.Infrastructure.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddInfrastructureService(this IServiceCollection services, IConfiguration configuration)
        {

            //Map keys to the configuration JwtSection
            services.Configure<JwtSection>(configuration.GetSection("JwtSection"));
            var jwtSection = configuration.GetSection(nameof(JwtSection)).Get<JwtSection>();

            //Add our dbcontext.
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("EmployeeManagementSystemConnectionString") ??
                    throw new InvalidOperationException("Sorry, Your connection is not found"));
            });

            //Add Interfaces and Implementations
            services.AddScoped<IUserAccountRepository, UserAccountRepository>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtSection!.Issuer,
                    ValidAudience = jwtSection.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection.Key!))
                };

            });

            //Add cors
            services.AddCors(options =>
            {
                options.AddPolicy("AllowEmployeeManagementSystemApp",
                    builder => builder
                    .WithOrigins("http://localhost:4200")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            //Fluent Validation
            services.AddFluentValidationAutoValidation();

            services.AddValidatorsFromAssemblyContaining<AccountBaseValidator>();
            services.AddValidatorsFromAssemblyContaining<RegisterValidator>();

            return services;
        }
    }
}
