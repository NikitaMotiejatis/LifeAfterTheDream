using System.Net;

namespace PortRiskMonitor.Application.Exceptions;

public abstract class CustomException : Exception
{
    public HttpStatusCode StatusCode { get; }

    protected CustomException(string message, HttpStatusCode statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}
