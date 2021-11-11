using System.Linq;
using AVStack.IdentityServer.WebApi.Models;
using AVStack.IdentityServer.WebApi.Models.System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AVStack.IdentityServer.WebApi.Controllers.Validators
{
    public class ValidateModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState.Values.Where(v => v.Errors.Count > 0)
                    .SelectMany(v => v.Errors)
                    .Select(v => v.ErrorMessage)
                    .ToList();

                var responseObj = new Response()
                {
                    Succeeded = false,
                    StatusCode = 400,
                    Errors = errors,
                };

                context.Result = new JsonResult(responseObj)
                {
                    StatusCode = 400
                };
            }
        }
    }
}