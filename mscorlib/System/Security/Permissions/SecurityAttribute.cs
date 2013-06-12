namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false), ComVisible(true)]
    public abstract class SecurityAttribute : Attribute
    {
        internal SecurityAction m_action;
        internal bool m_unrestricted;

        protected SecurityAttribute(SecurityAction action)
        {
            this.m_action = action;
        }

        public abstract IPermission CreatePermission();
        [SecurityCritical]
        internal static IntPtr FindSecurityAttributeTypeHandle(string typeName)
        {
            PermissionSet.s_fullTrust.Assert();
            Type type = Type.GetType(typeName, false, false);
            if (type == null)
            {
                return IntPtr.Zero;
            }
            return type.TypeHandle.Value;
        }

        public SecurityAction Action
        {
            get
            {
                return this.m_action;
            }
            set
            {
                this.m_action = value;
            }
        }

        public bool Unrestricted
        {
            get
            {
                return this.m_unrestricted;
            }
            set
            {
                this.m_unrestricted = value;
            }
        }
    }
}

