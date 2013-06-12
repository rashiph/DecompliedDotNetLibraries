namespace System.Runtime.InteropServices
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, ComVisible(true)]
    public sealed class BStrWrapper
    {
        private string m_WrappedObject;

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public BStrWrapper(object value)
        {
            this.m_WrappedObject = (string) value;
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public BStrWrapper(string value)
        {
            this.m_WrappedObject = value;
        }

        public string WrappedObject
        {
            get
            {
                return this.m_WrappedObject;
            }
        }
    }
}

