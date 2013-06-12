namespace System
{
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class MissingMemberException : MemberAccessException, ISerializable
    {
        protected string ClassName;
        protected string MemberName;
        protected byte[] Signature;

        public MissingMemberException() : base(Environment.GetResourceString("Arg_MissingMemberException"))
        {
            base.SetErrorCode(-2146233070);
        }

        public MissingMemberException(string message) : base(message)
        {
            base.SetErrorCode(-2146233070);
        }

        [SecuritySafeCritical]
        protected MissingMemberException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.ClassName = info.GetString("MMClassName");
            this.MemberName = info.GetString("MMMemberName");
            this.Signature = (byte[]) info.GetValue("MMSignature", typeof(byte[]));
        }

        public MissingMemberException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146233070);
        }

        public MissingMemberException(string className, string memberName)
        {
            this.ClassName = className;
            this.MemberName = memberName;
        }

        private MissingMemberException(string className, string memberName, byte[] signature)
        {
            this.ClassName = className;
            this.MemberName = memberName;
            this.Signature = signature;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern string FormatSignature(byte[] signature);
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("MMClassName", this.ClassName, typeof(string));
            info.AddValue("MMMemberName", this.MemberName, typeof(string));
            info.AddValue("MMSignature", this.Signature, typeof(byte[]));
        }

        public override string Message
        {
            [SecuritySafeCritical]
            get
            {
                if (this.ClassName == null)
                {
                    return base.Message;
                }
                return Environment.GetResourceString("MissingMember_Name", new object[] { this.ClassName + "." + this.MemberName + ((this.Signature != null) ? (" " + FormatSignature(this.Signature)) : "") });
            }
        }
    }
}

