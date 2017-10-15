using System;
using System.IO;
using System.Linq;
using DeploymentJobs.DataAccess.Queues;
using DeploymentSettings;
using DeploymentSettings.Json;
using DeployService.Common.Exceptions;
using DeployServiceWebApi.Models;
using DeployServiceWebApi.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DeployServiceWebApi.Services
{
    public class DeploymentSettingsDataService : IDeploymentSettingsDataService
    {
        private readonly ILogger<DeploymentSettingsDataService> _logger;
        private readonly IDeploymentSettingsDataStore _deploymentSettingsDataStore;
        private readonly IDeploymentJobQueues _jobQueues;
        private readonly string _deploySettingsPath;

        public DeploymentSettingsDataService(
            ILogger<DeploymentSettingsDataService> logger,
            IOptions<ConfigurationOptions> configOptions,
            IDeploymentJobQueues jobQueues,
            IDeploymentSettingsDataStore deploymentSettingsDataStore)
        {
            _logger = logger;
            _deploySettingsPath = configOptions.Value.DeploySettingsPath;
            _deploymentSettingsDataStore = deploymentSettingsDataStore;
            _jobQueues = jobQueues;
        }

        public void ReloadDeploymentSettingsAndJobQueues()
        {
            try 
            {
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore // ignore null values
                };

                var settingsString = File.ReadAllText(_deploySettingsPath);
                var settingsJson = JsonConvert.DeserializeObject<GlobalDeploymentSettingsJson>(settingsString, jsonSerializerSettings);

                _deploymentSettingsDataStore.SetGlobalDeploymentSettings(settingsJson);

                _jobQueues.InitializeAndRunJobQueues(settingsJson.Groups.Keys);
            }
            catch (Exception exception) 
            {
                var errorMessage = "Failed to reload deployment settings from file.";
                
                _logger.LogError(errorMessage, exception);
                throw new DeployServiceGenericException(errorMessage);
            }
        }

        public GroupModel[] GetGroupsModel()
        {
            var groups = _deploymentSettingsDataStore.GetGroups().Select(p =>
                new GroupModel
                {
                    Name = p.Key,
                    Services = p.Value.Services.Select(s => new ServiceModel
                    {
                        Name = s.Key,
                        Description = s.Value.DisplayText
                    }).ToArray()
                }).ToArray();

            return groups;
        }
    }
}
