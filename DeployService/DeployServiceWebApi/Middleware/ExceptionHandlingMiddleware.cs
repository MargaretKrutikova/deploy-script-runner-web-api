using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DeployServiceWebApi.Exceptions
{
	public class ExceptionHandlingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<ExceptionHandlingMiddleware> _logger;

		public ExceptionHandlingMiddleware(
			RequestDelegate next, 
			ILogger<ExceptionHandlingMiddleware> logger)
		{
			this._next = next;
			_logger = logger;
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

		private Task HandleExceptionAsync(HttpContext context, Exception exception)
		{
			_logger.LogError($"Error occured at ${context.Request.Path}", exception);

			var code = HttpStatusCode.InternalServerError;
			var errorTitle = "UnknownError";

			var deployServiceException = exception as DeployServiceGenericException;
			if (deployServiceException != null)
			{
				errorTitle = deployServiceException.Title;
			}

			var error = new ErrorModel(errorTitle, exception.Message, code);
			var result = JsonConvert.SerializeObject(error);

			context.Response.ContentType = "application/json";
			context.Response.StatusCode = (int)code;
			return context.Response.WriteAsync(result);
		}
	}
}
