namespace System.Net
{
    using System;
    using System.Globalization;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false)]
    public sealed class SocketPermissionAttribute : CodeAccessSecurityAttribute
    {
        private string m_access;
        private string m_host;
        private string m_port;
        private string m_transport;
        private const string strAccept = "Accept";
        private const string strAccess = "Access";
        private const string strConnect = "Connect";
        private const string strHost = "Host";
        private const string strPort = "Port";
        private const string strTransport = "Transport";

        public SocketPermissionAttribute(SecurityAction action) : base(action)
        {
        }

        public override IPermission CreatePermission()
        {
            SocketPermission perm = null;
            if (base.Unrestricted)
            {
                return new SocketPermission(PermissionState.Unrestricted);
            }
            perm = new SocketPermission(PermissionState.None);
            if (this.m_access == null)
            {
                throw new ArgumentException(SR.GetString("net_perm_attrib_count", new object[] { "Access" }));
            }
            if (this.m_host == null)
            {
                throw new ArgumentException(SR.GetString("net_perm_attrib_count", new object[] { "Host" }));
            }
            if (this.m_transport == null)
            {
                throw new ArgumentException(SR.GetString("net_perm_attrib_count", new object[] { "Transport" }));
            }
            if (this.m_port == null)
            {
                throw new ArgumentException(SR.GetString("net_perm_attrib_count", new object[] { "Port" }));
            }
            this.ParseAddPermissions(perm);
            return perm;
        }

        private void ParseAddPermissions(SocketPermission perm)
        {
            NetworkAccess connect;
            TransportType type;
            int num;
            if (string.Compare(this.m_access, "Connect", StringComparison.OrdinalIgnoreCase) == 0)
            {
                connect = NetworkAccess.Connect;
            }
            else
            {
                if (string.Compare(this.m_access, "Accept", StringComparison.OrdinalIgnoreCase) != 0)
                {
                    throw new ArgumentException(SR.GetString("net_perm_invalid_val", new object[] { "Access", this.m_access }));
                }
                connect = NetworkAccess.Accept;
            }
            try
            {
                type = (TransportType) System.Enum.Parse(typeof(TransportType), this.m_transport, true);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                throw new ArgumentException(SR.GetString("net_perm_invalid_val", new object[] { "Transport", this.m_transport }), exception);
            }
            if (string.Compare(this.m_port, "All", StringComparison.OrdinalIgnoreCase) == 0)
            {
                this.m_port = "-1";
            }
            try
            {
                num = int.Parse(this.m_port, NumberFormatInfo.InvariantInfo);
            }
            catch (Exception exception2)
            {
                if (((exception2 is ThreadAbortException) || (exception2 is StackOverflowException)) || (exception2 is OutOfMemoryException))
                {
                    throw;
                }
                throw new ArgumentException(SR.GetString("net_perm_invalid_val", new object[] { "Port", this.m_port }), exception2);
            }
            if (!ValidationHelper.ValidateTcpPort(num) && (num != -1))
            {
                throw new ArgumentOutOfRangeException("port", num, SR.GetString("net_perm_invalid_val", new object[] { "Port", this.m_port }));
            }
            perm.AddPermission(connect, type, this.m_host, num);
        }

        public string Access
        {
            get
            {
                return this.m_access;
            }
            set
            {
                if (this.m_access != null)
                {
                    throw new ArgumentException(SR.GetString("net_perm_attrib_multi", new object[] { "Access", value }), "value");
                }
                this.m_access = value;
            }
        }

        public string Host
        {
            get
            {
                return this.m_host;
            }
            set
            {
                if (this.m_host != null)
                {
                    throw new ArgumentException(SR.GetString("net_perm_attrib_multi", new object[] { "Host", value }), "value");
                }
                this.m_host = value;
            }
        }

        public string Port
        {
            get
            {
                return this.m_port;
            }
            set
            {
                if (this.m_port != null)
                {
                    throw new ArgumentException(SR.GetString("net_perm_attrib_multi", new object[] { "Port", value }), "value");
                }
                this.m_port = value;
            }
        }

        public string Transport
        {
            get
            {
                return this.m_transport;
            }
            set
            {
                if (this.m_transport != null)
                {
                    throw new ArgumentException(SR.GetString("net_perm_attrib_multi", new object[] { "Transport", value }), "value");
                }
                this.m_transport = value;
            }
        }
    }
}

