using System.Collections.Generic;
using DeploymentSettings.Json;
using DeploymentSettings.Models;

namespace DeploymentSettings
{
	public interface IDeploymentSettingsDataStore
	{
		SettingsRepo GetRepoSettings();

		IEnumerable<string> GetGroups();

		bool TryGetDeployablesByGroup(string group, out string[] deployables);

		void InitializeData(GlobalDeploymentSettingsJson settingsJson);
	}
}
