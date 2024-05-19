using Serilog;

namespace Cpb.Api.AspNetCore;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var exceptionId = Guid.NewGuid();
        Log.Error(exception, "An unexpected error occurred. Related id: {0}", exceptionId);

        var error = new ApiError(
            $"Something happened in our side. This error already have been in the process of fixing. " +
            $"You can contact us with the id({exceptionId}) to know what's going on now.");
        
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(error);
    }
}

file class ApiError(string Message);