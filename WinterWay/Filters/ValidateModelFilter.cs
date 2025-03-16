using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WinterWay.Enums;
using WinterWay.Models.DTOs.Responses.Shared;

namespace WinterWay.Filters
{
    public class ValidateModelFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var error = new ApiErrorDTO(InternalError.InvalidForm, "Invalid form", context.ModelState.Keys.ToList());

                context.Result = new BadRequestObjectResult(error);
            }
        }
    }
}
