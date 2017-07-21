using System.IO;
using DeploymentSettings;
using DeploymentSettings.Json;
using DeployServiceWebApi.Exceptions;
using DeployServiceWebApi.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DeployServiceWebApi
{
    public static class ApplicationBuilderExtensions
    {
	    public static void UseDeploymentSettingsDataInitializer(this IApplicationBuilder app)
	    {
		    var dataStoreService = app.ApplicationServices.GetRequiredService<IDeploymentSettingsDataStore>();

		    var configOptions = app.ApplicationServices.GetRequiredService<IOptions<ConfigurationOptions>>();
		    var settingsPath = configOptions.Value.DeploySettingsPath;

		    var settingsJson = JsonConvert.DeserializeObject<GlobalDeploymentSettingsJson>(
			    File.ReadAllText(settingsPath));

		    dataStoreService.InitializeData(settingsJson);
	    }

	    public static IApplicationBuilder UseExceptionHandlingMiddleware(
		    this IApplicationBuilder builder)
	    {
		    return builder.UseMiddleware<ExceptionHandlingMiddleware>();
	    }
	}
}