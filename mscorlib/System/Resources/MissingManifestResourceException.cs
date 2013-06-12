namespace System.Resources
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class MissingManifestResourceException : SystemException
    {
        public MissingManifestResourceException() : base(Environment.GetResourceString("Arg_MissingManifestResourceException"))
        {
            base.SetErrorCode(-2146233038);
        }

        public MissingManifestResourceException(string message) : base(message)
        {
            base.SetErrorCode(-2146233038);
        }

        [SecuritySafeCritical]
        protected MissingManifestResourceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MissingManifestResourceException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146233038);
        }
    }
}

