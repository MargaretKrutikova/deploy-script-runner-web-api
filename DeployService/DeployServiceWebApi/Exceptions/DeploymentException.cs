using System;

namespace DeployServiceWebApi.Exceptions
{
	public class DeploymentException : DeployServiceGenericException
	{
		public override string Title { get; set; } = "DeploymentError";

		public DeploymentException(string message)
			: base(message)
		{
		}

		public DeploymentException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
