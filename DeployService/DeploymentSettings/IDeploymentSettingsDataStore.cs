using System.Collections.Generic;
using System.Collections.ObjectModel;
using DeploymentSettings.Json;
using DeploymentSettings.Models;

namespace DeploymentSettings
{
    public interface IDeploymentSettingsDataStore
    {
        ReadOnlyDictionary<string, GroupDeploymentSettings> GetGroups();
        bool TryGetServiceSettings(string group, string service, out ServiceSettings settings);
        void SetGlobalDeploymentSettings(GlobalDeploymentSettingsJson settingsJson);
    }
}
