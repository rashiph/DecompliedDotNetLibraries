namespace System.Security.Principal
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, ComVisible(true), HostProtection(SecurityAction.LinkDemand, SecurityInfrastructure=true)]
    public class WindowsPrincipal : IPrincipal
    {
        private WindowsIdentity m_identity;
        private string[] m_roles;
        private bool m_rolesLoaded;
        private Hashtable m_rolesTable;

        private WindowsPrincipal()
        {
        }

        public WindowsPrincipal(WindowsIdentity ntIdentity)
        {
            if (ntIdentity == null)
            {
                throw new ArgumentNullException("ntIdentity");
            }
            this.m_identity = ntIdentity;
        }

        public virtual bool IsInRole(int rid)
        {
            SecurityIdentifier sid = new SecurityIdentifier(IdentifierAuthority.NTAuthority, new int[] { 0x20, rid });
            return this.IsInRole(sid);
        }

        [SecuritySafeCritical, ComVisible(false)]
        public virtual bool IsInRole(SecurityIdentifier sid)
        {
            if (sid == null)
            {
                throw new ArgumentNullException("sid");
            }
            if (this.m_identity.TokenHandle.IsInvalid)
            {
                return false;
            }
            SafeTokenHandle invalidHandle = SafeTokenHandle.InvalidHandle;
            if ((this.m_identity.ImpersonationLevel == TokenImpersonationLevel.None) && !Win32Native.DuplicateTokenEx(this.m_identity.TokenHandle, (uint) 8, IntPtr.Zero, (uint) 2, (uint) 2, ref invalidHandle))
            {
                throw new SecurityException(Win32Native.GetMessage(Marshal.GetLastWin32Error()));
            }
            bool isMember = false;
            if (!Win32Native.CheckTokenMembership((this.m_identity.ImpersonationLevel != TokenImpersonationLevel.None) ? this.m_identity.TokenHandle : invalidHandle, sid.BinaryForm, ref isMember))
            {
                throw new SecurityException(Win32Native.GetMessage(Marshal.GetLastWin32Error()));
            }
            invalidHandle.Dispose();
            return isMember;
        }

        public virtual bool IsInRole(WindowsBuiltInRole role)
        {
            if ((role < WindowsBuiltInRole.Administrator) || (role > WindowsBuiltInRole.Replicator))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { (int) role }), "role");
            }
            return this.IsInRole((int) role);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, ControlPrincipal=true)]
        public virtual bool IsInRole(string role)
        {
            if ((role == null) || (role.Length == 0))
            {
                return false;
            }
            NTAccount account = new NTAccount(role);
            SecurityIdentifier sid = NTAccount.Translate(new IdentityReferenceCollection(1) { account }, typeof(SecurityIdentifier), 0)[0] as SecurityIdentifier;
            if (sid == null)
            {
                return false;
            }
            return this.IsInRole(sid);
        }

        public virtual IIdentity Identity
        {
            get
            {
                return this.m_identity;
            }
        }
    }
}

