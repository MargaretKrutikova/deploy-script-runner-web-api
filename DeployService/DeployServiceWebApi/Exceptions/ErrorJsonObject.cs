using System.Net;

namespace DeployServiceWebApi.Exceptions
{
	public class ErrorJsonObject
	{
		public string Status { get; set; }

		public string Title { get; set; }

		public string Detail { get; set; }

		public ErrorJsonObject(string title, string detail, HttpStatusCode statusCode)
		{
			Title = title;
			Detail = detail;
			Status = ((int) statusCode).ToString();
		}
	}
}
