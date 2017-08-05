using System;
using DeploymentJobs.DataAccess;

namespace DeployServiceWebApi.Models
{
    public class DeploymentJobModel
    {
	    public string Id { get; }

	    public string Project { get; } 

	    public string Group { get; } 

	    public string Status { get; }

	    public string CurrentAction { get; }

	    public string ErrorMessage { get; }

	    public string CreatedTime { get; }

	    public string EndTime { get; }

	    public DeploymentJobModel(
		    string id,
		    string project,
		    string group,
		    string status,
		    string currentAction,
		    string errorMessage,
		    DateTime? createdTime,
		    DateTime? endTime = null)
	    {
		    Id = id;
		    Project = project;
		    Group = group;
		    Status = status;
		    CurrentAction = currentAction;
		    ErrorMessage = errorMessage;
		    CreatedTime = FormatDate(createdTime);
		    EndTime = FormatDate(endTime);
	    }

	    public DeploymentJobModel(DeploymentJob job)
	    {
		    Id = job.Id;
		    Project = job.Project;
		    Group = job.Group;
		    Status = job.Status.ToString();
		    CurrentAction = job.CurrentAction;
		    ErrorMessage = job.ErrorMessage;
		    CreatedTime = FormatDate(job.CreatedTime);
		    EndTime = FormatDate(job.EndTime);
	    }

	    private static string FormatDate(DateTime? date)
	    {
		    return date.HasValue ? string.Format("{0:G}", date) : null;
	    }
	}
}
