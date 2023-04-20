using System.Net;

namespace d_lama_service.Models
{
    /// <summary>
    /// Exceptions for modeling REST StatusCodes.
    /// </summary>
    public class RESTException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }

        public RESTException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
