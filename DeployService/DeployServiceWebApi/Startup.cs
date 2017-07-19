using System;
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
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
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

	        services.TryAddScoped<IDeploymentService, DeploymentService>();
			services.TryAddSingleton<IDeploymentSettingsDataStore, DeploymentSettingsDataStore>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
	    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
	    {
		    loggerFactory.AddConsole(Configuration.GetSection("Logging"));
		    loggerFactory.AddDebug();

		    try
		    {
			    if (env.IsDevelopment())
			    {
				    app.UseDeveloperExceptionPage();
			    }
			    else
			    {
				    app.UseExceptionHandlingMiddleware();
			    }

			    app.UseDeploymentSettingsDataInitializer();
			    app.UseMvc();
		    }
		    catch (Exception exception)
		    {
			    // TODO: log exception

			    if (env.IsDevelopment()) throw;

			    // app.Run terminates the pipeline.
			    app.Run(context => throw new StartupException("An error occurred while starting the application.",
				    exception));
		    }
	    }
    }
}
