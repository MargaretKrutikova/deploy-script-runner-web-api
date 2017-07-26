using System;

namespace DeploymentJobs.DataAccess
{
	public class DeploymentJob
    {
	    public string Id { get; }

	    public string Project { get; } // e.g. vds, sds, some-other-ds

	    public string Group { get; } // defined within a project, e.g. stage, wip, live etc.

	    public DeploymentJobStatus Status { get; }

	    public string CurrentAction { get; }

	    public string ErrorMessage { get; }

	    public DateTime? CreatedTime { get; }

	    public DateTime? EndTime { get; }

		public DeploymentJob(string id, string project, string @group)
	    {
		    Id = id;
		    Project = project;
		    Group = @group;
		    Status = DeploymentJobStatus.NOT_STARTED;
		    CreatedTime = DateTime.Now;
	    }

	    private DeploymentJob(
		    string id,
		    string project,
		    string @group,
		    DeploymentJobStatus status,
			string currentAction,
			string errorMessage,
			DateTime? createdTime,
			DateTime? endTime = null)
	    {
		    Id = id;
		    Project = project;
		    Group = @group;
		    Status = status;
		    CurrentAction = currentAction;
		    ErrorMessage = errorMessage;
		    CreatedTime = createdTime;
		    EndTime = endTime;
	    }


	    public DeploymentJob WithStatusFail(string errorMessage)
	    {
		    return new DeploymentJob(
			    this.Id,
			    this.Project,
			    this.Group,
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
				this.Group,
				DeploymentJobStatus.SUCCESS,
				this.CurrentAction,
				null,
				this.CreatedTime,
				DateTime.Now);
		}

		public DeploymentJob WithStatusInProgress(string currentAction = null)
	    {
			return new DeploymentJob(
				this.Id,
				this.Project,
				this.Group,
				DeploymentJobStatus.IN_PROGRESS,
				currentAction,
				null,
				this.CreatedTime);
		}

	    public bool IsCompleted()
	    {
		    return Status == DeploymentJobStatus.FAIL || Status == DeploymentJobStatus.SUCCESS;
	    }
	}
}
