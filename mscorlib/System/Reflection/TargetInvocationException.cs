namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable, ComVisible(true)]
    public sealed class TargetInvocationException : ApplicationException
    {
        private TargetInvocationException() : base(Environment.GetResourceString("Arg_TargetInvocationException"))
        {
            base.SetErrorCode(-2146232828);
        }

        public TargetInvocationException(Exception inner) : base(Environment.GetResourceString("Arg_TargetInvocationException"), inner)
        {
            base.SetErrorCode(-2146232828);
        }

        private TargetInvocationException(string message) : base(message)
        {
            base.SetErrorCode(-2146232828);
        }

        internal TargetInvocationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public TargetInvocationException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146232828);
        }
    }
}

