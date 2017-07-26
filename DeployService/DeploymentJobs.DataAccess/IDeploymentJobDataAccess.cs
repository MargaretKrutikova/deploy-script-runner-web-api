namespace DeploymentJobs.DataAccess
{
	public interface IDeploymentJobDataAccess
	{
		DeploymentJob GetOrCreate(string project, string group);

		bool TryGetJob(string jobId, out DeploymentJob job);

		bool DeleteJob(string jobId);

		void SetSuccess(string jobId);

		void SetInProgress(string jobId, string action);

		void SetFail(string jobId, string errorMessage);
	}
}
