namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public sealed class TypeInitializationException : SystemException
    {
        private string _typeName;

        private TypeInitializationException() : base(Environment.GetResourceString("TypeInitialization_Default"))
        {
            base.SetErrorCode(-2146233036);
        }

        private TypeInitializationException(string message) : base(message)
        {
            base.SetErrorCode(-2146233036);
        }

        internal TypeInitializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._typeName = info.GetString("TypeName");
        }

        public TypeInitializationException(string fullTypeName, Exception innerException) : base(Environment.GetResourceString("TypeInitialization_Type", new object[] { fullTypeName }), innerException)
        {
            this._typeName = fullTypeName;
            base.SetErrorCode(-2146233036);
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("TypeName", this.TypeName, typeof(string));
        }

        public string TypeName
        {
            get
            {
                if (this._typeName == null)
                {
                    return string.Empty;
                }
                return this._typeName;
            }
        }
    }
}

