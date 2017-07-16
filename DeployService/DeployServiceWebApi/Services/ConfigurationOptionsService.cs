using DeploymentSettings;
using DeployServiceWebApi.Options;
using Microsoft.Extensions.Options;

namespace DeployServiceWebApi.Services
{
	public class ConfigurationOptionsService : IConfigurationService
	{
	    private readonly ConfigurationOptions _configurationOptions;

	    public ConfigurationOptionsService(IOptions<ConfigurationOptions> optionsAccessor)
	    {
		    _configurationOptions = optionsAccessor.Value;
	    }

	    public string GetDeploySettingsFilePath()
	    {
		    return _configurationOptions.DeploySettingsPath;
	    }

		public string GetRepoUpdateScriptPath()
		{
			return _configurationOptions.RepoUpdateScriptPath;
		}
	}
}
