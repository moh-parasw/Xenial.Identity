namespace Xenial.Identity.Client;

public sealed class XenialUnknownApiException : XenialApiException
{
    public XenialUnknownApiException(Exception innerException) : base(innerException)
    {

    }
}

