using System;

namespace DeployService.Common.Exceptions
{
	public class DeploymentJobNotFoundException : DeployServiceGenericException
	{
		public override string Title { get; set; } = "JobNotFound";

        public string JobId { get; }

		public DeploymentJobNotFoundException(string jobId)
			: base($"Job with id {jobId} was not found.")
		{
		}
	}
}
