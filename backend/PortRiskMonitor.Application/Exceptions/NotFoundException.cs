using System.Net;

namespace PortRiskMonitor.Application.Exceptions;

// 404 Not Found
public class NotFoundException : CustomException
{
    public NotFoundException(string message) : base(message, HttpStatusCode.NotFound) { }
}
