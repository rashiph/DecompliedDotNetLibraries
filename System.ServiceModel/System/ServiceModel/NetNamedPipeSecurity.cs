namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Net.Security;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;

    public sealed class NetNamedPipeSecurity
    {
        internal const NetNamedPipeSecurityMode DefaultMode = NetNamedPipeSecurityMode.Transport;
        private NetNamedPipeSecurityMode mode;
        private NamedPipeTransportSecurity transport;

        public NetNamedPipeSecurity()
        {
            this.transport = new NamedPipeTransportSecurity();
            this.mode = NetNamedPipeSecurityMode.Transport;
        }

        private NetNamedPipeSecurity(NetNamedPipeSecurityMode mode, NamedPipeTransportSecurity transport)
        {
            this.transport = new NamedPipeTransportSecurity();
            this.mode = mode;
            this.transport = (transport == null) ? new NamedPipeTransportSecurity() : transport;
        }

        internal WindowsStreamSecurityBindingElement CreateTransportSecurity()
        {
            if (this.mode == NetNamedPipeSecurityMode.Transport)
            {
                return this.transport.CreateTransportProtectionAndAuthentication();
            }
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTransport()
        {
            if (this.transport.ProtectionLevel == ProtectionLevel.EncryptAndSign)
            {
                return false;
            }
            return true;
        }

        internal static bool TryCreate(WindowsStreamSecurityBindingElement wssbe, NetNamedPipeSecurityMode mode, out NetNamedPipeSecurity security)
        {
            security = null;
            NamedPipeTransportSecurity transportSecurity = new NamedPipeTransportSecurity();
            if ((mode == NetNamedPipeSecurityMode.Transport) && !NamedPipeTransportSecurity.IsTransportProtectionAndAuthentication(wssbe, transportSecurity))
            {
                return false;
            }
            security = new NetNamedPipeSecurity(mode, transportSecurity);
            return true;
        }

        [DefaultValue(1)]
        public NetNamedPipeSecurityMode Mode
        {
            get
            {
                return this.mode;
            }
            set
            {
                if (!NetNamedPipeSecurityModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.mode = value;
            }
        }

        public NamedPipeTransportSecurity Transport
        {
            get
            {
                return this.transport;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.transport = value;
            }
        }
    }
}

