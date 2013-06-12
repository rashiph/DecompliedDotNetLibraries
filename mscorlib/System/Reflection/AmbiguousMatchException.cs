namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable, ComVisible(true)]
    public sealed class AmbiguousMatchException : SystemException
    {
        public AmbiguousMatchException() : base(Environment.GetResourceString("RFLCT.Ambiguous"))
        {
            base.SetErrorCode(-2147475171);
        }

        public AmbiguousMatchException(string message) : base(message)
        {
            base.SetErrorCode(-2147475171);
        }

        internal AmbiguousMatchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public AmbiguousMatchException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2147475171);
        }
    }
}

