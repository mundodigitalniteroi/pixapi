using System;

namespace NegocioCielo
{
    public class CancellationTokenException : Exception
    {
        private const string _msg = "Timeout out.";

        public CancellationTokenException()
            : base(_msg) { }

        public CancellationTokenException(Exception innerException)
           : base(_msg, innerException) { }
    }
}
