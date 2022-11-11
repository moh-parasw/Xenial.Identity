namespace Xenial.Identity.Client;

public abstract class XenialApiException : Exception
{
    public XenialApiException()
    {

    }

    public XenialApiException(string message) : base(message)
    {

    }

    public XenialApiException(Exception innerException) : base(null, innerException)
    {

    }
}

