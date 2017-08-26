using System.Linq;
using DeploymentSettings;
using DeployServiceWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DeployServiceWebApi.Controllers
{
    [Route("api/settings")]
    public class DeploymentSettingsController : Controller
    {
	    private readonly IDeploymentSettingsDataStore _deploymentSettingsStore;
		private readonly ILogger<DeploymentSettingsController> _logger;

	    public DeploymentSettingsController(
		    IDeploymentSettingsDataStore deploymentSettingsStore,
			ILogger<DeploymentSettingsController> logger)
	    {
		    _deploymentSettingsStore = deploymentSettingsStore;
			_logger = logger;
	    }

	    [HttpGet("projects")]
	    public IActionResult GetProjects()
	    {
			ProjectModel[] projects = _deploymentSettingsStore.GetProjects().Select(p => 
				new ProjectModel {
					Name = p.Key,
					Services = p.Value.Services.Select(s => new ServiceModel {
						Name = s.Key,
						Description = s.Value.DisplayText
					}).ToArray()
				}).ToArray();

		    return Ok(projects);
	    }
    }
}
