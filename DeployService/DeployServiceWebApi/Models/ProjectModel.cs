using System;

namespace DeployServiceWebApi.Models
{
    public class ProjectModel
    {
        public string Name { get; set; }

        public ServiceModel[] Services { get; set; }
    }

    public class ServiceModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}