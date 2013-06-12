namespace System.Threading
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class ThreadStartException : SystemException
    {
        private ThreadStartException() : base(Environment.GetResourceString("Arg_ThreadStartException"))
        {
            base.SetErrorCode(-2146233051);
        }

        private ThreadStartException(Exception reason) : base(Environment.GetResourceString("Arg_ThreadStartException"), reason)
        {
            base.SetErrorCode(-2146233051);
        }

        internal ThreadStartException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

