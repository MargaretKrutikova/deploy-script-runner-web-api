namespace DeploymentSettings
{
    public interface IConfigurationService
    {
	    string GetDeploySettingsFilePath();

	    string GetRepoUpdateScriptPath();
    }
}