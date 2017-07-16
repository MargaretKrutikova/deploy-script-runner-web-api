using System.Text;
using DeploymentSettings;
using DeployServiceWebApi.Options;
using DeployServiceWebApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

			services.AddScoped<IConfigurationService, ConfigurationOptionsService>();
	        services.AddScoped<IDeploymentService, DeploymentService>();

			services.AddSingleton<IDeploymentSettingsDataStore, DeploymentSettingsDataStore>();
		}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

	        if (!env.IsDevelopment())
	        {
		        app.UseDeveloperExceptionPage();
	        }
	        else
	        {
				// TODO: Implement exception handling middleware.
				app.UseExceptionHandler(errorApp =>
				{
					errorApp.Run(async context =>
					{
						context.Response.StatusCode = 500; // or another Status accordingly to Exception Type
						context.Response.ContentType = "application/json";

						var error = context.Features.Get<IExceptionHandlerFeature>();
						if (error != null)
						{
							var ex = error.Error;

							await context.Response.WriteAsync(new 
							{
								Code = context.Response.StatusCode,
								ex.Message
							}.ToString(), Encoding.UTF8);
						}
					});
				});
			}
            app.UseMvc();
        }
    }
}
