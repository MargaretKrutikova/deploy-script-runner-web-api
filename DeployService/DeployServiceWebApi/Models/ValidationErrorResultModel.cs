using System.Collections.Generic;
using System.Linq;
using System.Net;
using DeployService.Common.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DeployServiceWebApi.Exceptions
{
    public class ValidationErrorResultModel : ErrorModel
    {
        public List<ValidationError> Errors { get; }

        public ValidationErrorResultModel(string detail, ModelStateDictionary modelState)
            : base("ValidationError", detail, HttpStatusCode.BadRequest)
        {
            Errors = modelState.Keys
                    .SelectMany(key => modelState[key].Errors.Select(
                        x => new ValidationError(key.ToCamelCase(), x.ErrorMessage)))
                    .ToList();
        }
    }
}