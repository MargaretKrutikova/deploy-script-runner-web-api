using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using DeploymentSettings;
using DeployServiceWebApi.Filters;
using DeployServiceWebApi.Models;
using DeployServiceWebApi.Options;
using DeployServiceWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DeployServiceWebApi.Controllers
{
    [Route("api/auth")]
    public class AuthController : Controller
    {
		private readonly ILogger<AuthController> _logger;
        private readonly JwtOptions _jwtOptions;
        private readonly IUserService _userService;

        public AuthController(
			ILogger<AuthController> logger,
			IUserService userService,
			IOptions<JwtOptions> jwtOptionsAccessor)
	    {
			_logger = logger;
			_userService = userService;
			_jwtOptions = jwtOptionsAccessor.Value;
	    }

		[ValidateModel]
		[HttpPost("token")]
		public IActionResult CreateToken([FromBody] CredentialModel model)
		{
			// make sure that the credentials are valid
			if (!_userService.Authenticate(model.UserName, model.Password))
			{
				return Unauthorized();
			}

			// create token claims
			var claims = new []
			{
				new Claim(JwtRegisteredClaimNames.Sub, model.UserName),
				// jti - string that claims uniqueness for the current jwt
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			var now = DateTime.UtcNow;

			// specify signing credentials
			var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtOptions.SignatureKey));
			var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _jwtOptions.Issuer,
				audience: _jwtOptions.Audience,
				claims: claims,
				notBefore: now,
				expires: now.AddMinutes(_jwtOptions.LifeTimeMinutes),
				signingCredentials: creds
			);

			return Ok(new {
				token = new JwtSecurityTokenHandler().WriteToken(token),
				expiration = token.ValidTo
			});
		}
    }
}
