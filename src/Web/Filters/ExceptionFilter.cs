using System.Net;
using Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Filters;

public class ExceptionFilter : IAsyncExceptionFilter
{

    public Task OnExceptionAsync(ExceptionContext context)
    {
        var error = new ExceptionModel { Error = context.Exception.Message };
        var result = new JsonResult(error);
        switch (context.Exception)
        {
            case ResourceNotFoundException:
                result.StatusCode = (int) HttpStatusCode.NotFound;
                break;
            case ResourceExistsException:
                result.StatusCode = (int)HttpStatusCode.Conflict;
                break;
            default:
                result.StatusCode = (int) HttpStatusCode.InternalServerError;
                break;
        }
        context.Result = result;
        return Task.CompletedTask;
    }
}