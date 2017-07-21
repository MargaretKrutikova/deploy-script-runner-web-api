using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
		Task RunDeployables(SettingsRepo repo, IEnumerable<string> deployables);
	}

	public class DeploymentService : IDeploymentService
	{
		private readonly ILogger<DeploymentService> _logger;
		private readonly string _svnCeckoutScriptPath;
		private const string SvnCheckoutFlags = "--non-interactive --trust-server-cert --no-auth-cache";

		public DeploymentService(
			IOptions<ConfigurationOptions> optionsAccessor,
			ILogger<DeploymentService> logger)
		{
			_logger = logger;
			_svnCeckoutScriptPath = optionsAccessor.Value.RepoUpdateScriptPath;
		}

		// TODO: should return deployment result
		public async Task RunDeployables(SettingsRepo repo, IEnumerable<string> deployables)
		{
			try
			{
				// 1. make sure the settings repository is updated
				await UpdateRepository(repo.RemoteUrl, repo.LocalPath);

				// 2. run deployables located in the settings repository

				// TODO: check if deployables can be run in parallel
				await Task.WhenAll(deployables
					.Select(d => Path.Combine(repo.LocalPath, d))
					.Where(File.Exists)
					.Select(d => ExecuteScript(d)));
			}
			catch (Exception ex)
			{
				var errorMessage = $"Error running deployables: {ex.Message}";
				_logger.LogError(errorMessage, ex);

				throw ex as DeploymentException ?? new DeploymentException(errorMessage, ex);
			}
		}

		private async Task UpdateRepository(string repoUrl, string localPath)
		{
			var args = $"\"{repoUrl}\" \"{localPath}\" {SvnCheckoutFlags}";

			await ExecuteScript(_svnCeckoutScriptPath, args);
		}

		private async Task ExecuteScript(string scriptPath, string args = "")
		{
			var proc = new Process
			{
				StartInfo =
				{
					FileName = scriptPath,
					Arguments = args,
					RedirectStandardOutput = true,
					RedirectStandardError = true
				}
			};
			_logger.LogInformation($"Starting script {Path.GetFileName(scriptPath)}");

			await Task.Run(() =>
			{
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
			});
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
