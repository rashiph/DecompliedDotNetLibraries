namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;

    public sealed class WSFederationHttpSecurity
    {
        internal const WSFederationHttpSecurityMode DefaultMode = WSFederationHttpSecurityMode.Message;
        private FederatedMessageSecurityOverHttp messageSecurity;
        private WSFederationHttpSecurityMode mode;

        public WSFederationHttpSecurity() : this(WSFederationHttpSecurityMode.Message, new FederatedMessageSecurityOverHttp())
        {
        }

        private WSFederationHttpSecurity(WSFederationHttpSecurityMode mode, FederatedMessageSecurityOverHttp messageSecurity)
        {
            this.mode = mode;
            this.messageSecurity = (messageSecurity == null) ? new FederatedMessageSecurityOverHttp() : messageSecurity;
        }

        internal SecurityBindingElement CreateMessageSecurity(bool isReliableSessionEnabled, MessageSecurityVersion version)
        {
            if ((this.mode != WSFederationHttpSecurityMode.Message) && (this.mode != WSFederationHttpSecurityMode.TransportWithMessageCredential))
            {
                return null;
            }
            return this.messageSecurity.CreateSecurityBindingElement(this.Mode == WSFederationHttpSecurityMode.TransportWithMessageCredential, isReliableSessionEnabled, version);
        }

        internal bool InternalShouldSerialize()
        {
            if (!this.ShouldSerializeMode())
            {
                return this.ShouldSerializeMessage();
            }
            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMessage()
        {
            return this.Message.InternalShouldSerialize();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMode()
        {
            return (this.Mode != WSFederationHttpSecurityMode.Message);
        }

        internal static bool TryCreate(SecurityBindingElement sbe, WSFederationHttpSecurityMode mode, HttpTransportSecurity transportSecurity, bool isReliableSessionEnabled, MessageSecurityVersion version, out WSFederationHttpSecurity security)
        {
            security = null;
            FederatedMessageSecurityOverHttp messageSecurity = null;
            if (sbe == null)
            {
                mode = WSFederationHttpSecurityMode.None;
            }
            else
            {
                mode &= WSFederationHttpSecurityMode.TransportWithMessageCredential | WSFederationHttpSecurityMode.Message;
                if (!FederatedMessageSecurityOverHttp.TryCreate(sbe, mode == WSFederationHttpSecurityMode.TransportWithMessageCredential, isReliableSessionEnabled, version, out messageSecurity))
                {
                    return false;
                }
            }
            security = new WSFederationHttpSecurity(mode, messageSecurity);
            return true;
        }

        public FederatedMessageSecurityOverHttp Message
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

        public WSFederationHttpSecurityMode Mode
        {
            get
            {
                return this.mode;
            }
            set
            {
                if (!WSFederationHttpSecurityModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.mode = value;
            }
        }
    }
}

