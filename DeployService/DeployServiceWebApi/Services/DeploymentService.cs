using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DeploymentSettings;

namespace DeployServiceWebApi.Services
{
	public interface IDeploymentService
	{
		Task RunDeployables(
			string settingsRepoUrl, 
			string settingsLocalPath, 
			IEnumerable<string> deployables);
	}

	public class DeploymentService : IDeploymentService
	{
	    private readonly string _svnCeckoutScriptPath;
	    private const string SvnCheckoutFlags = "--non-interactive --trust-server-cert --no-auth-cache";

	    public DeploymentService(IConfigurationService configurationService)
	    {
		    _svnCeckoutScriptPath = configurationService.GetRepoUpdateScriptPath();
	    }

		// should return deployment result
		public async Task RunDeployables(
			string settingsRepoUrl, 
			string settingsLocalPath, 
			IEnumerable<string> deployables)
		{
			// 1. make the settings repository is updated
			await UpdateRepository(settingsRepoUrl, settingsLocalPath);

			// 2. run deployables located in the settings repository

			// TODO: check if deployables can be run in parallel
			await Task.WhenAll(deployables
				.Select(d => Path.Combine(settingsLocalPath, d))
				.Where(File.Exists)
				.Select(d => ExecuteScript(d)));
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
					throw new Exception(error);
					// throw new InvalidOperationException($"Cant run scriptPath");
				}
			});
		}
	}
}
