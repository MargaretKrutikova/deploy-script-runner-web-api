namespace DeployServiceWebApi.Options
{
    public class ConfigurationOptions
    {
        public ConfigurationOptions()
        {
            CorsOrigins = new string []{};
        }
        
        public string DeploySettingsPath { get; set; }
        public string[] CorsOrigins { get; set; }
    }
}