using Microsoft.AspNetCore.Mvc;

namespace Xenial.Identity.Client;

public sealed class XenialNotFoundException : XenialApiException
{
    public ProblemDetails Details { get; private set; }
    public XenialNotFoundException(ProblemDetails details)
        => Details = details;
}

