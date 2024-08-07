using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WinterWay.Models.DTOs.Error;
using WinterWay.Enums;

namespace WinterWay.Filters
{
    public class ValidateModelFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var error = new ApiError(InnerErrors.InvalidForm, "Invalid form", context.ModelState.Keys.ToList());

                context.Result = new BadRequestObjectResult(error);
            }
        }
    }
}
