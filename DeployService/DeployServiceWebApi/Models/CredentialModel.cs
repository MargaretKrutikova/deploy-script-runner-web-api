using System;
using System.ComponentModel.DataAnnotations;

namespace DeployServiceWebApi.Models
{
    public class CredentialModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}