namespace System.DirectoryServices.Protocols
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class ConnectionHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal ConnectionHandle() : base(true)
        {
            base.SetHandle(Wldap32.ldap_init(null, 0x185));
            if (base.handle == IntPtr.Zero)
            {
                int errorCode = Wldap32.LdapGetLastError();
                if (Utility.IsLdapError((LdapError) errorCode))
                {
                    string message = LdapErrorMappings.MapResultCode(errorCode);
                    throw new LdapException(errorCode, message);
                }
                throw new LdapException(errorCode);
            }
        }

        protected override bool ReleaseHandle()
        {
            Wldap32.ldap_unbind(base.handle);
            return true;
        }
    }
}

