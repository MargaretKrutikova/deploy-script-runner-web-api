using System;
using DeploymentJobs.DataAccess;

namespace DeployServiceWebApi.Models
{
    public class DeploymentJobLightModel
    {
	    public string Id { get; }

	    public string Project { get; } 

	    public string Service { get; } 

	    public string Status { get; }

	    public DeploymentJobLightModel(
		    string id,
		    string project,
		    string service,
		    string status)
	    {
		    Id = id;
		    Project = project;
		    Service = service;
		    Status = status;
	    }

	    public DeploymentJobLightModel(DeploymentJob job)
	    {
		    Id = job.Id;
		    Project = job.Project;
		    Service = job.Service;
		    Status = job.Status.ToString();
	    }
	}
}
