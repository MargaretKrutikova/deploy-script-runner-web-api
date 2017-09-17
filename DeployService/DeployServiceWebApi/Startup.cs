using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using DeploymentJobs.DataAccess;
using DeploymentSettings;
using DeployServiceWebApi.Exceptions;
using DeployServiceWebApi.Options;
using DeployServiceWebApi.Services;
using DeployService.Common.Exceptions;
using DeployService.Common.Options;
using Serilog;

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
            services.AddMvc();
            // TODO: this didnt work on the server, investigate why.
            /*(opt =>
            {
                if (!_env.IsProduction())
                {
                    opt.SslPort = 44388;
                }
                opt.Filters.Add(new RequireHttpsAttribute());
            });*/

            services.AddJwtBearerAuthentication(Configuration);

            services.Configure<ConfigurationOptions>(Configuration);
            services.Configure<DeploymentJobsCleanerOptions>(Configuration);
            services.Configure<JwtOptions>(options => Configuration.GetSection("JwtOptions").Bind(options));
            services.Configure<CustomAuthorizationOptions>(options => Configuration.GetSection("AuthorizationOptions").Bind(options));

            services.TryAddScoped<IDeploymentService, DeploymentService>();
            services.TryAddSingleton<IUserService, UserService>();
            services.TryAddSingleton<IDeploymentSettingsDataService, DeploymentSettingsDataService>();
            services.TryAddSingleton<IDeploymentSettingsDataStore, DeploymentSettingsDataStore>();
            services.TryAddSingleton<IDeploymentJobDataAccess, DeploymentJobDataAccess>();
            services.TryAddSingleton<IDeploymentJobsCleaner, DeploymentJobsCleaner>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            ILogger<Startup> logger,
            IDeploymentJobsCleaner jobsCleaner)
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

                app.UseAuthentication();
                app.UseMvc();

                jobsCleaner.Start();
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
