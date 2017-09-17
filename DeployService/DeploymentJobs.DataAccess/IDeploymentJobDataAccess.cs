using System.Collections.Generic;
using System.Diagnostics;

namespace DeploymentJobs.DataAccess
{
    public interface IDeploymentJobDataAccess
    {
        IEnumerable<DeploymentJob> GetCurrentJobs();
        void CancelJob(string jobId);
        bool TryCreateIfVacant(string project, string service, out DeploymentJob newJob);
        bool TryGetJob(string jobId, out DeploymentJob job);
        bool CheckJobStatus(string jobId, DeploymentJobStatus statusToCheck);
        DeploymentJob GetJob(string jobId);
        DeploymentJob DeleteJob(string jobId);
        void DeleteAllFinished();
        void SetSuccess(string jobId);
        void SetInProgress(string jobId, string action, Process process = null);
        void SetFail(string jobId, string errorMessage);
    }
}
