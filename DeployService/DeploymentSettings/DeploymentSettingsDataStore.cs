using System;
using System.Collections.Generic;
using System.IO;
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

			// convert to full path if it specified as relative path in the settings.
			string fullRepoLocalPath = (new FileInfo(settingsJson.SettingsRepoLocalPath)).FullName;

			_deploymentSettings = new GlobalDeploymentSettings(
				settingsJson.SettingsRepoSvnUrl,
				fullRepoLocalPath,
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
