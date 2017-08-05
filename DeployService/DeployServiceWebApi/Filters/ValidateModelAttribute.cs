using DeployServiceWebApi.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DeployServiceWebApi.Filters
{
    public class ValidateModelAttribute : ActionFilterAttribute 
    {
        public override void OnActionExecuting(ActionExecutingContext context) 
        {
            base.OnActionExecuting(context);

            if (!context.ModelState.IsValid) 
            {
                context.Result = new BadRequestObjectResult(
                    new ValidationErrorResultModel("Validation failed.", context.ModelState));
            }
        }
    }
}