using System;

namespace DeployService.Common.Exceptions
{
    public class DeployOperationNotAllowedException : DeployServiceGenericException
    {
        public override string Title { get; set; } = "OperationNotAllowed";

        public DeployOperationNotAllowedException(string message)
            : base(message)
        {
        }

        public DeployOperationNotAllowedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
