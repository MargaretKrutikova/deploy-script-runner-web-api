using DeployServiceWebApi.Models;

namespace DeployServiceWebApi.Services
{
    public interface IDeploymentSettingsDataService
    {
       void ReloadDeploymentSettingsAndJobQueues();
       ProjectModel[] GetProjectsModel();
    }
}