using System.Threading.Tasks;
using DeploymentSettings;
using DeployServiceWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeployServiceWebApi.Controllers
{
    [Route("api")]
    public class DeployController : Controller
    {
	    private readonly IDeploymentSettingsDataStore _deploymentSettingsStore;
	    private readonly IDeploymentService _deploymentService;

	    public DeployController(
		    IDeploymentSettingsDataStore deploymentSettingsStore,
		    IDeploymentService deploymentService)
	    {
		    _deploymentSettingsStore = deploymentSettingsStore;
		    _deploymentService = deploymentService;
	    }

	    [HttpGet("deployGroup/{groupId}")]
	    public async Task<IActionResult> Deploy(string groupId)
	    {
		    if (!_deploymentSettingsStore.TryGetDeployablesByGroup(groupId, out string[] deployables))
		    {
			    return NotFound($"Group {groupId} was not found.");
		    }

		    var repoSettings = _deploymentSettingsStore.GetRepoSettings();

		    await _deploymentService.RunDeployables(
				repoSettings.RemoteUrl, 
				repoSettings.LocalPath,
			    deployables);

		    return Ok("Deployed successfully.");
	    }
    }
}
