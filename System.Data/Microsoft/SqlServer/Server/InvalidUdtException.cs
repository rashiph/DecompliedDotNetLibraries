namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public sealed class InvalidUdtException : SystemException
    {
        internal InvalidUdtException()
        {
            base.HResult = -2146232009;
        }

        internal InvalidUdtException(string message) : base(message)
        {
            base.HResult = -2146232009;
        }

        private InvalidUdtException(SerializationInfo si, StreamingContext sc) : base(si, sc)
        {
        }

        internal InvalidUdtException(string message, Exception innerException) : base(message, innerException)
        {
            base.HResult = -2146232009;
        }

        internal static InvalidUdtException Create(Type udtType, string resourceReason)
        {
            string str2 = Res.GetString(resourceReason);
            InvalidUdtException e = new InvalidUdtException(Res.GetString("SqlUdt_InvalidUdtMessage", new object[] { udtType.FullName, str2 }));
            ADP.TraceExceptionAsReturnValue(e);
            return e;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo si, StreamingContext context)
        {
            base.GetObjectData(si, context);
        }
    }
}

