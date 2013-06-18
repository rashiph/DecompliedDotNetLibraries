namespace Microsoft.InfoCards
{
    using System;
    using System.Runtime.Serialization;

    internal class UserCancelledException : InfoCardBaseException
    {
        private const int HRESULT = -1073413869;

        public UserCancelledException() : base(-1073413869)
        {
        }

        public UserCancelledException(string message) : base(-1073413869, message)
        {
        }

        protected UserCancelledException(SerializationInfo si, StreamingContext sc) : base(-1073413869, si, sc)
        {
        }

        public UserCancelledException(string message, Exception inner) : base(-1073413869, message, inner)
        {
        }
    }
}

