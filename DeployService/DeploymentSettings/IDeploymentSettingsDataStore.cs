using System.Collections.Generic;
using System.Collections.ObjectModel;
using DeploymentSettings.Json;
using DeploymentSettings.Models;

namespace DeploymentSettings
{
	public interface IDeploymentSettingsDataStore
	{
		ReadOnlyDictionary<string, ProjectDeploymentSettings> GetProjects();

		bool TryGetDeployScripts(
			string project,
			string service,
			out List<DeploymentScript> scripts);

		void InitializeData(GlobalDeploymentSettingsJson settingsJson);
	}
}
