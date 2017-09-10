using System;
using System.Threading;
using DeployService.Common.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DeploymentJobs.DataAccess
{
    public interface IDeploymentJobsCleaner 
    {
        void Start();
        void Stop();
    }

    public class DeploymentJobsCleaner : IDeploymentJobsCleaner
    {
        private readonly ILogger<DeploymentJobsCleaner> _logger;
        private IDeploymentJobDataAccess _jobDataAccess;
        private readonly Timer _cleanerTimer;
        private TimeSpan _timerPeriod;

        public DeploymentJobsCleaner(
            ILogger<DeploymentJobsCleaner> logger,
            IDeploymentJobDataAccess jobDataAccess, 
            IOptions<DeploymentJobsCleanerOptions> options)
        {
            _jobDataAccess = jobDataAccess;
            _logger = logger;
            _timerPeriod = options.Value.JobCleanupTimespan;

            _cleanerTimer = new Timer(
                this.CleanupEventHandler, 
                null, 
                Timeout.Infinite, 
                Timeout.Infinite);
        }

        public void Start()
        {
            _cleanerTimer.Change(TimeSpan.Zero, _timerPeriod);
            _logger.LogInformation($"Deployment jobs cleaner started at {DateTime.Now}");
        }
        public void Stop()
        {
            _cleanerTimer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
            _logger.LogInformation($"Deployment jobs cleaner stopped at {DateTime.Now}");
        }

        private void CleanupEventHandler(object state)
        {
            try
            {
                _jobDataAccess.DeleteAllFinished();
                _logger.LogInformation($"Cleaned jobs at {DateTime.Now}");
            }
            catch(Exception ex)
            {
                _logger.LogError("Exception occured when removing deployment jobd.", ex);
            }
        }
    }
}