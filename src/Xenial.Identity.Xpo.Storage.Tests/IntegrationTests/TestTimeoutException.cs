using System;
using System.Runtime.Serialization;

namespace Xenial.Identity.Xpo.Storage.Tests.IntegrationTests
{
    [Serializable]
    internal class TestTimeoutException : Exception
    {
        public int Timeout { get; }

        public TestTimeoutException()
        {
        }

        public TestTimeoutException(int timeout) => Timeout = timeout;

        public TestTimeoutException(string message) : base(message)
        {
        }

        public TestTimeoutException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TestTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

    }
}
