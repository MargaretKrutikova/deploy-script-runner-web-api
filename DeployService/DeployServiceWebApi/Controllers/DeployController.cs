using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DeploymentSettings;
using DeployServiceWebApi.Exceptions;
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

	    [HttpGet("groups")]
	    public IActionResult GetGroups()
	    {
		    return Ok(_deploymentSettingsStore.GetGroups().ToList());
	    }

		[HttpGet("deployGroup/{group}")]
	    public async Task<IActionResult> Deploy(string group)
	    {
		    if (!_deploymentSettingsStore
					.TryGetDeployablesByGroup(group, out string[] deployables))
		    {
			    // error object corresponding to missing group.
			    var error = new ErrorJsonObject()
			    {
				    Detail = $"Group {group} was not found.",
				    Title = "GroupNotFound",
				    Status = ((int) HttpStatusCode.NotFound).ToString()
			    };
			    return NotFound(error);
		    }

			// TODO: should be possible to set settings for different projects, t.ex. vds, sds, etc.
		    var repoSettings = _deploymentSettingsStore.GetRepoSettings();
		    await _deploymentService.RunDeployables(repoSettings, deployables);

		    return Ok("Deployed successfully.");
	    }
    }
}
