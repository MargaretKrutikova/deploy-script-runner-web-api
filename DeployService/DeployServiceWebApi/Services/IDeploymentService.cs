using DeploymentJobs.DataAccess;
using DeploymentSettings.Models;

namespace DeployServiceWebApi.Services
{
    public interface IDeploymentService
    {
        bool TryAddJobToQueue(ServiceSettings settings, out DeploymentJob job);
    }
}