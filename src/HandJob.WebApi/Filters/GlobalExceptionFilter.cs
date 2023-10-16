using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HandJob.Domain.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HandJob.WebApi.Filters;

public class GlobalExceptionFilter : IAsyncExceptionFilter
{
    public Task OnExceptionAsync(ExceptionContext context)
    {
        OkObjectResult result = new OkObjectResult(new ResponseBox()
        {
            Code = 500,
            Message = "服务器异常.",
            Data = context.Exception.Message
        });

        context.Result = result;
        context.ExceptionHandled = true;

        return Task.FromResult(result);
    }
}