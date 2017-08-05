using System;

namespace DeployServiceWebApi.Options
{
    public class JwtOptions
    {
        public double LifeTimeMinutes { get; set; }

        public string SignatureKey { get; set; }

	    public string Audience { get; set; }

        public string Issuer { get; set; }
	}
}