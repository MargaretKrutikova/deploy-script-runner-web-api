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
			string group, 
			SettingsRepo repo, 
			IEnumerable<string> deployables,
			out DeploymentJob job);
	}

	public class DeploymentService : IDeploymentService
	{
		private readonly ILogger<DeploymentService> _logger;
		private readonly IDeploymentJobDataAccess _jobsDataAccess;
		private readonly string _svnCeckoutScriptPath;
		private const string SvnCheckoutFlags = "--non-interactive --trust-server-cert --no-auth-cache";

		public DeploymentService(
			IOptions<ConfigurationOptions> optionsAccessor,
			ILogger<DeploymentService> logger,
			IDeploymentJobDataAccess jobsDataAccess)
		{
			_logger = logger;
			_jobsDataAccess = jobsDataAccess;
			_svnCeckoutScriptPath = optionsAccessor.Value.RepoUpdateScriptPath;
		}

		public bool TryRunJobIfNotInProgress(
			string project, 
			string group,
			SettingsRepo repo, 
			IEnumerable<string> deployables,
			out DeploymentJob job)
		{
			job = _jobsDataAccess.GetOrCreate(project, group);
			if (job.Status == DeploymentJobStatus.IN_PROGRESS)
			{
				return false;
			}

			var jobId = job.Id;
			Task.Run(() => RunJob(jobId, repo, deployables));
			
			return true;
		}

		private void RunJob(
			string jobId, 
			SettingsRepo repo,
			IEnumerable<string> deployables)
		{
			try
			{
				// 1. make sure the settings repository is updated
				_jobsDataAccess.SetInProgress(jobId, "Updating settings repository.");

				UpdateRepository(repo.RemoteUrl, repo.LocalPath);

				// 2. run deployables located in the settings repository
				foreach (var deployable in deployables)
				{
					var path = Path.Combine(repo.LocalPath, deployable);
					if (!File.Exists(path)) continue;

					_jobsDataAccess.SetInProgress(jobId, $"Running deployable {Path.GetFileName(path)}");

					ExecuteScript(path);
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

		private void UpdateRepository(string repoUrl, string localPath)
		{
			var args = $"\"{repoUrl}\" \"{localPath}\" {SvnCheckoutFlags}";
			ExecuteScript(_svnCeckoutScriptPath, args);
		}

		private void ExecuteScript(string scriptPath, string args = "")
		{
			var proc = new Process
			{
				StartInfo =
				{
					FileName = "cmd.exe \"scriptPath\"",
					Arguments = args,
					RedirectStandardOutput = true,
					RedirectStandardError = true
				}
			};
			_logger.LogInformation($"Starting script {Path.GetFileName(scriptPath)}");
			
			proc.Start();

			// read output and error streams async
			proc.OutputDataReceived += (sender, eventArgs) => LogIfNotEmpty(eventArgs.Data);
			proc.ErrorDataReceived += (sender, eventArgs) => LogIfNotEmpty(eventArgs.Data, LogEventLevel.Error);

			proc.BeginOutputReadLine();
			proc.BeginErrorReadLine();

			proc.WaitForExit();

			if (proc.ExitCode != 0)
			{
				var message = $"Error running script {Path.GetFileName(scriptPath)}, exit code: {proc.ExitCode}";
				throw new DeploymentException(message);
			}
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
