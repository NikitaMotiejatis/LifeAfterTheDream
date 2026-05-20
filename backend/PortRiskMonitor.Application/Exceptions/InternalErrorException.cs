using System.Net;

namespace PortRiskMonitor.Application.Exceptions;

// 500 Internal Server Error (For explicit application failures)
public class InternalErrorException : CustomException
{
    public InternalErrorException(string message) : base(message, HttpStatusCode.InternalServerError) { }
}
