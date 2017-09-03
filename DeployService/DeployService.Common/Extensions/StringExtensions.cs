using System;
using System.Linq;

namespace DeployService.Common.Extensions
{
	public static class StringExtensions
	{
        public static string ToCamelCase(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }
            return $"{input.First().ToString().ToLower()}{input.Substring(1)}";
        }
	}
}
