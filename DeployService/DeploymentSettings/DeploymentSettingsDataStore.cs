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
                var projectSettingsDictionary = settingsJson.Projects.ToDictionary(project => project.Key, project =>
                    new ProjectDeploymentSettings(
                        ConvertJsonDeploymentScripts(project.Value.Scripts), // project scripts

                        project.Value.Services.ToDictionary(service => service.Key, service => 
                            new ServiceDeploymentSettings(
                                service.Value.DisplayText, 
                                ConvertJsonDeploymentScripts(service.Value.Scripts, project.Value.ServiceScriptsRootPath ?? "") // service scripts
                            )),

                        project.Value.ServiceScriptsRootPath
                    ));

                _deploymentSettings = new GlobalDeploymentSettings(projectSettingsDictionary);
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
            string project,
            string service,
            out ServiceSettings settings)
        {
            lock (_lockObject) 
            {
                if (!_deploymentSettings.Projects.TryGetValue(project, out ProjectDeploymentSettings projectSettings) ||
                    !projectSettings.Services.TryGetValue(service, out ServiceDeploymentSettings serviceSettings))
                {
                    settings = null;
                    return false;
                }

                var scripts = projectSettings.Scripts?.ToList() ?? new List<DeploymentScript>();
                scripts.AddRange(serviceSettings.Scripts);

                settings = new ServiceSettings(project, service, scripts);
                return true;
            }
        }

        public ReadOnlyDictionary<string, ProjectDeploymentSettings> GetProjects()
        {
            lock (_lockObject) 
            {
                return _deploymentSettings.Projects;
            }
        }
    }
}
