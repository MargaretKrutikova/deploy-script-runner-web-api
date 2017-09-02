using System.Collections.Generic;
using System.Diagnostics;

namespace DeploymentJobs.DataAccess
{
	public interface IDeploymentJobDataAccess
	{
		IEnumerable<DeploymentJob> GetCurrentJobs();

		DeploymentJob GetOrCreate(string project, string Service);

		bool TryGetJob(string jobId, out DeploymentJob job);

		DeploymentJob GetJob(string jobId);

		bool DeleteJob(string jobId);

		void SetSuccess(string jobId);

		void SetInProgress(string jobId, string action);

		void SetFail(string jobId, string errorMessage);
	}
}
