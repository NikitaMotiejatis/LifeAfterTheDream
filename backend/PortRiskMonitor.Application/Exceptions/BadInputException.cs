using System.Net;

namespace PortRiskMonitor.Application.Exceptions;

// 400 Bad Request
public class BadInputException : CustomException
{
    public BadInputException(string message) : base(message, HttpStatusCode.BadRequest) { }
}
