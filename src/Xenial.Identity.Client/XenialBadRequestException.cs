using Microsoft.AspNetCore.Mvc;

namespace Xenial.Identity.Client;

public sealed class XenialBadRequestException : XenialApiException
{
    public ProblemDetails Details { get; private set; }
    public XenialBadRequestException(ProblemDetails details)
        => Details = details;
}

