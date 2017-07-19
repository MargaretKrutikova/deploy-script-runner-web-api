using System;

namespace DeployServiceWebApi.Exceptions
{
	public class DeployServiceGenericException : Exception
	{
		public virtual string Title { get; set; } = "GenericError";

		public DeployServiceGenericException()
		{
		}

		public DeployServiceGenericException(string message)
			: base(message)
		{
		}

		public DeployServiceGenericException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
