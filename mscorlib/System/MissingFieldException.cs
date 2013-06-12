namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class MissingFieldException : MissingMemberException, ISerializable
    {
        public MissingFieldException() : base(Environment.GetResourceString("Arg_MissingFieldException"))
        {
            base.SetErrorCode(-2146233071);
        }

        public MissingFieldException(string message) : base(message)
        {
            base.SetErrorCode(-2146233071);
        }

        [SecuritySafeCritical]
        protected MissingFieldException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MissingFieldException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146233071);
        }

        public MissingFieldException(string className, string fieldName)
        {
            base.ClassName = className;
            base.MemberName = fieldName;
        }

        private MissingFieldException(string className, string fieldName, byte[] signature)
        {
            base.ClassName = className;
            base.MemberName = fieldName;
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
                return Environment.GetResourceString("MissingField_Name", new object[] { ((base.Signature != null) ? (MissingMemberException.FormatSignature(base.Signature) + " ") : "") + base.ClassName + "." + base.MemberName });
            }
        }
    }
}

