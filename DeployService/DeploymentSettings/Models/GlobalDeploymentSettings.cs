using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DeploymentSettings.Models
{
    public class GlobalDeploymentSettings
    {
        public GlobalDeploymentSettings(Dictionary<string, GroupDeploymentSettings> groups)
        {
            Groups = new ReadOnlyDictionary<string, GroupDeploymentSettings>(groups);
        }
        public ReadOnlyDictionary<string, GroupDeploymentSettings> Groups { get; }
    }

    public class GroupDeploymentSettings
    {
        public GroupDeploymentSettings(
            List<DeploymentScript> scripts,
            Dictionary<string, ServiceDeploymentSettings> services,
            string serviceScriptsRootPath = "")
        {
            Scripts = scripts?.AsReadOnly();
            Services = new ReadOnlyDictionary<string, ServiceDeploymentSettings>(services);
            ServiceScriptsRootPath = serviceScriptsRootPath;
        }
        public IReadOnlyCollection<DeploymentScript> Scripts { get; }
        public ReadOnlyDictionary<string, ServiceDeploymentSettings> Services { get; }

        public string ServiceScriptsRootPath { get; }
    }

    public class ServiceDeploymentSettings
    {
        public ServiceDeploymentSettings(
            string text,
            List<DeploymentScript> scripts)
        {
            DisplayText = text;
            Scripts = scripts.AsReadOnly();
        }
        public string DisplayText { get; }
        public IReadOnlyCollection<DeploymentScript> Scripts { get; }
    }

    public class DeploymentScript
    {
        public DeploymentScript(string path, string args)
        {
            Path = path;
            Arguments = args;
        }
        public string Path { get; }
        public string Arguments { get; }
    }
}
