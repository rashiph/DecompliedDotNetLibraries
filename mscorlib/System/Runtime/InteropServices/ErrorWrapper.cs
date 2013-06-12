namespace System.Runtime.InteropServices
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, ComVisible(true)]
    public sealed class ErrorWrapper
    {
        private int m_ErrorCode;

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public ErrorWrapper(Exception e)
        {
            this.m_ErrorCode = Marshal.GetHRForException(e);
        }

        public ErrorWrapper(int errorCode)
        {
            this.m_ErrorCode = errorCode;
        }

        public ErrorWrapper(object errorCode)
        {
            if (!(errorCode is int))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeInt32"), "errorCode");
            }
            this.m_ErrorCode = (int) errorCode;
        }

        public int ErrorCode
        {
            get
            {
                return this.m_ErrorCode;
            }
        }
    }
}

