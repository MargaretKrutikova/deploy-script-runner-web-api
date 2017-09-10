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

        public void InitializeData(GlobalDeploymentSettingsJson settingsJson)
        {
            if (_deploymentSettings != null)
            {
                throw new InvalidOperationException("Data has already been initialized");
            }

            var projectSettingsDictionary = settingsJson.Projects.ToDictionary(project => project.Key, project =>
                new ProjectDeploymentSettings(
                    project.Value.Scripts?.Select(script => new DeploymentScript(script.Path, script.Arguments)).ToList(),
                    project.Value.Services.ToDictionary(service => service.Key, service => new ServiceDeploymentSettings(
                                        service.Value.DisplayText,
                                        service.Value.Scripts.Select(
                                            script => new DeploymentScript(
                                                Path.Combine(project.Value.ServiceScriptsRootPath, script.Path),
                                                script.Arguments))
                                                            .ToList())),
                    project.Value.ServiceScriptsRootPath
                ));

            _deploymentSettings = new GlobalDeploymentSettings(projectSettingsDictionary);
        }

        public bool TryGetDeployScripts(
            string project,
            string service,
            out List<DeploymentScript> scripts)
        {
            if (!_deploymentSettings.Projects.TryGetValue(project, out ProjectDeploymentSettings projectSettings) ||
                !projectSettings.Services.TryGetValue(service, out ServiceDeploymentSettings serviceSettings))
            {
                scripts = null;
                return false;
            }

            scripts = new List<DeploymentScript>();

            if (projectSettings.Scripts != null)
            {
                scripts.AddRange(projectSettings.Scripts);
            }
            scripts.AddRange(serviceSettings.Scripts);
            return true;
        }

        public ReadOnlyDictionary<string, ProjectDeploymentSettings> GetProjects()
        {
            return _deploymentSettings.Projects;
        }
    }
}
