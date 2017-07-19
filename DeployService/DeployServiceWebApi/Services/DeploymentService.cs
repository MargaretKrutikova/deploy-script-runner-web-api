using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DeploymentSettings.Models;
using DeployServiceWebApi.Exceptions;
using DeployServiceWebApi.Options;
using Microsoft.Extensions.Options;

namespace DeployServiceWebApi.Services
{
	public interface IDeploymentService
	{
		Task RunDeployables(SettingsRepo repo, IEnumerable<string> deployables);
	}

	public class DeploymentService : IDeploymentService
	{
	    private readonly string _svnCeckoutScriptPath;
	    private const string SvnCheckoutFlags = "--non-interactive --trust-server-cert --no-auth-cache";

	    public DeploymentService(IOptions<ConfigurationOptions> optionsAccessor)
	    {
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
				// TODO: log the original exception
				var deployServiceEx = ex as DeploymentException ??
				                      new DeploymentException($"Error running deployables: {ex.Message}", ex);
				throw deployServiceEx;
			}
		}

		private async Task UpdateRepository(string repoUrl, string localPath)
		{
			var args = $"\"{repoUrl}\" \"{localPath}\" {SvnCheckoutFlags}";

			await ExecuteScript(_svnCeckoutScriptPath, args);
	    }

	    private static async Task ExecuteScript(string scriptPath, string args = "")
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
		    await Task.Run(() =>
		    {
			    try
			    {

				    proc.Start();

				    proc.OutputDataReceived += (sender, eventArgs) =>
				    {
					    string output = eventArgs.Data;
						// TODO: log output
					};

				    proc.BeginOutputReadLine();

				    string error = proc.StandardError.ReadToEnd();
					// TODO: log error

					proc.WaitForExit();

				    if (!string.IsNullOrWhiteSpace(error))
				    {
					    var message = $"Error running script {scriptPath} with args {args} : {error}";
					    throw new DeploymentException(message);
				    }
			    }
			    catch (Exception ex)
			    {
				    throw new DeploymentException($"Error running script {scriptPath} with args {args}", ex);
				}
		    });
		}
	}
}
