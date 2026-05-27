using System.Net;

namespace PortRiskMonitor.Application.Exceptions;

// 409 Confict
public class ConflictException : CustomException
{
    public ConflictException(string message) : base(message, HttpStatusCode.Conflict) { }
}

