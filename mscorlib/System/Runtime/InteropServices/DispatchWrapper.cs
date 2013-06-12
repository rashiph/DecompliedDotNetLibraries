namespace System.Runtime.InteropServices
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, ComVisible(true)]
    public sealed class DispatchWrapper
    {
        private object m_WrappedObject;

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public DispatchWrapper(object obj)
        {
            if (obj != null)
            {
                Marshal.Release(Marshal.GetIDispatchForObject(obj));
            }
            this.m_WrappedObject = obj;
        }

        public object WrappedObject
        {
            get
            {
                return this.m_WrappedObject;
            }
        }
    }
}

