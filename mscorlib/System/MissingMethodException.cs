namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class MissingMethodException : MissingMemberException, ISerializable
    {
        public MissingMethodException() : base(Environment.GetResourceString("Arg_MissingMethodException"))
        {
            base.SetErrorCode(-2146233069);
        }

        public MissingMethodException(string message) : base(message)
        {
            base.SetErrorCode(-2146233069);
        }

        [SecuritySafeCritical]
        protected MissingMethodException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MissingMethodException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146233069);
        }

        public MissingMethodException(string className, string methodName)
        {
            base.ClassName = className;
            base.MemberName = methodName;
        }

        private MissingMethodException(string className, string methodName, byte[] signature)
        {
            base.ClassName = className;
            base.MemberName = methodName;
            base.Signature = signature;
        }

        public override string Message
        {
            [SecuritySafeCritical]
            get
            {
                if (base.ClassName == null)
                {
                    return base.Message;
                }
                return Environment.GetResourceString("MissingMethod_Name", new object[] { base.ClassName + "." + base.MemberName + ((base.Signature != null) ? (" " + MissingMemberException.FormatSignature(base.Signature)) : "") });
            }
        }
    }
}

