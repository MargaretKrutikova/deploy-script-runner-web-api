using System.Collections.Generic;
using System.Linq;
using System.Net;
using DeploymentJobs.DataAccess;
using DeploymentSettings;
using DeploymentSettings.Models;
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
        [HttpGet]
        [Authorize]
        public IActionResult Get()
        {
            return Ok(_jobsDataAccess.GetCurrentJobs()
                            .Select(job => new DeploymentJobLightModel(job)).ToArray());
        }

        // GET api/jobs/5
        [HttpGet("{id}")]
        [Authorize]
        public IActionResult Get(string id)
        {
            var job = _jobsDataAccess.GetJob(id);
            return Ok(new DeploymentJobModel(job));
        }

        // POST api/jobs
        [HttpPost]
        [Authorize]
        [ValidateModel]
        public IActionResult Post([FromBody] CreateJobModel model)
        {
            if (!_deploymentSettings.TryGetServiceSettings(model.Group, model.Service, out ServiceSettings settings))
            {
                // error object corresponding to missing group/service.
                var error = new ErrorModel(
                    "GroupOrServiceNotFound",
                    $"Group {model.Group} or service {model.Service} was not found.",
                    HttpStatusCode.NotFound);

                return NotFound(error);
            }

            if (!_deploymentService.TryAddJobToQueue(settings, out DeploymentJob job))
            {
                // error object corresponding to failure adding job to the queue.
                var error = new ErrorModel(
                    "FailedAddToQueue",
                    $"Job for the group {settings.Group} and service {settings.Service} is already running",
                    HttpStatusCode.BadRequest);

                return BadRequest(error);
            }

            return Accepted(new DeploymentJobModel(job));
        }

        // PUT api/jobs/71n8Z1gvH0aJpjiQNXwSCg
        [HttpPut("{id}")]
        [Authorize]
        [ValidateModel]
        public IActionResult Put(string id, [FromBody]UpdateJobModel jobModel)
        {
            // only cancelled is supported for now.
            if (jobModel.Status != DeploymentJobStatus.CANCELLED)
            {
                var error = new ErrorModel(
                    "JobStatusNotSupported",
                    $"Status ${jobModel.Status} is currently not supported for job updates.",
                    HttpStatusCode.BadRequest); // ?? 422 Unprocessable Entity

                return BadRequest(error);
            }

            _jobsDataAccess.CancelJob(id);
            return Ok();
        }

        // DELETE api/jobs/71n8Z1gvH0aJpjiQNXwSCg
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(string id)
        {
            DeploymentJob deletedJob = _jobsDataAccess.DeleteJob(id);

            return Ok(new DeploymentJobModel(deletedJob));
        }

        // DELETE api/jobs
        [HttpDelete]
        [Authorize]
        public IActionResult DeleteAll()
        {
            _jobsDataAccess.DeleteAllFinished();

            return NoContent();
        }
    }
}