using System;
using DeploymentJobs.DataAccess;
using DeploymentSettings;
using DeployServiceWebApi.Exceptions;
using DeployServiceWebApi.Options;
using DeployServiceWebApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;
using DeployService.Common.Exceptions;

namespace DeployServiceWebApi
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            _env = env;
            Configuration = builder.Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            // Add framework services.
            services.AddMvc(opt =>
            {
                if (!_env.IsProduction())
                {
                    opt.SslPort = 44388;
                }
                opt.Filters.Add(new RequireHttpsAttribute());
            });

            services.Configure<ConfigurationOptions>(Configuration);
            services.Configure<JwtOptions>(options => Configuration.GetSection("JwtOptions").Bind(options));
            services.Configure<CustomAuthorizationOptions>(options => Configuration.GetSection("AuthorizationOptions").Bind(options));

            services.TryAddScoped<IDeploymentService, DeploymentService>();
            services.TryAddSingleton<IUserService, UserService>();
            services.TryAddSingleton<IDeploymentSettingsDataStore, DeploymentSettingsDataStore>();
            services.TryAddSingleton<IDeploymentJobDataAccess, DeploymentJobDataAccess>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            ILogger<Startup> logger)
        {
            loggerFactory.AddSerilog();

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            try
            {
                app.UseCorsFromOptions();

                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.UseExceptionHandlingMiddleware();

                app.UseDeploymentSettingsDataInitializer();

                app.UseJwtBearerAuthenticationWithCustomJwtValidation();
                app.UseMvc();
            }
            catch (Exception exception)
            {
                logger.LogError("Startup error occured.", exception);

                if (env.IsDevelopment()) throw;

                // app.Run terminates the pipeline.
                app.Run(context => throw new StartupException("An error occurred while starting the application.",
                    exception));
            }
        }
    }
}
