using FlotaVehicular.Domain.DTO.PatternResult;
using FlotaVehicular.WebAPI.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FlotaVehicular.WebAPI.Filters;

// Guarda los Result de error para que la bitácora incluya su detalle.
public sealed class ResultLoggingFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(
        ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult { Value: BaseResult result } && !result.IsSuccess)
            context.HttpContext.Items[RequestLoggingMiddleware.ErrorItemKey] = result;

        await next();
    }
}
