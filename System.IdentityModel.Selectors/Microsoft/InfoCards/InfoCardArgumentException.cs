namespace Microsoft.InfoCards
{
    using System;
    using System.Runtime.Serialization;

    internal class InfoCardArgumentException : InfoCardBaseException
    {
        private const int HRESULT = -1073413883;

        public InfoCardArgumentException() : base(-1073413883)
        {
        }

        public InfoCardArgumentException(string message) : base(-1073413883, message)
        {
        }

        protected InfoCardArgumentException(SerializationInfo si, StreamingContext sc) : base(-1073413883, si, sc)
        {
        }

        public InfoCardArgumentException(string message, Exception inner) : base(-1073413883, message, inner)
        {
        }
    }
}

