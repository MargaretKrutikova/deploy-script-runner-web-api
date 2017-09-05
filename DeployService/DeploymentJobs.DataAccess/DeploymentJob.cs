using System;
using System.Diagnostics;

namespace DeploymentJobs.DataAccess
{
	public class DeploymentJob
    {
	    public string Id { get; }

	    public string Project { get; } // e.g. vds, sds, some-other-ds

	    public string Service { get; } // defined within a project, e.g. stage, wip, live etc.

	    public DeploymentJobStatus Status { get; }

	    public string CurrentAction { get; }

	    public string ErrorMessage { get; }

	    public DateTime? CreatedTime { get; }

	    public DateTime? EndTime { get; }

		public Process CurrentProcess { get; }

		public DeploymentJob(string id, string project, string service)
		{
			Id = id;
			Project = project;
			Service = service;
			Status = DeploymentJobStatus.NOT_STARTED;
			CreatedTime = DateTime.Now;
		}

	  private DeploymentJob(
		  string id,
		  string project,
		  string service,
		  DeploymentJobStatus status,
			string currentAction,
			string errorMessage,
			DateTime? createdTime,
			DateTime? endTime = null,
			Process currentProcess = null)
	    {
		    Id = id;
		    Project = project;
		    Service = service;
		    Status = status;
		    CurrentAction = currentAction;
		    ErrorMessage = errorMessage;
		    CreatedTime = createdTime;
		    EndTime = endTime;
				CurrentProcess = currentProcess;
	    }

	    public DeploymentJob WithStatusFail(string errorMessage)
	    {
		    return new DeploymentJob(
			    this.Id,
			    this.Project,
			    this.Service,
			    DeploymentJobStatus.FAIL,
			    this.CurrentAction,
			    errorMessage,
			    this.CreatedTime,
			    DateTime.Now);
		}

	    public DeploymentJob WithStatusSuccess()
		{
			return new DeploymentJob(
				this.Id,
				this.Project,
				this.Service,
				DeploymentJobStatus.SUCCESS,
				this.CurrentAction,
				null,
				this.CreatedTime,
				DateTime.Now);
		}

		public DeploymentJob WithStatusInProgress(
			string currentAction = null, 
			Process currentProcess = null)
	    {
			return new DeploymentJob(
				this.Id,
				this.Project,
				this.Service,
				DeploymentJobStatus.IN_PROGRESS,
				currentAction,
				null,
				this.CreatedTime,
				null,
				currentProcess);
		}

		public DeploymentJob WithStatusCancelled(string errorMessage = null)
	    {
			return new DeploymentJob(
				this.Id,
				this.Project,
				this.Service,
				DeploymentJobStatus.CANCELLED,
				this.CurrentAction,
				errorMessage,
				this.CreatedTime,
				DateTime.Now);
		}

	    public bool IsCompleted()
	    {
		    return Status == DeploymentJobStatus.FAIL || 
							Status == DeploymentJobStatus.SUCCESS || 
							Status == DeploymentJobStatus.CANCELLED;
	    }
	}
}
