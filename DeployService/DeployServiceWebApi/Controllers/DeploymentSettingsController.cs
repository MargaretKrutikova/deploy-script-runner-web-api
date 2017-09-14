using System.Linq;
using DeploymentSettings;
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

        public DeploymentSettingsController(
            IDeploymentSettingsDataService deploymentSettingsService,
            ILogger<DeploymentSettingsController> logger)
        {
            _deploymentSettingsService = deploymentSettingsService;
            _logger = logger;
        }

        // WARNING: this method is not RESTful but rather an RPC.
        [Authorize]
        [HttpGet("reload")]
        public IActionResult ReloadSettings()
        {
            _deploymentSettingsService.ReloadDeploymentSettingsFromFile();
            return Ok();
        }

        [HttpGet("projects")]
        public IActionResult GetProjects()
        {
            return Ok(_deploymentSettingsService.GetProjectsModel());
        }
    }
}
