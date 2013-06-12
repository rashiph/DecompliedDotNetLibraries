namespace System.ComponentModel.Design
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable, PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), HostProtection(SecurityAction.LinkDemand, SharedState=true), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class CheckoutException : ExternalException
    {
        public static readonly CheckoutException Canceled = new CheckoutException(SR.GetString("CHECKOUTCanceled"), -2147467260);

        public CheckoutException()
        {
        }

        public CheckoutException(string message) : base(message)
        {
        }

        protected CheckoutException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public CheckoutException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public CheckoutException(string message, int errorCode) : base(message, errorCode)
        {
        }
    }
}

