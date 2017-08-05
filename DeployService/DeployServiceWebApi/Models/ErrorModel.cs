using System.Net;

namespace DeployServiceWebApi.Exceptions
{
	public class ErrorModel
	{
		public string Status { get; set; }

		public string Title { get; set; }

		public string Detail { get; set; }

		public ErrorModel(string title, string detail, HttpStatusCode statusCode)
		{
			Title = title;
			Detail = detail;
			Status = ((int) statusCode).ToString();
		}
	}
}
