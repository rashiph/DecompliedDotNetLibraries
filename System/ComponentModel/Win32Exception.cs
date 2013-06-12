namespace System.ComponentModel
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    [Serializable, SuppressUnmanagedCodeSecurity, HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class Win32Exception : ExternalException, ISerializable
    {
        private readonly int nativeErrorCode;

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public Win32Exception() : this(Marshal.GetLastWin32Error())
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public Win32Exception(int error) : this(error, GetErrorMessage(error))
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public Win32Exception(string message) : this(Marshal.GetLastWin32Error(), message)
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public Win32Exception(int error, string message) : base(message)
        {
            this.nativeErrorCode = error;
        }

        protected Win32Exception(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            IntSecurity.UnmanagedCode.Demand();
            this.nativeErrorCode = info.GetInt32("NativeErrorCode");
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public Win32Exception(string message, Exception innerException) : base(message, innerException)
        {
            this.nativeErrorCode = Marshal.GetLastWin32Error();
        }

        private static string GetErrorMessage(int error)
        {
            StringBuilder lpBuffer = new StringBuilder(0x100);
            if (Microsoft.Win32.SafeNativeMethods.FormatMessage(0x3200, Microsoft.Win32.NativeMethods.NullHandleRef, error, 0, lpBuffer, lpBuffer.Capacity + 1, IntPtr.Zero) == 0)
            {
                return ("Unknown error (0x" + Convert.ToString(error, 0x10) + ")");
            }
            int length = lpBuffer.Length;
            while (length > 0)
            {
                char ch = lpBuffer[length - 1];
                if ((ch > ' ') && (ch != '.'))
                {
                    break;
                }
                length--;
            }
            return lpBuffer.ToString(0, length);
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("NativeErrorCode", this.nativeErrorCode);
            base.GetObjectData(info, context);
        }

        public int NativeErrorCode
        {
            get
            {
                return this.nativeErrorCode;
            }
        }
    }
}

