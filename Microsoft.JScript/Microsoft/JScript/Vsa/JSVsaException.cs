namespace Microsoft.JScript.Vsa
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable, Guid("064C47AC-C9DF-4FCD-9009-E9299D620018"), Obsolete("Use of this type is not recommended because it is being deprecated in Visual Studio 2005; there will be no replacement for this feature. Please see the ICodeCompiler documentation for additional help."), ComVisible(true), PermissionSet(SecurityAction.Demand, Name="FullTrust")]
    public sealed class JSVsaException : ExternalException
    {
        public JSVsaException()
        {
        }

        public JSVsaException(JSVsaError error) : base(string.Empty, (int) error)
        {
        }

        public JSVsaException(string message) : base(message)
        {
        }

        public JSVsaException(JSVsaError error, string message) : base(message, (int) error)
        {
        }

        public JSVsaException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            base.HResult = (int) info.GetValue("VsaException_HResult", typeof(int));
            this.HelpLink = (string) info.GetValue("VsaException_HelpLink", typeof(string));
            this.Source = (string) info.GetValue("VsaException_Source", typeof(string));
        }

        public JSVsaException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public JSVsaException(JSVsaError error, string message, Exception innerException) : base(message, innerException)
        {
            base.HResult = (int) error;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true), PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("VsaException_HResult", base.HResult);
            info.AddValue("VsaException_HelpLink", this.HelpLink);
            info.AddValue("VsaException_Source", this.Source);
        }

        public override string ToString()
        {
            if ((this.Message != null) && ("" != this.Message))
            {
                return ("Microsoft.JScript.Vsa.JSVsaException: " + Enum.GetName(((JSVsaError) base.HResult).GetType(), (JSVsaError) base.HResult) + " (0x" + string.Format(CultureInfo.InvariantCulture, "{0,8:X}", new object[] { base.HResult }) + "): " + this.Message);
            }
            return ("Microsoft.JScript.Vsa.JSVsaException: " + Enum.GetName(((JSVsaError) base.HResult).GetType(), (JSVsaError) base.HResult) + " (0x" + string.Format(CultureInfo.InvariantCulture, "{0,8:X}", new object[] { base.HResult }) + ").");
        }

        public JSVsaError ErrorCode
        {
            get
            {
                return (JSVsaError) base.HResult;
            }
        }
    }
}

