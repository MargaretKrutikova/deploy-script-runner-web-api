using System;
using DeploymentJobs.DataAccess;
using DeployService.Common.Extensions;

namespace DeployServiceWebApi.Models
{
    public class DeploymentJobLightModel
    {
        public string Id { get; }
        public string Project { get; }
        public string Service { get; }
        public string Status { get; }
        public string EndTime { get; }

        public DeploymentJobLightModel(
            string id,
            string project,
            string service,
            string status,
                DateTime? endTime = null)
        {
            Id = id;
            Project = project;
            Service = service;
            Status = status;
            EndTime = endTime.ToPresentationFormat();
        }

        public DeploymentJobLightModel(DeploymentJob job)
        {
            Id = job.Id;
            Project = job.Project;
            Service = job.Service;
            Status = job.Status.ToString();
            EndTime = job.EndTime.ToPresentationFormat();
        }
    }
}
