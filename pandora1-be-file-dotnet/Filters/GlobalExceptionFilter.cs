using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using pandora1_be_file_dotnet.Helpers;
using pandora1_be_file_dotnet.Models.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pandora1_be_file_dotnet.Filters
{
    public class GlobalExceptionsFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.ExceptionHandled == false)
            {
                var json = new ApiResponse();
                if (context.Exception.GetType() == typeof(ServiceException))
                {
                    var ex = (ServiceException)context.Exception;
                    json.Code = ex.Descriptor.errorCode;
                    json.Message = ex.Descriptor.errorMessage;
                    context.Result = new JsonResult(json);
                }
                else
                {
                    json.Code = 500;
                    json.Message = context.Exception.Message;//错误信息
                    context.Result = new JsonResult(json);
                }
            }
            context.ExceptionHandled = true;

        }
    }
}
