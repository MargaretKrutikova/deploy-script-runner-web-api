using System;
using System.ComponentModel.DataAnnotations;
using DeploymentJobs.DataAccess;

namespace DeployServiceWebApi.Models
{
    public class UpdateJobModel
    {
        [Required]
        public DeploymentJobStatus Status { get; set; }
    }
}