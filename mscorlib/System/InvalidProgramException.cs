namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable, ComVisible(true)]
    public sealed class InvalidProgramException : SystemException
    {
        public InvalidProgramException() : base(Environment.GetResourceString("InvalidProgram_Default"))
        {
            base.SetErrorCode(-2146233030);
        }

        public InvalidProgramException(string message) : base(message)
        {
            base.SetErrorCode(-2146233030);
        }

        internal InvalidProgramException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidProgramException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146233030);
        }
    }
}

