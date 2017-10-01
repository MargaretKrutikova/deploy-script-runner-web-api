using System.Linq;
using System.Net;
using DeploymentJobs.DataAccess;
using DeploymentSettings;
using DeployServiceWebApi.Exceptions;
using DeployServiceWebApi.Models;
using DeployServiceWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DeployServiceWebApi.Controllers
{
    [Route("api/settings")]
    public class DeploymentSettingsController : Controller
    {
        private readonly IDeploymentSettingsDataService _deploymentSettingsService;
        private readonly ILogger<DeploymentSettingsController> _logger;
        private readonly IDeploymentJobDataAccess _jobsDataAccess;

        public DeploymentSettingsController(
            IDeploymentJobDataAccess jobsDataAccess,
            IDeploymentSettingsDataService deploymentSettingsService,
            ILogger<DeploymentSettingsController> logger)
        {
            _deploymentSettingsService = deploymentSettingsService;
            _jobsDataAccess = jobsDataAccess;
            _logger = logger;
        }

        // WARNING: this method is not RESTful but rather an RPC.
        [Authorize]
        [HttpGet("reload")]
        public IActionResult ReloadSettings()
        {
            // can only reload settings if there are no unfinished jobs
            if (_jobsDataAccess.HasUnfinishedJobs())
            {
                // error object corresponding to failure to reload settings due to unfinished jobs.
                var error = new ErrorModel(
                    "FailedToReload",
                    "Settings can't be reloaded if there are unfinished jobs.",
                    HttpStatusCode.BadRequest);

                return BadRequest(error);
            }

            _deploymentSettingsService.ReloadDeploymentSettingsAndJobQueues();
            return Ok();
        }

        [HttpGet("projects")]
        public IActionResult GetProjects()
        {
            return Ok(_deploymentSettingsService.GetProjectsModel());
        }
    }
}
