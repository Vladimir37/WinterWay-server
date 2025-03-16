﻿using WinterWay.Enums;
using WinterWay.Models.DTOs.Responses.Shared;

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
                await context.Response.WriteAsJsonAsync(new ApiErrorDTO(InternalError.PageNotFound, "Page not found"));
            }
            else if (context.Response.StatusCode == 415)
            {
                await context.Response.WriteAsJsonAsync(new ApiErrorDTO(InternalError.UnsupportedMediaType, "Invalid form type"));
            }
            else if (context.Response.StatusCode >= 400) 
            {
                await context.Response.WriteAsJsonAsync(new ApiErrorDTO(InternalError.Other, "Other server error"));
            }
            // Uncomment when .NET 10
            // else if (context.Response.StatusCode == 401) 
            // {
            //     await context.Response.WriteAsJsonAsync(new ApiError(InternalError.NotAuthorized, "User is not authorized"));
            // }
        }
    }
}
