using System;
using System.Collections.Generic;
using DeploymentSettings.Json;
using DeploymentSettings.Models;

namespace DeploymentSettings
{
	public class DeploymentSettingsDataStore : IDeploymentSettingsDataStore
	{
	    private GlobalDeploymentSettings _deploymentSettings;

		public void InitializeData(GlobalDeploymentSettingsJson settingsJson)
		{
			if (_deploymentSettings != null)
			{
				throw new InvalidOperationException("Data has already been initialized");
			}

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

		public IEnumerable<string> GetGroups()
		{
			return _deploymentSettings.GroupDeployablePaths.Keys;
		}
	}
}
