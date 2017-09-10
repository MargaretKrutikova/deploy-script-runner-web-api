using System;
using DeploymentJobs.DataAccess;
using DeployService.Common.Extensions;

namespace DeployServiceWebApi.Models
{
    public class DeploymentJobModel
    {
        public string Id { get; }
        public string Project { get; }
        public string Service { get; }
        public string Status { get; }
        public string CurrentAction { get; }
        public string ErrorMessage { get; }
        public string CreatedTime { get; }
        public string EndTime { get; }

        public DeploymentJobModel(
            string id,
            string project,
            string service,
            string status,
            string currentAction,
            string errorMessage,
            DateTime? createdTime,
            DateTime? endTime = null)
        {
            Id = id;
            Project = project;
            Service = service;
            Status = status;
            CurrentAction = currentAction;
            ErrorMessage = errorMessage;
            CreatedTime = createdTime.ToPresentationFormat();
            EndTime = endTime.ToPresentationFormat();
        }

        public DeploymentJobModel(DeploymentJob job)
        {
            Id = job.Id;
            Project = job.Project;
            Service = job.Service;
            Status = job.Status.ToString();
            CurrentAction = job.CurrentAction;
            ErrorMessage = job.ErrorMessage;
            CreatedTime = job.CreatedTime.ToPresentationFormat();
            EndTime = job.EndTime.ToPresentationFormat();
        }
    }
}
