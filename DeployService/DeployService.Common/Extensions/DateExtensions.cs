using System;
using System.Linq;

namespace DeployService.Common.Extensions
{
    public static class DateExtensions
    {
        public static string ToPresentationFormat(this DateTime? date)
        {
            return date.HasValue ? string.Format("{0:G}", date) : "";
        }
    }
}
