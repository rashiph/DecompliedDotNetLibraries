namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;

    public sealed class NetMsmqSecurity
    {
        internal const NetMsmqSecurityMode DefaultMode = NetMsmqSecurityMode.Transport;
        private MessageSecurityOverMsmq messageSecurity;
        private NetMsmqSecurityMode mode;
        private MsmqTransportSecurity transportSecurity;

        public NetMsmqSecurity() : this(NetMsmqSecurityMode.Transport, null, null)
        {
        }

        internal NetMsmqSecurity(NetMsmqSecurityMode mode) : this(mode, null, null)
        {
        }

        private NetMsmqSecurity(NetMsmqSecurityMode mode, MsmqTransportSecurity transportSecurity, MessageSecurityOverMsmq messageSecurity)
        {
            this.mode = mode;
            this.transportSecurity = (transportSecurity == null) ? new MsmqTransportSecurity() : transportSecurity;
            this.messageSecurity = (messageSecurity == null) ? new MessageSecurityOverMsmq() : messageSecurity;
        }

        internal void ConfigureTransportSecurity(System.ServiceModel.Channels.MsmqBindingElementBase msmq)
        {
            if ((this.mode == NetMsmqSecurityMode.Transport) || (this.mode == NetMsmqSecurityMode.Both))
            {
                msmq.MsmqTransportSecurity = this.Transport;
            }
            else
            {
                msmq.MsmqTransportSecurity.Disable();
            }
        }

        internal SecurityBindingElement CreateMessageSecurity()
        {
            return this.Message.CreateSecurityBindingElement();
        }

        internal static bool IsConfiguredTransportSecurity(MsmqTransportBindingElement msmq, out UnifiedSecurityMode mode)
        {
            if (msmq == null)
            {
                mode = UnifiedSecurityMode.None;
                return false;
            }
            if (msmq.MsmqTransportSecurity.Enabled)
            {
                mode = UnifiedSecurityMode.Both | UnifiedSecurityMode.Transport;
            }
            else
            {
                mode = UnifiedSecurityMode.Message | UnifiedSecurityMode.None;
            }
            return true;
        }

        internal static bool TryCreate(SecurityBindingElement sbe, NetMsmqSecurityMode mode, out NetMsmqSecurity security)
        {
            MessageSecurityOverMsmq msmq;
            security = null;
            if (!MessageSecurityOverMsmq.TryCreate(sbe, out msmq))
            {
                msmq = null;
            }
            security = new NetMsmqSecurity(mode, null, msmq);
            if (sbe != null)
            {
                return SecurityElementBase.AreBindingsMatching(security.CreateMessageSecurity(), sbe, false);
            }
            return true;
        }

        public MessageSecurityOverMsmq Message
        {
            get
            {
                if (this.messageSecurity == null)
                {
                    this.messageSecurity = new MessageSecurityOverMsmq();
                }
                return this.messageSecurity;
            }
            set
            {
                this.messageSecurity = value;
            }
        }

        [DefaultValue(1)]
        public NetMsmqSecurityMode Mode
        {
            get
            {
                return this.mode;
            }
            set
            {
                if (!NetMsmqSecurityModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.mode = value;
            }
        }

        public MsmqTransportSecurity Transport
        {
            get
            {
                if (this.transportSecurity == null)
                {
                    this.transportSecurity = new MsmqTransportSecurity();
                }
                return this.transportSecurity;
            }
            set
            {
                this.transportSecurity = value;
            }
        }
    }
}

