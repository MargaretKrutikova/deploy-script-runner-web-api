using System.Net;
using DeploymentJobs.DataAccess;
using DeploymentSettings;
using DeployServiceWebApi.Exceptions;
using DeployServiceWebApi.Filters;
using DeployServiceWebApi.Models;
using DeployServiceWebApi.Services;
using Microsoft.AspNetCore.Authorization;
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

		// GET api/jobs
		/*[HttpGet]
		public IActionResult Get()
		{
			return new string[] { "value1", "value2" };
		}*/

		// GET api/jobs/5
		[HttpGet("{id}")]
		[Authorize]
		public IActionResult Get(string id)
		{
			if (!_jobsDataAccess.TryGetJob(id, out DeploymentJob job))
			{
				// error object corresponding to missing job.
				var error = new ErrorModel(
					"JobNotFound", 
					$"Job with id {id} was not found.",
					HttpStatusCode.NotFound);

				return NotFound(error);
			}

			return Ok(new DeploymentJobModel(job));
		}

		// POST api/jobs
		[HttpPost]
		[Authorize]
		[ValidateModel]
		public IActionResult Post([FromBody] CreateJobModel jobModel)
		{
			// TODO: project should be included in the deployment settings
			if (!_deploymentSettings.TryGetDeployablesByGroup(jobModel.Group, out string[] deployables))
			{
				// error object corresponding to missing group.
				var error = new ErrorModel(
					"GroupNotFound",
					$"Group {jobModel.Group} was not found.",
					HttpStatusCode.NotFound);

				return NotFound(error);
			}

			var repoSettings = _deploymentSettings.GetRepoSettings();

			if (!_deploymentService.TryRunJobIfNotInProgress(
				jobModel.Project, jobModel.Group, repoSettings, deployables, out DeploymentJob job))
			{
				// error object corresponding to job already running.
				// TODO: probably return currently running job?
				var error = new ErrorModel(
					"JobAlreadyInProgress",
					$"Job for the project {jobModel.Project} and group {jobModel.Group} is already running",
					HttpStatusCode.BadRequest);

				return BadRequest(error);
			}

			return Accepted(new DeploymentJobModel(job));
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