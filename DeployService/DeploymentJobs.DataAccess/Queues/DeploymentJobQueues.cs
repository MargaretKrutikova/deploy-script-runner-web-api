using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DeploymentJobs.DataAccess.Queues
{
    public delegate Task TaskCreator();

    public class DeploymentJobQueues : IDeploymentJobQueues
    {
        private readonly object _lockObject = new object();
        private Dictionary<string, BlockingCollection<TaskCreator>> _jobQueues;
        private List<CancellationTokenSource> _queueCancellationTokens;

        public DeploymentJobQueues()
        {
            _queueCancellationTokens = new List<CancellationTokenSource>();
        }
        public void InitializeAndRunJobQueues(IEnumerable<string> queues)
        {
            lock (_lockObject) 
            {
                _jobQueues = new Dictionary<string, BlockingCollection<TaskCreator>>(
                    queues.ToDictionary(queue => queue, queue => new BlockingCollection<TaskCreator>()));

                // cancel all already running job queues since they will be reloaded.
                foreach (var cancellationToken in _queueCancellationTokens)
                {
                   cancellationToken.Cancel();
                }

                _queueCancellationTokens.Clear();
                foreach (var jobQueue in _jobQueues)
                {
                    RunQueue(jobQueue.Value);
                }
            }
        }

        private void RunQueue(BlockingCollection<TaskCreator> taskCollection)
        {
            var tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            Task.Factory.StartNew(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    TaskCreator taskFunc;
                    try
                    {
                        if (taskCollection.TryTake(out taskFunc, -1, token))
                        {
                            await taskFunc();
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // the task has been canceled.
                        break;
                    }
                }
            }, token);

            _queueCancellationTokens.Add(tokenSource);
        }

        public bool TryAddToQueue(string queue, TaskCreator taskCreator)
        {
            lock (_lockObject) 
            {
                bool isSucces = _jobQueues.TryGetValue(queue, out BlockingCollection<TaskCreator> queueBlockingCollection);
                if (!isSucces) return false;

                return queueBlockingCollection.TryAdd(taskCreator);
            }
        }
    }
}