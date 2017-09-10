using System;

namespace DeployServiceWebApi.Options
{
    public class JwtOptions
    {
        public double LifeTimeMinutes { get; set; }
        public string SignatureKey { get; set; }
    }
}