using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DeployService.Common.Extensions
{
    public static class ProcessExtensions
    {
        public static Task WaitForExitAsync(this Process p)
        {
            var tcs = new TaskCompletionSource<object>();
            p.EnableRaisingEvents = true;
            p.Exited += (s,e) => tcs.TrySetResult(null);
            return tcs.Task;
        }
    }
}
