using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using DeployService.Common.Exceptions;

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

		public IEnumerable<DeploymentJob> GetCurrentJobs() 
		{
			return _deploymentJobsDictionary.Values;
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

		public bool CheckJobStatus(string jobId, DeploymentJobStatus statusToCheck)
		{
			lock (_lockObject)
			{
				if (!_deploymentJobsDictionary.TryGetValue(jobId, out DeploymentJob job))
				{
					throw new DeploymentJobNotFoundException(jobId);
				}
				return job.Status == statusToCheck;
			}
		}

		public DeploymentJob GetJob(string jobId)
		{
			lock (_lockObject)
			{
				if (!_deploymentJobsDictionary.TryGetValue(jobId, out DeploymentJob job))
				{
					throw new DeploymentJobNotFoundException(jobId);
				}
				return job;
			}
		}

		public bool TryGetJob(string jobId, out DeploymentJob job)
		{
			lock (_lockObject)
			{
				return _deploymentJobsDictionary.TryGetValue(jobId, out job);
			}
		}

		public void CancelJob(string jobId)
		{
			lock (_lockObject)
			{
				if (!_deploymentJobsDictionary.TryGetValue(jobId, out DeploymentJob job))
				{
					throw new DeploymentJobNotFoundException(jobId);
				}

				// job that has finished can't be deleted.
				if (job.IsCompleted()) 
				{
					throw new DeployOperationNotAllowedException("Job is completed. Cancel on completed jobs is not allowed.");
				}
				_deploymentJobsDictionary[job.Id] = job.WithStatusCancelled();
				
				// Feature in question since it does process kill.
				// try kill currently running process
				// if (job.CurrentProcess == null || job.CurrentProcess.HasExited) return;
				// try 
				// {
				// 	job.CurrentProcess.Kill();
				// }
				// catch(Exception) 
				// {
				// 	_deploymentJobsDictionary[job.Id] = job.WithStatusCancelled("Failed to kill currently running process");
				// 	throw;
				// }
			}
		}

		public void SetInProgress(string jobId, string action, Process currentProcess = null)
		{
			lock (_lockObject)
			{
				if (!_deploymentJobsDictionary.TryGetValue(jobId, out DeploymentJob job))
				{
					throw new DeploymentJobNotFoundException(jobId);
				}
				_deploymentJobsDictionary[job.Id] = job.WithStatusInProgress(action, currentProcess);
			}
		}

		public void SetSuccess(string jobId)
		{
			lock (_lockObject)
			{
				if (!_deploymentJobsDictionary.TryGetValue(jobId, out DeploymentJob job))
				{
					throw new DeploymentJobNotFoundException(jobId);
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
					throw new DeploymentJobNotFoundException(jobId);
				}
				_deploymentJobsDictionary[job.Id] = job.WithStatusFail(errorMessage);
			}
		}

		public DeploymentJob DeleteJob(string jobId)
		{
			lock (_lockObject)
			{
				if (!_deploymentJobsDictionary.TryGetValue(jobId, out DeploymentJob job))
				{
					throw new DeploymentJobNotFoundException(jobId);
				}

				// job that is running can't be deleted.
				if (job.Status == DeploymentJobStatus.IN_PROGRESS) 
				{
					throw new DeployOperationNotAllowedException("Job is in progress. Delete on running jobs is not allowed.");
				}
				_deploymentJobsDictionary.Remove(jobId);
				return job;
			}
		}

		public void DeleteAllFinished()
		{
			lock (_lockObject)
			{
				var keysToDelete = _deploymentJobsDictionary.Where(kvp => 
						kvp.Value.Status != DeploymentJobStatus.IN_PROGRESS || 
						kvp.Value.Status != DeploymentJobStatus.NOT_STARTED).Select(kvp => kvp.Key).ToArray();

				foreach (var keyToDelete in keysToDelete)
				{
					_deploymentJobsDictionary.Remove(keyToDelete);
				}
			}
		}

		private static string GenerateUid()
		{
			return Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
		}
	}
}
