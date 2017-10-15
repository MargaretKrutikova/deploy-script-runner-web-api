using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using DeploymentSettings.Json;
using DeploymentSettings.Models;

namespace DeploymentSettings
{
    public class DeploymentSettingsDataStore : IDeploymentSettingsDataStore
    {
        private GlobalDeploymentSettings _deploymentSettings;

        private readonly object _lockObject = new object();

        public void SetGlobalDeploymentSettings(GlobalDeploymentSettingsJson settingsJson)
        {
            lock (_lockObject) 
            {
                var groupSettingsDictionary = settingsJson.Groups.ToDictionary(group => group.Key, group =>
                    new GroupDeploymentSettings(
                        ConvertJsonDeploymentScripts(group.Value.Scripts), // group scripts

                        group.Value.Services.ToDictionary(service => service.Key, service => 
                            new ServiceDeploymentSettings(
                                service.Value.DisplayText, 
                                ConvertJsonDeploymentScripts(service.Value.Scripts, group.Value.ServiceScriptsRootPath ?? "") // service scripts
                            )),

                        group.Value.ServiceScriptsRootPath
                    ));

                _deploymentSettings = new GlobalDeploymentSettings(groupSettingsDictionary);
            }
        }

        private List<DeploymentScript> ConvertJsonDeploymentScripts(DeploymentScriptJson[] scripts, string rootPath = "") 
        {
            return scripts?.Select(script => new DeploymentScript(
                    Path.Combine(rootPath, script.Path), 
                    script.Arguments))
                .ToList();
        }

        public bool TryGetServiceSettings(
            string group,
            string service,
            out ServiceSettings settings)
        {
            lock (_lockObject) 
            {
                if (!_deploymentSettings.Groups.TryGetValue(group, out GroupDeploymentSettings groupSettings) ||
                    !groupSettings.Services.TryGetValue(service, out ServiceDeploymentSettings serviceSettings))
                {
                    settings = null;
                    return false;
                }

                var scripts = groupSettings.Scripts?.ToList() ?? new List<DeploymentScript>();
                scripts.AddRange(serviceSettings.Scripts);

                settings = new ServiceSettings(group, service, scripts);
                return true;
            }
        }

        public ReadOnlyDictionary<string, GroupDeploymentSettings> GetGroups()
        {
            lock (_lockObject) 
            {
                return _deploymentSettings.Groups;
            }
        }
    }
}
