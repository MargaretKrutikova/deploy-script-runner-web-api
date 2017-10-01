using System.Collections.Generic;
using System.Collections.ObjectModel;
using DeploymentSettings.Json;
using DeploymentSettings.Models;

namespace DeploymentSettings
{
    public interface IDeploymentSettingsDataStore
    {
        ReadOnlyDictionary<string, ProjectDeploymentSettings> GetProjects();
        bool TryGetServiceSettings(string project, string service, out ServiceSettings settings);
        void SetGlobalDeploymentSettings(GlobalDeploymentSettingsJson settingsJson);
    }
}
