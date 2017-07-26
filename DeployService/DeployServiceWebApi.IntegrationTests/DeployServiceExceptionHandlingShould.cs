using System;
using System.Net;
using System.Threading.Tasks;
using DeployServiceWebApi.Exceptions;
using DeployServiceWebApi.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DeployServiceWebApi.IntegrationTests
{
	public class DeployServiceExceptionHandlingShould
	{
		private readonly Uri _baseAddress = new Uri("https://localhost/");

		[Fact]
		public async Task Returns500AndDeploymentErrorObject_IfErrorDuringDeployment()
		{
			var builder = new WebHostBuilder()
				.UseStartup<Startup>();

			var server = new TestServer(builder);
			var client = server.CreateClient();
			client.BaseAddress = _baseAddress;

			var response = await client.GetAsync("api/jobs?project=vds&group=error-test");
			Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

			var jsonResponse = JsonConvert.DeserializeObject<ErrorJsonObject>(await response.Content.ReadAsStringAsync());
			Assert.Equal("500", jsonResponse.Status);
			Assert.Equal("DeploymentError", jsonResponse.Title);

			client.Dispose();
			server.Dispose();
		}

		[Fact]
		public async Task Returns500AndStartupErrorObject_IfMissingSettingsFile_InProductionEnvironment()
		{
			var builder = new WebHostBuilder()
				.UseEnvironment("Production")
				.ConfigureServices(services =>
				{
					services.AddSingleton(
						serviceProvider => FakeConfigurationOptions(serviceProvider, settingsPath: "non-existing-path/empty.json"));
				})
				.UseStartup<Startup>();


			var server = new TestServer(builder);
			var client = server.CreateClient();
			client.BaseAddress = _baseAddress;

			var response = await client.GetAsync("api/groups");
			Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);


			var jsonResponse = JsonConvert.DeserializeObject<ErrorJsonObject>(await response.Content.ReadAsStringAsync());
			Assert.Equal("500", jsonResponse.Status);
			Assert.Equal("StartupError", jsonResponse.Title);

			client.Dispose();
			server.Dispose();
		}

		[Fact]
		public async Task Returns404AndGroupNotFoundErrorObject_IfCalledDeployWithNonExistingGroup()
		{
			var builder = new WebHostBuilder()
				.UseStartup<Startup>();

			var server = new TestServer(builder);
			var client = server.CreateClient();
			client.BaseAddress = _baseAddress;

			var response = await client.GetAsync("api/deployGroup/unknown");
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

			var jsonResponse = JsonConvert.DeserializeObject<ErrorJsonObject>(await response.Content.ReadAsStringAsync());
			Assert.Equal("404", jsonResponse.Status);
			Assert.Equal("GroupNotFound", jsonResponse.Title);

			client.Dispose();
			server.Dispose();
		}

		public IOptions<ConfigurationOptions> FakeConfigurationOptions(
			IServiceProvider provider,
			string settingsPath = null,
			string checkoutScriptPath = null)
		{
			return new OptionsManager<ConfigurationOptions>(new IConfigureOptions<ConfigurationOptions>[]
			{
				new ConfigureOptions<ConfigurationOptions>(options =>
				{
					if (settingsPath != null)
					{
						options.DeploySettingsPath = settingsPath;
					}
					if (checkoutScriptPath != null)
					{
						options.RepoUpdateScriptPath = checkoutScriptPath;
					}
				})
			});
		}
	}
}
