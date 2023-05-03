using d_lama_service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace d_lama_service.Middleware
{
    /// <summary>
    /// OwnerExceptionFilter handles RESTExceptions.
    /// </summary>
    public class RESTExceptionFilter : IExceptionFilter
    {
        /// <summary>
        /// Forwards an exception to the correct StatusCode.
        /// </summary>
        /// <param name="context"> The exception context. </param>
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is RESTException) 
            {
                var restException = (RESTException)context.Exception;
                switch (restException.StatusCode) 
                {
                    case HttpStatusCode.Unauthorized:
                        context.Result = new UnauthorizedObjectResult(restException.Message);
                        break;
                    case HttpStatusCode.NotFound:
                        context.Result = new NotFoundObjectResult(restException.Message);
                        break;
                }
            }
        }
    }
}
