using System;
using System.ComponentModel.DataAnnotations;

namespace DeployServiceWebApi.Models
{
    public class CreateJobModel
    {
        [Required]
        public string Group { get; set; }

        [Required]
        public string Service { get; set; }
    }
}