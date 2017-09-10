using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;

namespace DeployServiceWebApi.IntegrationTests.TestBase
{
    public class DeployServiceWebApiTestBase : IDisposable
    {
        protected readonly TestServer _server;
        protected readonly System.Net.Http.HttpClient _client;
        protected readonly Uri _baseAddress = new Uri("https://localhost/");

        protected const string _postContentType = "application/json";
        protected readonly Encoding _defaultEncoding = Encoding.UTF8;

        protected const string _apiPrefix = "/api";
        protected static readonly string _authTokenEndpoint = $"{_apiPrefix}/auth/token";
        protected static readonly string _jobsEndpoint = $"{_apiPrefix}/jobs";
        protected static readonly string _settingsEndpoint = $"{_apiPrefix}/settings";

        public DeployServiceWebApiTestBase()
        {
            _server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            _client = _server.CreateClient();
            _client.BaseAddress = _baseAddress;
        }

        public async Task<HttpResponseMessage> PostDataAsync(object data, string endPoint)
        {
            var jsonString = JsonConvert.SerializeObject(data);
            var postContent = new StringContent(jsonString, _defaultEncoding, _postContentType);

            var response = await _client.PostAsync(endPoint, postContent);
            return response;
        }

        public void Dispose()
        {
            _server.Dispose();
            _client.Dispose();
        }
    }
}