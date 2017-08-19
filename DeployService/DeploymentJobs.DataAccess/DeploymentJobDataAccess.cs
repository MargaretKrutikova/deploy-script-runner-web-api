using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DeploymentJobs.DataAccess
{
	public class DeploymentJobDataAccess : IDeploymentJobDataAccess
	{
		private readonly Dictionary<string, DeploymentJob> _deploymentJobsDictionary;

		private readonly object _lockObject = new object();

		public DeploymentJobDataAccess()
	    {
		    _deploymentJobsDictionary = new Dictionary<string, DeploymentJob>();
		}

		public DeploymentJob GetOrCreate(
			string project,
			string service)
		{
			lock (_lockObject)
			{
				var jobInProgress = _deploymentJobsDictionary.Values.FirstOrDefault(
					j => j.Project == project &&
					     j.Service == service &&
					     j.Status == DeploymentJobStatus.IN_PROGRESS);

				if (jobInProgress != null) return jobInProgress;

				var newJob = new DeploymentJob(GenerateUid(), project, service);
				_deploymentJobsDictionary.Add(newJob.Id, newJob);

				return newJob;
			}
		}

		public bool TryGetJob(string jobId, out DeploymentJob job)
		{
			lock (_lockObject)
			{
				return _deploymentJobsDictionary.TryGetValue(jobId, out job);
			}
		}

		public void SetInProgress(string jobId, string action)
		{
			lock (_lockObject)
			{
				if (!_deploymentJobsDictionary.TryGetValue(jobId, out DeploymentJob job))
				{
					throw new KeyNotFoundException($"Job with id {jobId} was not found.");
				}
				_deploymentJobsDictionary[job.Id] = job.WithStatusInProgress(action);
			}
		}

		public void SetSuccess(string jobId)
		{
			lock (_lockObject)
			{
				if (!_deploymentJobsDictionary.TryGetValue(jobId, out DeploymentJob job))
				{
					throw new KeyNotFoundException($"Job with id {jobId} was not found.");
				}
				_deploymentJobsDictionary[job.Id] = job.WithStatusSuccess();
			}
		}

		public void SetFail(string jobId, string errorMessage)
		{
			lock (_lockObject)
			{
				if (!_deploymentJobsDictionary.TryGetValue(jobId, out DeploymentJob job))
				{
					throw new KeyNotFoundException($"Job with id {jobId} was not found.");
				}
				_deploymentJobsDictionary[job.Id] = job.WithStatusFail(errorMessage);
			}
		}

		public bool DeleteJob(string jobId)
		{
			lock (_lockObject)
			{
				return _deploymentJobsDictionary.Remove(jobId);
			}
		}

		private static string GenerateUid()
	    {
			return Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
		}
    }
}
