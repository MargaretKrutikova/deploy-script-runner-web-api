using System;
using System.ComponentModel.DataAnnotations;
using DeploymentJobs.DataAccess;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DeployServiceWebApi.Models
{
    public class UpdateJobModel
    {
        [Required]
        [EnumDataType(typeof(DeploymentJobStatus))]
        [JsonConverter(typeof(StringEnumConverter))]
        public DeploymentJobStatus Status { get; set; }
    }
}