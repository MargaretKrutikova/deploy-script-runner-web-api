using System.Collections.Generic;

namespace DeploymentSettings.Models
{
	public class GlobalDeploymentSettings
	{
		public GlobalDeploymentSettings(
			string remoteUrl, 
			string localPath, 
			Dictionary<string, string[]> groupDeployablePaths, 
			Dictionary<string, string[]> serviceDeployablePaths)
		{
			GlobalSettingsRepo = new SettingsRepo(remoteUrl, localPath);
			GroupDeployablePaths = groupDeployablePaths;
			ServiceDeployablePaths = serviceDeployablePaths;
		}

		public SettingsRepo GlobalSettingsRepo { get; }
		
		public  Dictionary<string, string[]> GroupDeployablePaths { get; }

		public Dictionary<string, string[]> ServiceDeployablePaths { get; }
	}

	public class SettingsRepo
	{
		public string RemoteUrl { get; }

		public string LocalPath { get; }

		public SettingsRepo(string remoteUrl, string localPath)
		{
			RemoteUrl = remoteUrl;
			LocalPath = localPath;
		}
	}
}
