using System.IO;
using DeploymentSettings.Json;
using DeploymentSettings.Models;
using Newtonsoft.Json;

namespace DeploymentSettings
{
	public interface IDeploymentSettingsDataStore
	{
		SettingsRepo GetRepoSettings();
		bool TryGetDeployablesByGroup(string group, out string[] deployables);
	}

	public class DeploymentSettingsDataStore : IDeploymentSettingsDataStore
	{
	    private readonly GlobalDeploymentSettings _deploymentSettings;

	    public DeploymentSettingsDataStore(IConfigurationService configurationService)
	    {
		    var settingsJson = JsonConvert.DeserializeObject<GlobalDeploymentSettingsJson>(
				File.ReadAllText(configurationService.GetDeploySettingsFilePath()));

		    _deploymentSettings = new GlobalDeploymentSettings(
				settingsJson.SettingsRepoSvnUrl,
			    settingsJson.SettingsRepoLocalPath,
			    settingsJson.GroupDeployablePaths,
			    settingsJson.ServiceDeployablePaths);
	    }

	    public SettingsRepo GetRepoSettings()
	    {
		    return _deploymentSettings.GlobalSettingsRepo;
	    }

		public bool TryGetDeployablesByGroup(string group, out string[] deployables)
		{
			return _deploymentSettings.GroupDeployablePaths.TryGetValue(group, out deployables);
		}
	}
}
