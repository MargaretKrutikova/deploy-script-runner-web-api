using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DeploymentJobs.DataAccess;
using DeploymentSettings.Models;
using DeployServiceWebApi.Exceptions;
using DeployServiceWebApi.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Events;

namespace DeployServiceWebApi.Services
{
	public interface IDeploymentService
	{
		bool TryRunJobIfNotInProgress(
			string project, 
			string service, 
			List<DeploymentScript> scripts,
			out DeploymentJob job);
	}

	public class DeploymentService : IDeploymentService
	{
		private readonly ILogger<DeploymentService> _logger;
		private readonly IDeploymentJobDataAccess _jobsDataAccess;
		//private const string SvnCheckoutFlags = "--non-interactive --trust-server-cert --no-auth-cache";

		public DeploymentService(
			ILogger<DeploymentService> logger,
			IDeploymentJobDataAccess jobsDataAccess)
		{
			_logger = logger;
			_jobsDataAccess = jobsDataAccess;
		}

		public bool TryRunJobIfNotInProgress(
			string project, 
			string service, 
			List<DeploymentScript> scripts,
			out DeploymentJob job)
		{
			job = _jobsDataAccess.GetOrCreate(project, service);
			if (job.Status == DeploymentJobStatus.IN_PROGRESS)
			{
				return false;
			}

			var jobId = job.Id;
			Task.Run(() => RunJob(jobId, scripts));
			
			return true;
		}

		private void RunJob(string jobId, List<DeploymentScript> scripts)
		{
			try
			{
				foreach (var script in scripts)
				{
					_jobsDataAccess.SetInProgress(jobId, $"Running script {Path.GetFileName(script.Path)}");
					
					if (!File.Exists(script.Path)) 
					{
						throw new DeploymentException($"File cannot be found: {script.Path}");
					};

					ExecuteScript(script);
				}

				_jobsDataAccess.SetSuccess(jobId);
			}
			catch (Exception ex)
			{
				var errorMessage = $"Error running deployables: {ex.Message}";

				_jobsDataAccess.SetFail(jobId, errorMessage);
				_logger.LogError(errorMessage, ex);
			}
		}
		private void ExecuteScript(DeploymentScript script)
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

			// read output and error streams async
			proc.OutputDataReceived += (sender, eventArgs) => LogIfNotEmpty(eventArgs.Data);
			proc.ErrorDataReceived += (sender, eventArgs) => LogIfNotEmpty(eventArgs.Data, LogEventLevel.Error);

			proc.BeginOutputReadLine();
			proc.BeginErrorReadLine();

			proc.WaitForExit();

			if (proc.ExitCode != 0)
			{
				var message = $"Error running script {Path.GetFileName(script.Path)}, exit code: {proc.ExitCode}";
				throw new DeploymentException(message);
			}
			//proc.Id
		}

		private void LogIfNotEmpty(string logMessage, LogEventLevel level = LogEventLevel.Information)
		{
			if (string.IsNullOrWhiteSpace(logMessage)) return;

			if (level == LogEventLevel.Error)
			{
				_logger.LogError(logMessage);
				return;
			}

			_logger.LogInformation(logMessage);
		}
	}
}
