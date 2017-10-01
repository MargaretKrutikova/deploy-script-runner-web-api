using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DeploymentJobs.DataAccess;
using DeploymentJobs.DataAccess.Queues;
using DeploymentSettings.Models;
using DeployService.Common.Exceptions;
using DeployService.Common.Extensions;
using DeployServiceWebApi.Exceptions;
using DeployServiceWebApi.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Events;

namespace DeployServiceWebApi.Services
{
    public class DeploymentService : IDeploymentService
    {
        private readonly ILogger<DeploymentService> _logger;
        private readonly IDeploymentJobDataAccess _jobsDataAccess;
        private readonly IDeploymentJobQueues _queues;

        public DeploymentService(
            ILogger<DeploymentService> logger,
            IDeploymentJobDataAccess jobsDataAccess,
            IDeploymentJobQueues queues)
        {
            _logger = logger;
            _jobsDataAccess = jobsDataAccess;
            _queues = queues;
        }

        public bool TryAddJobToQueue(ServiceSettings settings, out DeploymentJob job)
        {
            var createdJob = _jobsDataAccess.CreateJob(settings.Project, settings.Service);

            TaskCreator taskCreator = async () => await RunJob(createdJob.Id, settings.Scripts);
            if (!_queues.TryAddToQueue(settings.Project, taskCreator)) 
            {
                job = null;
                _jobsDataAccess.SetFail(createdJob.Id, $"Failed to add job to the queue {settings.Project}");
                return false;
            }

            job = createdJob;
            return true;
        }

        private async Task RunJob(string jobId, IEnumerable<DeploymentScript> scripts)
        {
            try
            {
                foreach (var script in scripts)
                {
                    // check if the job hasn't been cancelled and exit if it has.
                    if (_jobsDataAccess.CheckJobStatus(jobId, DeploymentJobStatus.CANCELLED))
                    {
                        _logger.LogInformation($"Job with id ${jobId} has been cancelled. Exiting deployment.");
                        return;
                    }
                    if (!File.Exists(script.Path))
                    {
                        throw new DeploymentException($"File cannot be found: {script.Path}");
                    };

                    await ExecuteScript(script, jobId);
                }

                _jobsDataAccess.SetSuccess(jobId);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error running deployables: {ex.Message}";
                _logger.LogError(errorMessage, ex);

                if (_jobsDataAccess.CheckJobStatus(jobId, DeploymentJobStatus.FAIL)) return;
                
                // set fail if haven't already been set while running deployment scripts.
                _jobsDataAccess.SetFail(jobId, ex.Message);
            }
        }
        
        private async Task ExecuteScript(DeploymentScript script, string jobId)
        {
            var proc = new Process
            {
                StartInfo =
                {
                    FileName = (new FileInfo(script.Path)).FullName,
                    Arguments = script.Arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            _logger.LogInformation($"Starting script {Path.GetFileName(script.Path)}");

             proc.Start();

            _jobsDataAccess.SetInProgress(jobId, $"Running script {Path.GetFileName(script.Path)}", proc);
            
            // read output and error streams async
            proc.OutputDataReceived += (sender, eventArgs) => LogIfNotEmpty(jobId, eventArgs.Data);
            proc.ErrorDataReceived += (sender, eventArgs) => LogIfNotEmpty(jobId, eventArgs.Data, LogEventLevel.Error);

            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            await proc.WaitForExitAsync();

            // check if the job has failed during deployment.
            if (proc.ExitCode != 0 || _jobsDataAccess.CheckJobStatus(jobId, DeploymentJobStatus.FAIL))
            {
                var message = $"Error running script {Path.GetFileName(script.Path)}, exit code: {proc.ExitCode}";
                throw new DeploymentException(message);
            }
        }

        private void LogIfNotEmpty(
            string jobId,
            string logMessage,
            LogEventLevel level = LogEventLevel.Information)
        {
            if (string.IsNullOrWhiteSpace(logMessage)) return;

            // catch deployer-specific error messages when no error code is returned.
            if (MessageHasDeploymentErrors(logMessage)) 
            {
                _jobsDataAccess.SetFail(jobId, "Deploy failed. Check logs for more information.");
            }

            if (level == LogEventLevel.Error)
            {
                _logger.LogError(logMessage);
                return;
            }

            _logger.LogInformation(logMessage);
        }

        private bool MessageHasDeploymentErrors(string message)
        {
            var messageLowerCase = message.ToLower();
            return messageLowerCase.Contains("deploy failed") || messageLowerCase.Contains("there were errors");
        }
    }
}