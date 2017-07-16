using System.Collections.Generic;

namespace DeploymentSettings.Json
{
	public class GlobalDeploymentSettingsJson
	{
		public string SettingsRepoSvnUrl { get; set; }

		public string SettingsRepoLocalPath { get; set; }
			
		public Dictionary<string, string[]> GroupDeployablePaths { get; set; }

		public Dictionary<string, string[]> ServiceDeployablePaths { get; set; }
	}
}
