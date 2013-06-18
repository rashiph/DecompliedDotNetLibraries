namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;

    public sealed class WSDualHttpSecurity
    {
        internal const WSDualHttpSecurityMode DefaultMode = WSDualHttpSecurityMode.Message;
        private MessageSecurityOverHttp messageSecurity;
        private WSDualHttpSecurityMode mode;
        private static readonly MessageSecurityVersion WSDualMessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10;

        public WSDualHttpSecurity() : this(WSDualHttpSecurityMode.Message, new MessageSecurityOverHttp())
        {
        }

        private WSDualHttpSecurity(WSDualHttpSecurityMode mode, MessageSecurityOverHttp messageSecurity)
        {
            this.mode = mode;
            this.messageSecurity = (messageSecurity == null) ? new MessageSecurityOverHttp() : messageSecurity;
        }

        internal SecurityBindingElement CreateMessageSecurity()
        {
            if (this.mode == WSDualHttpSecurityMode.Message)
            {
                return this.messageSecurity.CreateSecurityBindingElement(false, true, WSDualMessageSecurityVersion);
            }
            return null;
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
            return (this.Mode != WSDualHttpSecurityMode.Message);
        }

        internal static bool TryCreate(SecurityBindingElement sbe, out WSDualHttpSecurity security)
        {
            security = null;
            if (sbe == null)
            {
                security = new WSDualHttpSecurity(WSDualHttpSecurityMode.None, null);
            }
            else
            {
                MessageSecurityOverHttp http;
                if (!MessageSecurityOverHttp.TryCreate<MessageSecurityOverHttp>(sbe, false, true, out http))
                {
                    return false;
                }
                security = new WSDualHttpSecurity(WSDualHttpSecurityMode.Message, http);
            }
            return SecurityElementBase.AreBindingsMatching(security.CreateMessageSecurity(), sbe);
        }

        public MessageSecurityOverHttp Message
        {
            get
            {
                return this.messageSecurity;
            }
            set
            {
                this.messageSecurity = (value == null) ? new MessageSecurityOverHttp() : value;
            }
        }

        public WSDualHttpSecurityMode Mode
        {
            get
            {
                return this.mode;
            }
            set
            {
                if (!WSDualHttpSecurityModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.mode = value;
            }
        }
    }
}

