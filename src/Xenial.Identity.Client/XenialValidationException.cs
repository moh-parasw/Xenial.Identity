using Microsoft.AspNetCore.Mvc;

namespace Xenial.Identity.Client;

public sealed class XenialValidationException : XenialApiException
{
    public ValidationProblemDetails Details { get; private set; }
    public XenialValidationException(ValidationProblemDetails details)
        => Details = details;
}

