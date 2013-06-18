namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Xml;

    internal abstract class WsrmIndex
    {
        private static Wsrm11Index wsAddressing10WSReliableMessaging11;
        private static WsrmFeb2005Index wsAddressing10WSReliableMessagingFeb2005;
        private static Wsrm11Index wsAddressingAug2004WSReliableMessaging11;
        private static WsrmFeb2005Index wsAddressingAug2004WSReliableMessagingFeb2005;

        protected WsrmIndex()
        {
        }

        internal static ActionHeader GetAckRequestedActionHeader(AddressingVersion addressingVersion, ReliableMessagingVersion reliableMessagingVersion)
        {
            return GetActionHeader(addressingVersion, reliableMessagingVersion, "AckRequested");
        }

        protected abstract ActionHeader GetActionHeader(string element);
        private static ActionHeader GetActionHeader(AddressingVersion addressingVersion, ReliableMessagingVersion reliableMessagingVersion, string element)
        {
            WsrmIndex index = null;
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                if (addressingVersion == AddressingVersion.WSAddressingAugust2004)
                {
                    if (wsAddressingAug2004WSReliableMessagingFeb2005 == null)
                    {
                        wsAddressingAug2004WSReliableMessagingFeb2005 = new WsrmFeb2005Index(addressingVersion);
                    }
                    index = wsAddressingAug2004WSReliableMessagingFeb2005;
                }
                else if (addressingVersion == AddressingVersion.WSAddressing10)
                {
                    if (wsAddressing10WSReliableMessagingFeb2005 == null)
                    {
                        wsAddressing10WSReliableMessagingFeb2005 = new WsrmFeb2005Index(addressingVersion);
                    }
                    index = wsAddressing10WSReliableMessagingFeb2005;
                }
            }
            else
            {
                if (reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11)
                {
                    throw Fx.AssertAndThrow("Reliable messaging version not supported.");
                }
                if (addressingVersion == AddressingVersion.WSAddressingAugust2004)
                {
                    if (wsAddressingAug2004WSReliableMessaging11 == null)
                    {
                        wsAddressingAug2004WSReliableMessaging11 = new Wsrm11Index(addressingVersion);
                    }
                    index = wsAddressingAug2004WSReliableMessaging11;
                }
                else if (addressingVersion == AddressingVersion.WSAddressing10)
                {
                    if (wsAddressing10WSReliableMessaging11 == null)
                    {
                        wsAddressing10WSReliableMessaging11 = new Wsrm11Index(addressingVersion);
                    }
                    index = wsAddressing10WSReliableMessaging11;
                }
            }
            if (index == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("AddressingVersionNotSupported", new object[] { addressingVersion })));
            }
            return index.GetActionHeader(element);
        }

        internal static ActionHeader GetCloseSequenceActionHeader(AddressingVersion addressingVersion)
        {
            return GetActionHeader(addressingVersion, ReliableMessagingVersion.WSReliableMessaging11, "CloseSequence");
        }

        internal static ActionHeader GetCloseSequenceResponseActionHeader(AddressingVersion addressingVersion)
        {
            return GetActionHeader(addressingVersion, ReliableMessagingVersion.WSReliableMessaging11, "CloseSequenceResponse");
        }

        internal static ActionHeader GetCreateSequenceActionHeader(AddressingVersion addressingVersion, ReliableMessagingVersion reliableMessagingVersion)
        {
            return GetActionHeader(addressingVersion, reliableMessagingVersion, "CreateSequence");
        }

        internal static string GetCreateSequenceActionString(ReliableMessagingVersion reliableMessagingVersion)
        {
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                return "http://schemas.xmlsoap.org/ws/2005/02/rm/CreateSequence";
            }
            if (reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11)
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
            return "http://docs.oasis-open.org/ws-rx/wsrm/200702/CreateSequence";
        }

        internal static XmlDictionaryString GetCreateSequenceResponseAction(ReliableMessagingVersion reliableMessagingVersion)
        {
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                return XD.WsrmFeb2005Dictionary.CreateSequenceResponseAction;
            }
            if (reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11)
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
            return DXD.Wsrm11Dictionary.CreateSequenceResponseAction;
        }

        internal static string GetCreateSequenceResponseActionString(ReliableMessagingVersion reliableMessagingVersion)
        {
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                return "http://schemas.xmlsoap.org/ws/2005/02/rm/CreateSequenceResponse";
            }
            if (reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11)
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
            return "http://docs.oasis-open.org/ws-rx/wsrm/200702/CreateSequenceResponse";
        }

        internal static string GetFaultActionString(AddressingVersion addressingVersion, ReliableMessagingVersion reliableMessagingVersion)
        {
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                return addressingVersion.DefaultFaultAction;
            }
            if (reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11)
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
            return "http://docs.oasis-open.org/ws-rx/wsrm/200702/fault";
        }

        internal static XmlDictionaryString GetNamespace(ReliableMessagingVersion reliableMessagingVersion)
        {
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                return XD.WsrmFeb2005Dictionary.Namespace;
            }
            if (reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11)
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
            return DXD.Wsrm11Dictionary.Namespace;
        }

        internal static string GetNamespaceString(ReliableMessagingVersion reliableMessagingVersion)
        {
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                return "http://schemas.xmlsoap.org/ws/2005/02/rm";
            }
            if (reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11)
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
            return "http://docs.oasis-open.org/ws-rx/wsrm/200702";
        }

        internal static ActionHeader GetSequenceAcknowledgementActionHeader(AddressingVersion addressingVersion, ReliableMessagingVersion reliableMessagingVersion)
        {
            return GetActionHeader(addressingVersion, reliableMessagingVersion, "SequenceAcknowledgement");
        }

        internal static string GetSequenceAcknowledgementActionString(ReliableMessagingVersion reliableMessagingVersion)
        {
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                return "http://schemas.xmlsoap.org/ws/2005/02/rm/SequenceAcknowledgement";
            }
            if (reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11)
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
            return "http://docs.oasis-open.org/ws-rx/wsrm/200702/SequenceAcknowledgement";
        }

        internal static MessagePartSpecification GetSignedReliabilityMessageParts(ReliableMessagingVersion reliableMessagingVersion)
        {
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                return WsrmFeb2005Index.SignedReliabilityMessageParts;
            }
            if (reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11)
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
            return Wsrm11Index.SignedReliabilityMessageParts;
        }

        internal static ActionHeader GetTerminateSequenceActionHeader(AddressingVersion addressingVersion, ReliableMessagingVersion reliableMessagingVersion)
        {
            return GetActionHeader(addressingVersion, reliableMessagingVersion, "TerminateSequence");
        }

        internal static string GetTerminateSequenceActionString(ReliableMessagingVersion reliableMessagingVersion)
        {
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                return "http://schemas.xmlsoap.org/ws/2005/02/rm/TerminateSequence";
            }
            if (reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11)
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
            return "http://docs.oasis-open.org/ws-rx/wsrm/200702/TerminateSequence";
        }

        internal static ActionHeader GetTerminateSequenceResponseActionHeader(AddressingVersion addressingVersion)
        {
            return GetActionHeader(addressingVersion, ReliableMessagingVersion.WSReliableMessaging11, "TerminateSequenceResponse");
        }

        internal static string GetTerminateSequenceResponseActionString(ReliableMessagingVersion reliableMessagingVersion)
        {
            if (reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11)
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
            return "http://docs.oasis-open.org/ws-rx/wsrm/200702/TerminateSequenceResponse";
        }
    }
}

