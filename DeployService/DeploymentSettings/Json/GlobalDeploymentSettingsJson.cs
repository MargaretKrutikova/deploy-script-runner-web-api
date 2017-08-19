using System.Collections.Generic;
using Newtonsoft.Json;

namespace DeploymentSettings.Json
{
	public class GlobalDeploymentSettingsJson
	{
		public Dictionary<string, ProjectDeploymentSettingsJson> Projects { get; set; }
	}

	public class ProjectDeploymentSettingsJson
	{
		public string ServiceScriptsRootPath { get; set; }
		public DeploymentScriptJson[] Scripts { get; set; }
		public Dictionary<string, ServiceDeploymentSettingsJson> Services { get; set; }
	}

	public class ServiceDeploymentSettingsJson
	{
		public string DisplayText { get; set; }
		public DeploymentScriptJson[] Scripts { get; set; }
	}

	public class DeploymentScriptJson
	{
		public string Path { get; set; }
		public string Arguments { get; set; }
	}
}
