using System.Collections.Generic;

namespace DeploymentSettings.Models
{
    public class ServiceSettings 
    {
        public string Group { get; }
        public string Service { get; }
        public IReadOnlyCollection<DeploymentScript> Scripts { get; }

        public ServiceSettings(string group, string service, List<DeploymentScript> scripts)
        {
            Group = group;
            Service = service;
            Scripts = scripts.AsReadOnly();
        }
    }
}