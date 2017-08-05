using System;
using System.ComponentModel.DataAnnotations;

namespace DeployServiceWebApi.Models
{
    public class CreateJobModel
    {
        [Required]
        public string Project { get; set; }

        [Required]
        public string Group { get; set; }
    }
}