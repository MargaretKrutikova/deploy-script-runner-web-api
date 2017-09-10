using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;

namespace DeployServiceWebApi.IntegrationTests.TestBase
{
    public class AuthTokenWebApiFixture : DeployServiceWebApiTestBase
    {
        public string AuthToken { get; private set; }

        public AuthTokenWebApiFixture()
        {
            this.GetOrCreateAuthToken().Wait();
        }

        public async Task GetOrCreateAuthToken()
        {
            if (AuthToken != null) return;

            var jsonString = JsonConvert.SerializeObject(new { userName = "testUser", password = "testPassword" });
            var postContent = new StringContent(jsonString, _defaultEncoding, _postContentType);

            var response = await _client.PostAsync(_authTokenEndpoint, postContent);

            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            AuthToken = JsonConvert.DeserializeObject<dynamic>(responseBody).token;
            _client.DefaultRequestHeaders.Add("Authorization", new[] { $"bearer {AuthToken}" });
        }
    }
    public class AuthTokenWebApiTestBase :
        DeployServiceWebApiTestBase,
        Xunit.IClassFixture<AuthTokenWebApiFixture>
    {
        private AuthTokenWebApiFixture fixture;

        public AuthTokenWebApiTestBase(AuthTokenWebApiFixture fixture)
        {
            this.fixture = fixture;
            SetAuthTokenHeader(_client);
        }
        protected void SetAuthTokenHeader(HttpClient client)
        {
            client.DefaultRequestHeaders.Add("Authorization", new[] { $"bearer {fixture.AuthToken}" });
        }
    }
}