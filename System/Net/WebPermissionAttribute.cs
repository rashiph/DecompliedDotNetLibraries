namespace System.Net
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false)]
    public sealed class WebPermissionAttribute : CodeAccessSecurityAttribute
    {
        private object m_accept;
        private object m_connect;

        public WebPermissionAttribute(SecurityAction action) : base(action)
        {
        }

        public override IPermission CreatePermission()
        {
            WebPermission permission = null;
            if (base.Unrestricted)
            {
                return new WebPermission(PermissionState.Unrestricted);
            }
            NetworkAccess access = 0;
            if (this.m_connect is bool)
            {
                if ((bool) this.m_connect)
                {
                    access |= NetworkAccess.Connect;
                }
                this.m_connect = null;
            }
            if (this.m_accept is bool)
            {
                if ((bool) this.m_accept)
                {
                    access |= NetworkAccess.Accept;
                }
                this.m_accept = null;
            }
            permission = new WebPermission(access);
            if (this.m_accept != null)
            {
                if (this.m_accept is DelayedRegex)
                {
                    permission.AddAsPattern(NetworkAccess.Accept, (DelayedRegex) this.m_accept);
                }
                else
                {
                    permission.AddPermission(NetworkAccess.Accept, (string) this.m_accept);
                }
            }
            if (this.m_connect != null)
            {
                if (this.m_connect is DelayedRegex)
                {
                    permission.AddAsPattern(NetworkAccess.Connect, (DelayedRegex) this.m_connect);
                    return permission;
                }
                permission.AddPermission(NetworkAccess.Connect, (string) this.m_connect);
            }
            return permission;
        }

        public string Accept
        {
            get
            {
                return (this.m_accept as string);
            }
            set
            {
                if (this.m_accept != null)
                {
                    throw new ArgumentException(SR.GetString("net_perm_attrib_multi", new object[] { "Accept", value }), "value");
                }
                this.m_accept = value;
            }
        }

        public string AcceptPattern
        {
            get
            {
                if (this.m_accept is DelayedRegex)
                {
                    return this.m_accept.ToString();
                }
                if ((this.m_accept is bool) && ((bool) this.m_accept))
                {
                    return ".*";
                }
                return null;
            }
            set
            {
                if (this.m_accept != null)
                {
                    throw new ArgumentException(SR.GetString("net_perm_attrib_multi", new object[] { "AcceptPattern", value }), "value");
                }
                if (value == ".*")
                {
                    this.m_accept = true;
                }
                else
                {
                    this.m_accept = new DelayedRegex(value);
                }
            }
        }

        public string Connect
        {
            get
            {
                return (this.m_connect as string);
            }
            set
            {
                if (this.m_connect != null)
                {
                    throw new ArgumentException(SR.GetString("net_perm_attrib_multi", new object[] { "Connect", value }), "value");
                }
                this.m_connect = value;
            }
        }

        public string ConnectPattern
        {
            get
            {
                if (this.m_connect is DelayedRegex)
                {
                    return this.m_connect.ToString();
                }
                if ((this.m_connect is bool) && ((bool) this.m_connect))
                {
                    return ".*";
                }
                return null;
            }
            set
            {
                if (this.m_connect != null)
                {
                    throw new ArgumentException(SR.GetString("net_perm_attrib_multi", new object[] { "ConnectPatern", value }), "value");
                }
                if (value == ".*")
                {
                    this.m_connect = true;
                }
                else
                {
                    this.m_connect = new DelayedRegex(value);
                }
            }
        }
    }
}

