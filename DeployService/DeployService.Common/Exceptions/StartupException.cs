using System;

namespace DeployService.Common.Exceptions
{
    public class StartupException : DeployServiceGenericException
    {
        public override string Title { get; set; } = "StartupError";

        public StartupException(string message)
            : base(message)
        {
        }

        public StartupException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
