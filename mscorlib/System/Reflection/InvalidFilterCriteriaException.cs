namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class InvalidFilterCriteriaException : ApplicationException
    {
        public InvalidFilterCriteriaException() : base(Environment.GetResourceString("Arg_InvalidFilterCriteriaException"))
        {
            base.SetErrorCode(-2146232831);
        }

        public InvalidFilterCriteriaException(string message) : base(message)
        {
            base.SetErrorCode(-2146232831);
        }

        [SecuritySafeCritical]
        protected InvalidFilterCriteriaException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidFilterCriteriaException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146232831);
        }
    }
}

