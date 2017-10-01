using System.Collections.Generic;

namespace DeploymentSettings.Models
{
    public class ServiceSettings 
    {
        public string Project { get; }
        public string Service { get; }
        public IReadOnlyCollection<DeploymentScript> Scripts { get; }

        public ServiceSettings(string project, string service, List<DeploymentScript> scripts)
        {
            Project = project;
            Service = service;
            Scripts = scripts.AsReadOnly();
        }
    }
}