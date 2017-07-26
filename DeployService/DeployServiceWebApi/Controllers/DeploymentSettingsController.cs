using System.Linq;
using DeploymentSettings;
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

	    [HttpGet("groups")]
	    public IActionResult GetGroups()
	    {
		    return Ok(_deploymentSettingsStore.GetGroups().ToList());
	    }
    }
}
