using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace DeployServiceWebApi.Exceptions
{
	public static class ExceptionHandlingMiddlewareExtensions
	{
		public static IApplicationBuilder UseExceptionHandlingMiddleware(
			this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<ExceptionHandlingMiddleware>();
		}
	}
	public class ExceptionHandlingMiddleware
	{
		private readonly RequestDelegate _next;

		public ExceptionHandlingMiddleware(RequestDelegate next)
		{
			this._next = next;
		}

		public async Task Invoke(HttpContext context /* other scoped dependencies */)
		{
			try
			{
				// Call the next delegate/middleware in the pipeline
				await _next(context);
			}
			catch (Exception ex)
			{
				await HandleExceptionAsync(context, ex);
			}
		}

		private static Task HandleExceptionAsync(HttpContext context, Exception exception)
		{
			// TODO: log exception?

			var code = HttpStatusCode.InternalServerError;
			var errorTitle = "UnknownError";

			var deployServiceException = exception as DeployServiceGenericException;
			if (deployServiceException != null)
			{
				errorTitle = deployServiceException.Title;
			}
			var error = new ErrorJsonObject
			{
				Status = ((int)code).ToString(),
				Title = errorTitle,
				Detail = exception.Message
			};

			var result = JsonConvert.SerializeObject(error);

			context.Response.ContentType = "application/json";
			context.Response.StatusCode = (int)code;
			return context.Response.WriteAsync(result);
		}
	}
}
