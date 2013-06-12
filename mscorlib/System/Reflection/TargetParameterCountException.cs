namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable, ComVisible(true)]
    public sealed class TargetParameterCountException : ApplicationException
    {
        public TargetParameterCountException() : base(Environment.GetResourceString("Arg_TargetParameterCountException"))
        {
            base.SetErrorCode(-2147352562);
        }

        public TargetParameterCountException(string message) : base(message)
        {
            base.SetErrorCode(-2147352562);
        }

        internal TargetParameterCountException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public TargetParameterCountException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2147352562);
        }
    }
}

