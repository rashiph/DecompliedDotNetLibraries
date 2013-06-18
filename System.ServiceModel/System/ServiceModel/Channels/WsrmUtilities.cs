namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Xml;

    internal static class WsrmUtilities
    {
        internal static void AddAcknowledgementHeader(ReliableMessagingVersion reliableMessagingVersion, Message message, UniqueId id, SequenceRangeCollection ranges, bool final)
        {
            AddAcknowledgementHeader(reliableMessagingVersion, message, id, ranges, final, -1);
        }

        internal static void AddAcknowledgementHeader(ReliableMessagingVersion reliableMessagingVersion, Message message, UniqueId id, SequenceRangeCollection ranges, bool final, int bufferRemaining)
        {
            message.Headers.Insert(0, new WsrmAcknowledgmentHeader(reliableMessagingVersion, id, ranges, final, bufferRemaining));
        }

        internal static void AddAckRequestedHeader(ReliableMessagingVersion reliableMessagingVersion, Message message, UniqueId id)
        {
            message.Headers.Insert(0, new WsrmAckRequestedHeader(reliableMessagingVersion, id));
        }

        internal static void AddSequenceHeader(ReliableMessagingVersion reliableMessagingVersion, Message message, UniqueId id, long sequenceNumber, bool isLast)
        {
            message.Headers.Insert(0, new WsrmSequencedMessageHeader(reliableMessagingVersion, id, sequenceNumber, isLast));
        }

        internal static void AssertWsrm11(ReliableMessagingVersion reliableMessagingVersion)
        {
            if (reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11)
            {
                throw Fx.AssertAndThrow("WS-ReliableMessaging 1.1 required.");
            }
        }

        public static TimeSpan CalculateKeepAliveInterval(TimeSpan inactivityTimeout, int maxRetryCount)
        {
            return Ticks.ToTimeSpan((Ticks.FromTimeSpan(inactivityTimeout) / 2L) / ((long) maxRetryCount));
        }

        internal static Message CreateAcknowledgmentMessage(MessageVersion version, ReliableMessagingVersion reliableMessagingVersion, UniqueId id, SequenceRangeCollection ranges, bool final, int bufferRemaining)
        {
            Message message = Message.CreateMessage(version, WsrmIndex.GetSequenceAcknowledgementActionHeader(version.Addressing, reliableMessagingVersion));
            AddAcknowledgementHeader(reliableMessagingVersion, message, id, ranges, final, bufferRemaining);
            message.Properties.AllowOutputBatching = false;
            return message;
        }

        internal static Message CreateAckRequestedMessage(MessageVersion messageVersion, ReliableMessagingVersion reliableMessagingVersion, UniqueId id)
        {
            Message message = Message.CreateMessage(messageVersion, WsrmIndex.GetAckRequestedActionHeader(messageVersion.Addressing, reliableMessagingVersion));
            AddAckRequestedHeader(reliableMessagingVersion, message, id);
            message.Properties.AllowOutputBatching = false;
            return message;
        }

        internal static Message CreateCloseSequenceResponse(MessageVersion messageVersion, UniqueId messageId, UniqueId inputId)
        {
            CloseSequenceResponse body = new CloseSequenceResponse(inputId);
            Message message = Message.CreateMessage(messageVersion, WsrmIndex.GetCloseSequenceResponseActionHeader(messageVersion.Addressing), body);
            message.Headers.RelatesTo = messageId;
            return message;
        }

        internal static Message CreateCreateSequenceResponse(MessageVersion messageVersion, ReliableMessagingVersion reliableMessagingVersion, bool duplex, CreateSequenceInfo createSequenceInfo, bool ordered, UniqueId inputId, EndpointAddress acceptAcksTo)
        {
            CreateSequenceResponse body = new CreateSequenceResponse(messageVersion.Addressing, reliableMessagingVersion) {
                Identifier = inputId,
                Expires = createSequenceInfo.Expires,
                Ordered = ordered
            };
            if (duplex)
            {
                body.AcceptAcksTo = acceptAcksTo;
            }
            return Message.CreateMessage(messageVersion, ActionHeader.Create(WsrmIndex.GetCreateSequenceResponseAction(reliableMessagingVersion), messageVersion.Addressing), body);
        }

        public static Exception CreateCSFaultException(MessageVersion version, ReliableMessagingVersion reliableMessagingVersion, Message message, IChannel innerChannel)
        {
            FaultCode subCode;
            Exception exception;
            MessageFault messageFault = MessageFault.CreateFault(message, 0x10000);
            FaultCode code = messageFault.Code;
            if (version.Envelope == EnvelopeVersion.Soap11)
            {
                subCode = code;
            }
            else
            {
                if (version.Envelope != EnvelopeVersion.Soap12)
                {
                    throw Fx.AssertAndThrow("Unsupported version.");
                }
                subCode = code.SubCode;
            }
            if (subCode != null)
            {
                if ((subCode.Namespace == WsrmIndex.GetNamespaceString(reliableMessagingVersion)) && (subCode.Name == "CreateSequenceRefused"))
                {
                    string safeReasonText = FaultException.GetSafeReasonText(messageFault);
                    if (version.Envelope == EnvelopeVersion.Soap12)
                    {
                        FaultCode code3 = subCode.SubCode;
                        if (((code3 != null) && (code3.Namespace == "http://schemas.microsoft.com/ws/2006/05/rm")) && (code3.Name == "ConnectionLimitReached"))
                        {
                            return new ServerTooBusyException(safeReasonText);
                        }
                        if (code.IsSenderFault)
                        {
                            return new ProtocolException(safeReasonText);
                        }
                    }
                    return new CommunicationException(safeReasonText);
                }
                if ((subCode.Namespace == version.Addressing.Namespace) && (subCode.Name == "EndpointUnavailable"))
                {
                    return new EndpointNotFoundException(FaultException.GetSafeReasonText(messageFault));
                }
            }
            FaultConverter property = innerChannel.GetProperty<FaultConverter>();
            if (property == null)
            {
                property = FaultConverter.GetDefaultFaultConverter(version);
            }
            if (property.TryCreateException(message, messageFault, out exception))
            {
                return exception;
            }
            return new ProtocolException(System.ServiceModel.SR.GetString("UnrecognizedFaultReceivedOnOpen", new object[] { messageFault.Code.Namespace, messageFault.Code.Name, FaultException.GetSafeReasonText(messageFault) }));
        }

        internal static Message CreateCSRefusedCommunicationFault(MessageVersion messageVersion, ReliableMessagingVersion reliableMessagingVersion, string reason)
        {
            return CreateCSRefusedFault(messageVersion, reliableMessagingVersion, false, null, reason);
        }

        private static Message CreateCSRefusedFault(MessageVersion messageVersion, ReliableMessagingVersion reliableMessagingVersion, bool isSenderFault, FaultCode subCode, string reason)
        {
            FaultCode code;
            if (messageVersion.Envelope == EnvelopeVersion.Soap11)
            {
                code = new FaultCode("CreateSequenceRefused", WsrmIndex.GetNamespaceString(reliableMessagingVersion));
            }
            else
            {
                if (messageVersion.Envelope != EnvelopeVersion.Soap12)
                {
                    throw Fx.AssertAndThrow("Unsupported version.");
                }
                if (subCode == null)
                {
                    subCode = new FaultCode("CreateSequenceRefused", WsrmIndex.GetNamespaceString(reliableMessagingVersion), subCode);
                }
                if (isSenderFault)
                {
                    code = FaultCode.CreateSenderFaultCode(subCode);
                }
                else
                {
                    code = FaultCode.CreateReceiverFaultCode(subCode);
                }
            }
            FaultReason reason2 = new FaultReason(System.ServiceModel.SR.GetString("CSRefused", new object[] { reason }), CultureInfo.CurrentCulture);
            MessageFault fault = MessageFault.CreateFault(code, reason2);
            string faultActionString = WsrmIndex.GetFaultActionString(messageVersion.Addressing, reliableMessagingVersion);
            return Message.CreateMessage(messageVersion, fault, faultActionString);
        }

        internal static Message CreateCSRefusedProtocolFault(MessageVersion messageVersion, ReliableMessagingVersion reliableMessagingVersion, string reason)
        {
            return CreateCSRefusedFault(messageVersion, reliableMessagingVersion, true, null, reason);
        }

        internal static Message CreateCSRefusedServerTooBusyFault(MessageVersion messageVersion, ReliableMessagingVersion reliableMessagingVersion, string reason)
        {
            FaultCode subCode = new FaultCode("ConnectionLimitReached", "http://schemas.microsoft.com/ws/2006/05/rm");
            subCode = new FaultCode("CreateSequenceRefused", WsrmIndex.GetNamespaceString(reliableMessagingVersion), subCode);
            return CreateCSRefusedFault(messageVersion, reliableMessagingVersion, false, subCode, reason);
        }

        internal static Message CreateEndpointNotFoundFault(MessageVersion version, string reason)
        {
            FaultCode code2;
            FaultCode subCode = new FaultCode("EndpointUnavailable", version.Addressing.Namespace);
            if (version.Envelope == EnvelopeVersion.Soap11)
            {
                code2 = subCode;
            }
            else
            {
                if (version.Envelope != EnvelopeVersion.Soap12)
                {
                    throw Fx.AssertAndThrow("Unsupported version.");
                }
                code2 = FaultCode.CreateSenderFaultCode(subCode);
            }
            FaultReason reason2 = new FaultReason(reason, CultureInfo.CurrentCulture);
            MessageFault fault = MessageFault.CreateFault(code2, reason2);
            return Message.CreateMessage(version, fault, version.Addressing.DefaultFaultAction);
        }

        internal static Message CreateTerminateMessage(MessageVersion version, ReliableMessagingVersion reliableMessagingVersion, UniqueId id)
        {
            return CreateTerminateMessage(version, reliableMessagingVersion, id, -1L);
        }

        internal static Message CreateTerminateMessage(MessageVersion version, ReliableMessagingVersion reliableMessagingVersion, UniqueId id, long last)
        {
            Message message = Message.CreateMessage(version, WsrmIndex.GetTerminateSequenceActionHeader(version.Addressing, reliableMessagingVersion), new TerminateSequence(reliableMessagingVersion, id, last));
            message.Properties.AllowOutputBatching = false;
            return message;
        }

        internal static Message CreateTerminateResponseMessage(MessageVersion version, UniqueId messageId, UniqueId sequenceId)
        {
            Message message = Message.CreateMessage(version, WsrmIndex.GetTerminateSequenceResponseActionHeader(version.Addressing), new TerminateSequenceResponse(sequenceId));
            message.Properties.AllowOutputBatching = false;
            message.Headers.RelatesTo = messageId;
            return message;
        }

        internal static UniqueId GetInputId(WsrmMessageInfo info)
        {
            if (info.TerminateSequenceInfo != null)
            {
                return info.TerminateSequenceInfo.Identifier;
            }
            if (info.SequencedMessageInfo != null)
            {
                return info.SequencedMessageInfo.SequenceID;
            }
            if (info.AckRequestedInfo != null)
            {
                return info.AckRequestedInfo.SequenceID;
            }
            if ((info.WsrmHeaderFault != null) && info.WsrmHeaderFault.FaultsInput)
            {
                return info.WsrmHeaderFault.SequenceID;
            }
            if (info.CloseSequenceInfo != null)
            {
                return info.CloseSequenceInfo.Identifier;
            }
            return null;
        }

        internal static UniqueId GetOutputId(ReliableMessagingVersion reliableMessagingVersion, WsrmMessageInfo info)
        {
            if (info.AcknowledgementInfo != null)
            {
                return info.AcknowledgementInfo.SequenceID;
            }
            if ((info.WsrmHeaderFault != null) && info.WsrmHeaderFault.FaultsOutput)
            {
                return info.WsrmHeaderFault.SequenceID;
            }
            if (info.TerminateSequenceResponseInfo != null)
            {
                return info.TerminateSequenceResponseInfo.Identifier;
            }
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                if (info.CloseSequenceInfo != null)
                {
                    return info.CloseSequenceInfo.Identifier;
                }
                if (info.CloseSequenceResponseInfo != null)
                {
                    return info.CloseSequenceResponseInfo.Identifier;
                }
                if (info.TerminateSequenceResponseInfo != null)
                {
                    return info.TerminateSequenceResponseInfo.Identifier;
                }
            }
            return null;
        }

        internal static bool IsWsrmAction(ReliableMessagingVersion reliableMessagingVersion, string action)
        {
            if (action == null)
            {
                return false;
            }
            return action.StartsWith(WsrmIndex.GetNamespaceString(reliableMessagingVersion), StringComparison.Ordinal);
        }

        internal static UniqueId NextSequenceId()
        {
            return new UniqueId();
        }

        public static void ReadEmptyElement(XmlDictionaryReader reader)
        {
            if (reader.IsEmptyElement)
            {
                reader.Read();
            }
            else
            {
                reader.Read();
                reader.ReadEndElement();
            }
        }

        public static UniqueId ReadIdentifier(XmlDictionaryReader reader, ReliableMessagingVersion reliableMessagingVersion)
        {
            reader.ReadStartElement(XD.WsrmFeb2005Dictionary.Identifier, WsrmIndex.GetNamespace(reliableMessagingVersion));
            UniqueId id = reader.ReadContentAsUniqueId();
            reader.ReadEndElement();
            return id;
        }

        public static long ReadSequenceNumber(XmlDictionaryReader reader)
        {
            return ReadSequenceNumber(reader, false);
        }

        public static long ReadSequenceNumber(XmlDictionaryReader reader, bool allowZero)
        {
            long num = reader.ReadContentAsLong();
            if ((num < 0L) || ((num == 0L) && !allowZero))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("InvalidSequenceNumber", new object[] { num })));
            }
            return num;
        }

        public static string UseStrings()
        {
            string str = "SupportedAddressingModeNotSupported";
            str = "SequenceTerminatedUnexpectedCloseSequence";
            str = "UnexpectedCloseSequence";
            return "SequenceTerminatedUnsupportedTerminateSequence";
        }

        public static WsrmFault ValidateCloseSequenceResponse(ChannelReliableSession session, UniqueId messageId, WsrmMessageInfo info, long last)
        {
            string exceptionMessage = null;
            string faultReason = null;
            if (info.CloseSequenceResponseInfo == null)
            {
                exceptionMessage = System.ServiceModel.SR.GetString("InvalidWsrmResponseSessionFaultedExceptionString", new object[] { "CloseSequence", info.Action, "http://docs.oasis-open.org/ws-rx/wsrm/200702/CloseSequenceResponse" });
                faultReason = System.ServiceModel.SR.GetString("InvalidWsrmResponseSessionFaultedFaultString", new object[] { "CloseSequence", info.Action, "http://docs.oasis-open.org/ws-rx/wsrm/200702/CloseSequenceResponse" });
            }
            else if (!object.Equals(messageId, info.CloseSequenceResponseInfo.RelatesTo))
            {
                exceptionMessage = System.ServiceModel.SR.GetString("WsrmMessageWithWrongRelatesToExceptionString", new object[] { "CloseSequence" });
                faultReason = System.ServiceModel.SR.GetString("WsrmMessageWithWrongRelatesToFaultString", new object[] { "CloseSequence" });
            }
            else
            {
                if ((info.AcknowledgementInfo != null) && info.AcknowledgementInfo.Final)
                {
                    return ValidateFinalAck(session, info, last);
                }
                exceptionMessage = System.ServiceModel.SR.GetString("MissingFinalAckExceptionString");
                faultReason = System.ServiceModel.SR.GetString("SequenceTerminatedMissingFinalAck");
            }
            return SequenceTerminatedFault.CreateProtocolFault(session.OutputID, faultReason, exceptionMessage);
        }

        public static bool ValidateCreateSequence<TChannel>(WsrmMessageInfo info, ReliableChannelListenerBase<TChannel> listener, IChannel channel, out EndpointAddress acksTo) where TChannel: class, IChannel
        {
            acksTo = null;
            string reason = null;
            if (info.CreateSequenceInfo.OfferIdentifier == null)
            {
                if (typeof(TChannel) == typeof(IDuplexSessionChannel))
                {
                    reason = System.ServiceModel.SR.GetString("CSRefusedDuplexNoOffer", new object[] { listener.Uri });
                }
                else if (typeof(TChannel) == typeof(IReplySessionChannel))
                {
                    reason = System.ServiceModel.SR.GetString("CSRefusedReplyNoOffer", new object[] { listener.Uri });
                }
            }
            else if ((listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005) && (typeof(TChannel) == typeof(IInputSessionChannel)))
            {
                reason = System.ServiceModel.SR.GetString("CSRefusedInputOffer", new object[] { listener.Uri });
            }
            if (reason != null)
            {
                info.FaultReply = CreateCSRefusedProtocolFault(listener.MessageVersion, listener.ReliableMessagingVersion, reason);
                info.FaultException = new ProtocolException(System.ServiceModel.SR.GetString("ConflictingOffer"));
                return false;
            }
            if (listener.LocalAddresses != null)
            {
                Collection<EndpointAddress> results = new Collection<EndpointAddress>();
                try
                {
                    listener.LocalAddresses.GetMatchingValues(info.Message, results);
                }
                catch (CommunicationException exception)
                {
                    Message message;
                    FaultConverter property = channel.GetProperty<FaultConverter>();
                    if (property == null)
                    {
                        property = FaultConverter.GetDefaultFaultConverter(listener.MessageVersion);
                    }
                    if (!property.TryCreateFaultMessage(exception, out message))
                    {
                        throw;
                    }
                    info.FaultReply = message;
                    info.FaultException = new ProtocolException(System.ServiceModel.SR.GetString("MessageExceptionOccurred"), exception);
                    return false;
                }
                if (results.Count > 0)
                {
                    EndpointAddress address = results[0];
                    acksTo = new EndpointAddress(info.CreateSequenceInfo.To, address.Identity, address.Headers);
                    return true;
                }
                info.FaultReply = CreateEndpointNotFoundFault(listener.MessageVersion, System.ServiceModel.SR.GetString("EndpointNotFound", new object[] { info.CreateSequenceInfo.To }));
                info.FaultException = new ProtocolException(System.ServiceModel.SR.GetString("ConflictingAddress"));
                return false;
            }
            acksTo = new EndpointAddress(info.CreateSequenceInfo.To, new AddressHeader[0]);
            return true;
        }

        public static WsrmFault ValidateFinalAck(ChannelReliableSession session, WsrmMessageInfo info, long last)
        {
            WsrmAcknowledgmentInfo acknowledgementInfo = info.AcknowledgementInfo;
            WsrmFault fault = ValidateFinalAckExists(session, acknowledgementInfo);
            if (fault != null)
            {
                return fault;
            }
            SequenceRangeCollection ranges = acknowledgementInfo.Ranges;
            if (last == 0L)
            {
                if (ranges.Count == 0)
                {
                    return null;
                }
            }
            else if (ranges.Count == 1)
            {
                SequenceRange range = ranges[0];
                if (range.Lower == 1L)
                {
                    SequenceRange range2 = ranges[0];
                    if (range2.Upper == last)
                    {
                        return null;
                    }
                }
            }
            return new InvalidAcknowledgementFault(session.OutputID, acknowledgementInfo.Ranges);
        }

        public static WsrmFault ValidateFinalAckExists(ChannelReliableSession session, WsrmAcknowledgmentInfo ackInfo)
        {
            if ((ackInfo != null) && ackInfo.Final)
            {
                return null;
            }
            string exceptionMessage = System.ServiceModel.SR.GetString("MissingFinalAckExceptionString");
            string faultReason = System.ServiceModel.SR.GetString("SequenceTerminatedMissingFinalAck");
            return SequenceTerminatedFault.CreateProtocolFault(session.OutputID, faultReason, exceptionMessage);
        }

        public static WsrmFault ValidateTerminateSequenceResponse(ChannelReliableSession session, UniqueId messageId, WsrmMessageInfo info, long last)
        {
            string exceptionMessage = null;
            string faultReason = null;
            if (info.WsrmHeaderFault is UnknownSequenceFault)
            {
                return null;
            }
            if (info.TerminateSequenceResponseInfo == null)
            {
                exceptionMessage = System.ServiceModel.SR.GetString("InvalidWsrmResponseSessionFaultedExceptionString", new object[] { "TerminateSequence", info.Action, "http://docs.oasis-open.org/ws-rx/wsrm/200702/TerminateSequenceResponse" });
                faultReason = System.ServiceModel.SR.GetString("InvalidWsrmResponseSessionFaultedFaultString", new object[] { "TerminateSequence", info.Action, "http://docs.oasis-open.org/ws-rx/wsrm/200702/TerminateSequenceResponse" });
            }
            else if (!object.Equals(messageId, info.TerminateSequenceResponseInfo.RelatesTo))
            {
                exceptionMessage = System.ServiceModel.SR.GetString("WsrmMessageWithWrongRelatesToExceptionString", new object[] { "TerminateSequence" });
                faultReason = System.ServiceModel.SR.GetString("WsrmMessageWithWrongRelatesToFaultString", new object[] { "TerminateSequence" });
            }
            else
            {
                return ValidateFinalAck(session, info, last);
            }
            return SequenceTerminatedFault.CreateProtocolFault(session.OutputID, faultReason, exceptionMessage);
        }

        public static bool ValidateWsrmRequest(ChannelReliableSession session, WsrmRequestInfo info, IReliableChannelBinder binder, RequestContext context)
        {
            if (!(info is CloseSequenceInfo) && !(info is TerminateSequenceInfo))
            {
                throw Fx.AssertAndThrow("Method is meant for CloseSequence or TerminateSequence only.");
            }
            if (info.ReplyTo.Uri != binder.RemoteAddress.Uri)
            {
                string faultReason = System.ServiceModel.SR.GetString("WsrmRequestIncorrectReplyToFaultString", new object[] { info.RequestName });
                string exceptionMessage = System.ServiceModel.SR.GetString("WsrmRequestIncorrectReplyToExceptionString", new object[] { info.RequestName });
                WsrmFault fault = SequenceTerminatedFault.CreateProtocolFault(session.InputID, faultReason, exceptionMessage);
                session.OnLocalFault(fault.CreateException(), fault, context);
                return false;
            }
            return true;
        }

        public static void WriteIdentifier(XmlDictionaryWriter writer, ReliableMessagingVersion reliableMessagingVersion, UniqueId sequenceId)
        {
            writer.WriteStartElement("r", XD.WsrmFeb2005Dictionary.Identifier, WsrmIndex.GetNamespace(reliableMessagingVersion));
            writer.WriteValue(sequenceId);
            writer.WriteEndElement();
        }
    }
}

