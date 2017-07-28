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
		public async Task Returns400AndJobAlreadyRunningErrorObject_IfJobWithProjectAndGropAlreadyRunning()
		{
			var builder = new WebHostBuilder().UseStartup<Startup>();

			var server = new TestServer(builder);
			var client = server.CreateClient();
			client.BaseAddress = _baseAddress;

			var createJobResponse = await client.GetAsync("api/jobs?project=vds&group=long-running-job");
			var createDuplicateJobResponse = await client.GetAsync("api/jobs?project=vds&group=long-running-job");
			Assert.Equal(HttpStatusCode.BadRequest, createDuplicateJobResponse.StatusCode);

			var jsonResponse = JsonConvert.DeserializeObject<ErrorJsonObject>(await createDuplicateJobResponse.Content.ReadAsStringAsync());
			Assert.Equal("400", jsonResponse.Status);
			Assert.Equal("JobAlreadyInProgress", jsonResponse.Title);

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
						serviceProvider => 
						/*new OptionsManager<ConfigurationOptions>(new IConfigureOptions<ConfigurationOptions>[]
						{
							new ConfigureOptions<ConfigurationOptions>(options =>
							{
								options.DeploySettingsPath = "non-existing-path/empty.json";
							})
						}));*/
						FakeConfigurationOptions(serviceProvider, settingsPath: "non-existing-path/empty.json"));
				})
				.UseStartup<Startup>();


			var server = new TestServer(builder);
			var client = server.CreateClient();
			client.BaseAddress = _baseAddress;

			var response = await client.GetAsync("api/settings/groups");
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

			var response = await client.GetAsync("api/jobs?project=unknown&group=unknown");
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

			var jsonResponse = JsonConvert.DeserializeObject<ErrorJsonObject>(await response.Content.ReadAsStringAsync());
			Assert.Equal("404", jsonResponse.Status);
			Assert.Equal("GroupNotFound", jsonResponse.Title);

			client.Dispose();
			server.Dispose();
		}

		private IOptions<ConfigurationOptions> FakeConfigurationOptions(
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
