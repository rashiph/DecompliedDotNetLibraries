namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;

    public sealed class NetTcpSecurity
    {
        internal const SecurityMode DefaultMode = SecurityMode.Transport;
        private MessageSecurityOverTcp messageSecurity;
        private SecurityMode mode;
        private TcpTransportSecurity transportSecurity;

        public NetTcpSecurity() : this(SecurityMode.Transport, new TcpTransportSecurity(), new MessageSecurityOverTcp())
        {
        }

        private NetTcpSecurity(SecurityMode mode, TcpTransportSecurity transportSecurity, MessageSecurityOverTcp messageSecurity)
        {
            this.mode = mode;
            this.transportSecurity = (transportSecurity == null) ? new TcpTransportSecurity() : transportSecurity;
            this.messageSecurity = (messageSecurity == null) ? new MessageSecurityOverTcp() : messageSecurity;
        }

        internal SecurityBindingElement CreateMessageSecurity(bool isReliableSessionEnabled)
        {
            if (this.mode == SecurityMode.Message)
            {
                return this.messageSecurity.CreateSecurityBindingElement(false, isReliableSessionEnabled, null);
            }
            if (this.mode == SecurityMode.TransportWithMessageCredential)
            {
                return this.messageSecurity.CreateSecurityBindingElement(true, isReliableSessionEnabled, this.CreateTransportSecurity());
            }
            return null;
        }

        internal BindingElement CreateTransportSecurity()
        {
            if (this.mode == SecurityMode.TransportWithMessageCredential)
            {
                return this.transportSecurity.CreateTransportProtectionOnly();
            }
            if (this.mode == SecurityMode.Transport)
            {
                return this.transportSecurity.CreateTransportProtectionAndAuthentication();
            }
            return null;
        }

        internal static UnifiedSecurityMode GetModeFromTransportSecurity(BindingElement transport)
        {
            if (transport == null)
            {
                return (UnifiedSecurityMode.Message | UnifiedSecurityMode.None);
            }
            return (UnifiedSecurityMode.TransportWithMessageCredential | UnifiedSecurityMode.Transport);
        }

        internal bool InternalShouldSerialize()
        {
            if ((this.Mode == SecurityMode.Transport) && !this.Transport.InternalShouldSerialize())
            {
                return this.Message.InternalShouldSerialize();
            }
            return true;
        }

        internal static bool SetTransportSecurity(BindingElement transport, SecurityMode mode, TcpTransportSecurity transportSecurity)
        {
            if (mode == SecurityMode.TransportWithMessageCredential)
            {
                return TcpTransportSecurity.SetTransportProtectionOnly(transport, transportSecurity);
            }
            if (mode == SecurityMode.Transport)
            {
                return TcpTransportSecurity.SetTransportProtectionAndAuthentication(transport, transportSecurity);
            }
            return (transport == null);
        }

        internal static bool TryCreate(SecurityBindingElement wsSecurity, SecurityMode mode, bool isReliableSessionEnabled, BindingElement transportSecurity, TcpTransportSecurity tcpTransportSecurity, out NetTcpSecurity security)
        {
            security = null;
            MessageSecurityOverTcp messageSecurity = null;
            if (mode == SecurityMode.Message)
            {
                if (!MessageSecurityOverTcp.TryCreate(wsSecurity, isReliableSessionEnabled, null, out messageSecurity))
                {
                    return false;
                }
            }
            else if ((mode == SecurityMode.TransportWithMessageCredential) && !MessageSecurityOverTcp.TryCreate(wsSecurity, isReliableSessionEnabled, transportSecurity, out messageSecurity))
            {
                return false;
            }
            security = new NetTcpSecurity(mode, tcpTransportSecurity, messageSecurity);
            return SecurityElementBase.AreBindingsMatching(security.CreateMessageSecurity(isReliableSessionEnabled), wsSecurity, false);
        }

        public MessageSecurityOverTcp Message
        {
            get
            {
                return this.messageSecurity;
            }
            set
            {
                this.messageSecurity = value;
            }
        }

        [DefaultValue(1)]
        public SecurityMode Mode
        {
            get
            {
                return this.mode;
            }
            set
            {
                if (!SecurityModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.mode = value;
            }
        }

        public TcpTransportSecurity Transport
        {
            get
            {
                return this.transportSecurity;
            }
            set
            {
                this.transportSecurity = value;
            }
        }
    }
}

