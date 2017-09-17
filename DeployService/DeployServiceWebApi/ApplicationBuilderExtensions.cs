using System;
using System.IO;
using System.Text;
using DeploymentSettings;
using DeploymentSettings.Json;
using DeployServiceWebApi.Exceptions;
using DeployServiceWebApi.Options;
using DeployServiceWebApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DeployServiceWebApi
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseDeploymentSettingsDataInitializer(this IApplicationBuilder app)
        {
            var dataStoreService = app.ApplicationServices.GetRequiredService<IDeploymentSettingsDataService>();
            dataStoreService.ReloadDeploymentSettingsFromFile();
        }

        public static IApplicationBuilder UseExceptionHandlingMiddleware(
            this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }

        public static IApplicationBuilder UseCorsFromOptions(this IApplicationBuilder app)
        {
            var corsOptions = app.ApplicationServices.GetRequiredService<IOptions<ConfigurationOptions>>().Value;
            
            return app.UseCors(builder => builder
                .WithOriginsOrAny(corsOptions.CorsOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod());
        }

        public static CorsPolicyBuilder WithOriginsOrAny(
            this CorsPolicyBuilder builder, 
            string[] origins) 
        {
            return origins.Length == 0 ? 
                builder.AllowAnyOrigin() : 
                builder.WithOrigins(origins);
        }

        public static void AddJwtBearerAuthentication(
            this IServiceCollection services, 
            IConfigurationRoot configuration)
        {
            var jwtOptions = new JwtOptions();
            configuration.Bind("JwtOptions", jwtOptions);

            // secretKey contains a secret passphrase only your server knows
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtOptions.SignatureKey));

            // information that is used to validate the token
            var tokenValidationParameters = new TokenValidationParameters
            {
                // The signing key must match!
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                ValidateIssuer = false,

                ValidateAudience = false,

                // Validate the token expiry
                ValidateLifetime = true,

                // If you want to allow a certain amount of clock drift, set that here:
                ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.TokenValidationParameters = tokenValidationParameters;
                });
        }
    }
}