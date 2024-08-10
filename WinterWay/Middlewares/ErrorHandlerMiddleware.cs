using WinterWay.Models.DTOs.Error;
using WinterWay.Enums;

namespace WinterWay.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == 404 || context.Response.StatusCode == 405)
            {
                await context.Response.WriteAsJsonAsync(new ApiError(InnerErrors.PageNotFound, "Page not found"));
            }
            else if (context.Response.StatusCode == 415)
            {
                await context.Response.WriteAsJsonAsync(new ApiError(InnerErrors.UnsupportedMediaType, "Invalid form type"));
            }
            // Uncomment when .NET 10
            //else if (context.Response.StatusCode == 401) 
            //{
            //    await context.Response.WriteAsJsonAsync(new ApiError(InnerErrors.NotAuthorized, "User is not authorized"));
            //}
        }
    }
}
