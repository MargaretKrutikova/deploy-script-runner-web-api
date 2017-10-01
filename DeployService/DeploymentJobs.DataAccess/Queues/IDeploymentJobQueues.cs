using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeploymentJobs.DataAccess.Queues
{
    public interface IDeploymentJobQueues
    {
        void InitializeAndRunJobQueues(IEnumerable<string> queues);
        bool TryAddToQueue(string queue, TaskCreator taskCreator);
    }
}