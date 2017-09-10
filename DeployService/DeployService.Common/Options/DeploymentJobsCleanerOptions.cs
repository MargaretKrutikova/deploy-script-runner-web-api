using System;

namespace DeployService.Common.Options
{
    public class DeploymentJobsCleanerOptions
    {
        public int JobCleanupIntervalMinutes { get; set; }

        public TimeSpan JobCleanupTimespan
        {
            get
            {
                return TimeSpan.FromMinutes(JobCleanupIntervalMinutes);
            }
        }
    }
}