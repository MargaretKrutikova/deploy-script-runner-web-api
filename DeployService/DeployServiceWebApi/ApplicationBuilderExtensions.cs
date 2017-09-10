using System;
using System.IO;
using System.Text;
using DeploymentSettings;
using DeploymentSettings.Json;
using DeployServiceWebApi.Exceptions;
using DeployServiceWebApi.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DeployServiceWebApi
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseDeploymentSettingsDataInitializer(this IApplicationBuilder app)
        {
            var dataStoreService = app.ApplicationServices.GetRequiredService<IDeploymentSettingsDataStore>();

            var configOptions = app.ApplicationServices.GetRequiredService<IOptions<ConfigurationOptions>>();
            var settingsPath = configOptions.Value.DeploySettingsPath;

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore // ignore null values
            };

            var settingsString = File.ReadAllText(settingsPath);
            var settingsJson = JsonConvert.DeserializeObject<GlobalDeploymentSettingsJson>(settingsString, jsonSerializerSettings);

            dataStoreService.InitializeData(settingsJson);
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

        public static IApplicationBuilder UseJwtBearerAuthenticationWithCustomJwtValidation(
            this IApplicationBuilder app)
        {
            var jwtOptions = app.ApplicationServices.GetRequiredService<IOptions<JwtOptions>>();

            // secretKey contains a secret passphrase only your server knows
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtOptions.Value.SignatureKey));

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

            return app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = tokenValidationParameters
            });
        }
    }
}