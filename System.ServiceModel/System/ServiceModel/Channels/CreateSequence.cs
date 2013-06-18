namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Xml;

    internal sealed class CreateSequence : BodyWriter
    {
        private AddressingVersion addressingVersion;
        private IClientReliableChannelBinder binder;
        private UniqueId offerIdentifier;
        private bool ordered;
        private ReliableMessagingVersion reliableMessagingVersion;

        private CreateSequence() : base(true)
        {
        }

        public CreateSequence(AddressingVersion addressingVersion, ReliableMessagingVersion reliableMessagingVersion, bool ordered, IClientReliableChannelBinder binder, UniqueId offerIdentifier) : base(true)
        {
            this.addressingVersion = addressingVersion;
            this.reliableMessagingVersion = reliableMessagingVersion;
            this.ordered = ordered;
            this.binder = binder;
            this.offerIdentifier = offerIdentifier;
        }

        public static CreateSequenceInfo Create(MessageVersion messageVersion, ReliableMessagingVersion reliableMessagingVersion, ISecureConversationSession securitySession, XmlDictionaryReader reader)
        {
            CreateSequenceInfo info2;
            try
            {
                CreateSequenceInfo info = new CreateSequenceInfo();
                WsrmFeb2005Dictionary dictionary = XD.WsrmFeb2005Dictionary;
                XmlDictionaryString namespaceUri = WsrmIndex.GetNamespace(reliableMessagingVersion);
                reader.ReadStartElement(dictionary.CreateSequence, namespaceUri);
                info.AcksTo = EndpointAddress.ReadFrom(messageVersion.Addressing, reader, dictionary.AcksTo, namespaceUri);
                if (reader.IsStartElement(dictionary.Expires, namespaceUri))
                {
                    info.Expires = new TimeSpan?(reader.ReadElementContentAsTimeSpan());
                }
                if (!reader.IsStartElement(dictionary.Offer, namespaceUri))
                {
                    goto Label_01B7;
                }
                reader.ReadStartElement();
                reader.ReadStartElement(dictionary.Identifier, namespaceUri);
                info.OfferIdentifier = reader.ReadContentAsUniqueId();
                reader.ReadEndElement();
                bool flag = reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11;
                Wsrm11Dictionary dictionary2 = flag ? DXD.Wsrm11Dictionary : null;
                if (flag && (EndpointAddress.ReadFrom(messageVersion.Addressing, reader, dictionary2.Endpoint, namespaceUri).Uri != info.AcksTo.Uri))
                {
                    string str2 = System.ServiceModel.SR.GetString("CSRefusedAcksToMustEqualEndpoint");
                    Message message = WsrmUtilities.CreateCSRefusedProtocolFault(messageVersion, reliableMessagingVersion, str2);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(WsrmMessageInfo.CreateInternalFaultException(message, str2, new ProtocolException(str2)));
                }
                if (reader.IsStartElement(dictionary.Expires, namespaceUri))
                {
                    info.OfferExpires = new TimeSpan?(reader.ReadElementContentAsTimeSpan());
                }
                if (!flag || !reader.IsStartElement(dictionary2.IncompleteSequenceBehavior, namespaceUri))
                {
                    goto Label_01A9;
                }
                string str3 = reader.ReadElementContentAsString();
                if ((!(str3 != "DiscardEntireSequence") || !(str3 != "DiscardFollowingFirstGap")) || !(str3 != "NoDiscard"))
                {
                    goto Label_01A9;
                }
                string reason = System.ServiceModel.SR.GetString("CSRefusedInvalidIncompleteSequenceBehavior");
                Message faultReply = WsrmUtilities.CreateCSRefusedProtocolFault(messageVersion, reliableMessagingVersion, reason);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(WsrmMessageInfo.CreateInternalFaultException(faultReply, reason, new ProtocolException(reason)));
            Label_01A3:
                reader.Skip();
            Label_01A9:
                if (reader.IsStartElement())
                {
                    goto Label_01A3;
                }
                reader.ReadEndElement();
            Label_01B7:
                if (securitySession == null)
                {
                    goto Label_0217;
                }
                bool flag2 = false;
                while (reader.IsStartElement())
                {
                    if (securitySession.TryReadSessionTokenIdentifier(reader))
                    {
                        flag2 = true;
                        break;
                    }
                    reader.Skip();
                }
                if (flag2)
                {
                    goto Label_0217;
                }
                string str5 = System.ServiceModel.SR.GetString("CSRefusedRequiredSecurityElementMissing");
                Message message3 = WsrmUtilities.CreateCSRefusedProtocolFault(messageVersion, reliableMessagingVersion, str5);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(WsrmMessageInfo.CreateInternalFaultException(message3, str5, new ProtocolException(str5)));
            Label_0211:
                reader.Skip();
            Label_0217:
                if (reader.IsStartElement())
                {
                    goto Label_0211;
                }
                reader.ReadEndElement();
                if (reader.IsStartElement())
                {
                    string str6 = System.ServiceModel.SR.GetString("CSRefusedUnexpectedElementAtEndOfCSMessage");
                    Message message4 = WsrmUtilities.CreateCSRefusedProtocolFault(messageVersion, reliableMessagingVersion, str6);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(WsrmMessageInfo.CreateInternalFaultException(message4, str6, new ProtocolException(str6)));
                }
                info2 = info;
            }
            catch (XmlException exception)
            {
                string str7 = System.ServiceModel.SR.GetString("CouldNotParseWithAction", new object[] { WsrmIndex.GetCreateSequenceActionString(reliableMessagingVersion) });
                Message message5 = WsrmUtilities.CreateCSRefusedProtocolFault(messageVersion, reliableMessagingVersion, str7);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(WsrmMessageInfo.CreateInternalFaultException(message5, str7, new ProtocolException(str7, exception)));
            }
            return info2;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            WsrmFeb2005Dictionary dictionary = XD.WsrmFeb2005Dictionary;
            XmlDictionaryString namespaceUri = WsrmIndex.GetNamespace(this.reliableMessagingVersion);
            writer.WriteStartElement(dictionary.CreateSequence, namespaceUri);
            EndpointAddress localAddress = this.binder.LocalAddress;
            localAddress.WriteTo(this.addressingVersion, writer, dictionary.AcksTo, namespaceUri);
            if (this.offerIdentifier != null)
            {
                writer.WriteStartElement(dictionary.Offer, namespaceUri);
                writer.WriteStartElement(dictionary.Identifier, namespaceUri);
                writer.WriteValue(this.offerIdentifier);
                writer.WriteEndElement();
                if (this.reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
                {
                    Wsrm11Dictionary dictionary2 = DXD.Wsrm11Dictionary;
                    localAddress.WriteTo(this.addressingVersion, writer, dictionary2.Endpoint, namespaceUri);
                    writer.WriteStartElement(dictionary2.IncompleteSequenceBehavior, namespaceUri);
                    writer.WriteValue(this.ordered ? dictionary2.DiscardFollowingFirstGap : dictionary2.NoDiscard);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            ISecureConversationSession innerSession = this.binder.GetInnerSession() as ISecureConversationSession;
            if (innerSession != null)
            {
                innerSession.WriteSessionTokenIdentifier(writer);
            }
            writer.WriteEndElement();
        }
    }
}

