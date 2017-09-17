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
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
using DeployServiceWebApi.IntegrationTests.TestBase;

namespace DeployServiceWebApi.IntegrationTests
{
    public class DeployServiceErrorCases : AuthTokenWebApiTestBase
    {
        public DeployServiceErrorCases(AuthTokenWebApiFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task CreatingJobWithProjectAndGropAlreadyRunning_ShouldReturn_400AndJobAlreadyRunningErrorObject()
        {
            var data = new { project = "test", service = "long-running-job" };
            var createJobResponse = await PostDataAsync(data, _jobsEndpoint);
            var createDuplicateJobResponse = await PostDataAsync(data, _jobsEndpoint);

            Assert.Equal(HttpStatusCode.BadRequest, createDuplicateJobResponse.StatusCode);

            var jsonResponse = JsonConvert.DeserializeObject<ErrorModel>(await createDuplicateJobResponse.Content.ReadAsStringAsync());
            Assert.Equal("400", jsonResponse.Status);
            Assert.Equal("JobAlreadyInProgress", jsonResponse.Title);
        }

        [Fact]
        public async Task GettingGroups_IfMissingSettingsFileInProductionEnvironment_ShouldReturn_500AndStartupErrorObject()
        {
            var builder = new WebHostBuilder()
                .UseEnvironment("Production")
                .ConfigureServices(services =>
                {
                    services.AddSingleton(
                        serviceProvider => new OptionsManager<ConfigurationOptions>(new FakeOptionsFactory()));
                })
                .UseStartup<Startup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();
            client.BaseAddress = _baseAddress;
            SetAuthTokenHeader(client);

            var response = await client.GetAsync($"{_settingsEndpoint}/projects");
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            var jsonResponse = JsonConvert.DeserializeObject<ErrorModel>(await response.Content.ReadAsStringAsync());
            Assert.Equal("500", jsonResponse.Status);
            Assert.Equal("StartupError", jsonResponse.Title);

            client.Dispose();
            server.Dispose();
        }

        [Fact]
        public async Task CreatingJobWithNonExistingGroup_ShouldReturn_404NotFoundAndGroupNotFoundError()
        {
            var data = new { project = "unknown", service = "unknown" };

            var response = await PostDataAsync(data, _jobsEndpoint);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var jsonResponse = JsonConvert.DeserializeObject<ErrorModel>(await response.Content.ReadAsStringAsync());
            Assert.Equal("404", jsonResponse.Status);
            Assert.Equal("ProjectOrServiceNotFound", jsonResponse.Title);
        }

        public class FakeOptionsFactory : IOptionsFactory<ConfigurationOptions>
        {
            public ConfigurationOptions Create(string name) 
            {
                return (new ConfigurationOptions { DeploySettingsPath = "non-existing-path/empty.json" });
            }
        }
    }
}
