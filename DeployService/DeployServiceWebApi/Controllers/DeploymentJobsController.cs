using System.Net;
using DeploymentJobs.DataAccess;
using DeploymentSettings;
using DeployServiceWebApi.Exceptions;
using DeployServiceWebApi.Models;
using DeployServiceWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeployServiceWebApi.Controllers
{
	[Route("api/jobs")]
	public class DeploymentJobsController : Controller
	{
		private readonly IDeploymentSettingsDataStore _deploymentSettings;
		private readonly IDeploymentService _deploymentService;
		private readonly IDeploymentJobDataAccess _jobsDataAccess;

		public DeploymentJobsController(
			IDeploymentJobDataAccess jobsDataAccess, 
			IDeploymentSettingsDataStore deploymentSettings, 
			IDeploymentService deploymentService)
		{
			_jobsDataAccess = jobsDataAccess;
			_deploymentSettings = deploymentSettings;
			_deploymentService = deploymentService;
		}

		// GET api/values
		/*[HttpGet]
		public IEnumerable<string> Get()
		{
			return new string[] { "value1", "value2" };
		}*/

		// GET api/jobs/5
		[HttpGet("{id}")]
		public IActionResult Get(string id)
		{
			if (!_jobsDataAccess.TryGetJob(id, out DeploymentJob job))
			{
				// error object corresponding to missing job.
				var error = new ErrorJsonObject(
					"JobNotFound", 
					$"Job with id {id} was not found.",
					HttpStatusCode.NotFound);

				return NotFound(error);
			}

			return Ok(new DeploymentJobDto(job));
		}

		// GET api/jobs/5
		[HttpGet]
		public IActionResult Get(string project, string group)
		{
			// TODO: project should be included in the deplyment settings
			if (!_deploymentSettings.TryGetDeployablesByGroup(group, out string[] deployables))
			{
				// error object corresponding to missing group.
				var error = new ErrorJsonObject(
					"GroupNotFound",
					$"Group {group} was not found.",
					HttpStatusCode.NotFound);

				return NotFound(error);
			}

			var repoSettings = _deploymentSettings.GetRepoSettings();

			if (!_deploymentService.TryRunJobIfNotInProgress(
				project, group, repoSettings, deployables, out DeploymentJob job))
			{
				// error object corresponding to job already running.
				// TODO: probably return currently running job?
				var error = new ErrorJsonObject(
					"JobAlreadyInProgress",
					$"Job for the project {project} and group {group} is already running",
					HttpStatusCode.BadRequest);

				return BadRequest(error);
			}

			return Accepted(new DeploymentJobDto(job));
		}

		// POST api/values
		[HttpPost]
		public void Post([FromBody]string value)
		{
		}

		// PUT api/values/5
		[HttpPut("{id}")]
		public void Put(int id, [FromBody]string value)
		{
		}

		// DELETE api/values/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}
	}
}