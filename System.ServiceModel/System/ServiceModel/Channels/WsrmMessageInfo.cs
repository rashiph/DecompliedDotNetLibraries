namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Xml;

    internal sealed class WsrmMessageInfo
    {
        private WsrmAcknowledgmentInfo acknowledgementInfo;
        private WsrmAckRequestedInfo ackRequestedInfo;
        private string action;
        private System.ServiceModel.Channels.CloseSequenceInfo closeSequenceInfo;
        private System.ServiceModel.Channels.CloseSequenceResponseInfo closeSequenceResponseInfo;
        private System.ServiceModel.Channels.CreateSequenceInfo createSequenceInfo;
        private System.ServiceModel.Channels.CreateSequenceResponseInfo createSequenceResponseInfo;
        private Exception faultException;
        private System.ServiceModel.Channels.MessageFault faultInfo;
        private System.ServiceModel.Channels.Message faultReply;
        private System.ServiceModel.Channels.Message message;
        private Exception parsingException;
        private WsrmSequencedMessageInfo sequencedMessageInfo;
        private System.ServiceModel.Channels.TerminateSequenceInfo terminateSequenceInfo;
        private System.ServiceModel.Channels.TerminateSequenceResponseInfo terminateSequenceResponseInfo;
        private WsrmUsesSequenceSSLInfo usesSequenceSSLInfo;
        private WsrmUsesSequenceSTRInfo usesSequenceSTRInfo;

        public static Exception CreateInternalFaultException(System.ServiceModel.Channels.Message faultReply, string message, Exception inner)
        {
            return new InternalFaultException(faultReply, System.ServiceModel.SR.GetString("WsrmMessageProcessingError", new object[] { message }), inner);
        }

        private static Exception CreateWsrmRequiredException(MessageVersion messageVersion)
        {
            string message = System.ServiceModel.SR.GetString("WsrmRequiredExceptionString");
            return CreateInternalFaultException(new WsrmRequiredFault(System.ServiceModel.SR.GetString("WsrmRequiredFaultString")).CreateMessage(messageVersion, ReliableMessagingVersion.WSReliableMessaging11), message, new ProtocolException(message));
        }

        public static WsrmMessageInfo Get(MessageVersion messageVersion, ReliableMessagingVersion reliableMessagingVersion, IChannel channel, ISession session, System.ServiceModel.Channels.Message message)
        {
            return Get(messageVersion, reliableMessagingVersion, channel, session, message, false);
        }

        public static WsrmMessageInfo Get(MessageVersion messageVersion, ReliableMessagingVersion reliableMessagingVersion, IChannel channel, ISession session, System.ServiceModel.Channels.Message message, bool csrOnly)
        {
            WsrmMessageInfo info = new WsrmMessageInfo {
                message = message
            };
            bool isFault = true;
            try
            {
                isFault = message.IsFault;
                MessageHeaders headers = message.Headers;
                string action = headers.Action;
                info.action = action;
                bool flag2 = false;
                bool flag3 = reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005;
                bool flag4 = reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11;
                bool flag5 = false;
                if (action == WsrmIndex.GetCreateSequenceResponseActionString(reliableMessagingVersion))
                {
                    info.createSequenceResponseInfo = System.ServiceModel.Channels.CreateSequenceResponseInfo.ReadMessage(messageVersion, reliableMessagingVersion, message, headers);
                    ValidateMustUnderstand(messageVersion, message);
                    return info;
                }
                if (csrOnly)
                {
                    return info;
                }
                if (action == WsrmIndex.GetTerminateSequenceActionString(reliableMessagingVersion))
                {
                    info.terminateSequenceInfo = System.ServiceModel.Channels.TerminateSequenceInfo.ReadMessage(messageVersion, reliableMessagingVersion, message, headers);
                    flag2 = true;
                }
                else if (action == WsrmIndex.GetCreateSequenceActionString(reliableMessagingVersion))
                {
                    info.createSequenceInfo = System.ServiceModel.Channels.CreateSequenceInfo.ReadMessage(messageVersion, reliableMessagingVersion, session as ISecureConversationSession, message, headers);
                    if (flag3)
                    {
                        ValidateMustUnderstand(messageVersion, message);
                        return info;
                    }
                    flag5 = true;
                }
                else if (flag4)
                {
                    if (action == "http://docs.oasis-open.org/ws-rx/wsrm/200702/CloseSequence")
                    {
                        info.closeSequenceInfo = System.ServiceModel.Channels.CloseSequenceInfo.ReadMessage(messageVersion, message, headers);
                        flag2 = true;
                    }
                    else if (action == "http://docs.oasis-open.org/ws-rx/wsrm/200702/CloseSequenceResponse")
                    {
                        info.closeSequenceResponseInfo = System.ServiceModel.Channels.CloseSequenceResponseInfo.ReadMessage(messageVersion, message, headers);
                        flag2 = true;
                    }
                    else if (action == WsrmIndex.GetTerminateSequenceResponseActionString(reliableMessagingVersion))
                    {
                        info.terminateSequenceResponseInfo = System.ServiceModel.Channels.TerminateSequenceResponseInfo.ReadMessage(messageVersion, message, headers);
                        flag2 = true;
                    }
                }
                string namespaceString = WsrmIndex.GetNamespaceString(reliableMessagingVersion);
                bool flag6 = messageVersion.Envelope == EnvelopeVersion.Soap11;
                bool flag7 = false;
                int num = -1;
                int headerIndex = -1;
                int num3 = -1;
                int num4 = -1;
                int num5 = -1;
                int num6 = -1;
                int index = -1;
                int num8 = -1;
                int num9 = -1;
                for (int i = 0; i < headers.Count; i++)
                {
                    MessageHeaderInfo info2 = headers[i];
                    if (messageVersion.Envelope.IsUltimateDestinationActor(info2.Actor) && (info2.Namespace == namespaceString))
                    {
                        bool flag8 = true;
                        if (flag5)
                        {
                            if (flag4 && (info2.Name == "UsesSequenceSSL"))
                            {
                                if (num8 != -1)
                                {
                                    num = i;
                                    break;
                                }
                                num8 = i;
                            }
                            else if (flag4 && (info2.Name == "UsesSequenceSTR"))
                            {
                                if (num9 != -1)
                                {
                                    num = i;
                                    break;
                                }
                                num9 = i;
                            }
                            else
                            {
                                flag8 = false;
                            }
                        }
                        else if (info2.Name == "Sequence")
                        {
                            if (headerIndex != -1)
                            {
                                num = i;
                                break;
                            }
                            headerIndex = i;
                        }
                        else if (info2.Name == "SequenceAcknowledgement")
                        {
                            if (num3 != -1)
                            {
                                num = i;
                                break;
                            }
                            num3 = i;
                        }
                        else if (info2.Name == "AckRequested")
                        {
                            if (num4 != -1)
                            {
                                num = i;
                                break;
                            }
                            num4 = i;
                        }
                        else if (flag6 && (info2.Name == "SequenceFault"))
                        {
                            if (index != -1)
                            {
                                num = i;
                                break;
                            }
                            index = i;
                        }
                        else
                        {
                            flag8 = false;
                        }
                        if (flag8)
                        {
                            if (i > num5)
                            {
                                num5 = i;
                            }
                            if (num6 == -1)
                            {
                                num6 = i;
                            }
                        }
                    }
                }
                if (num != -1)
                {
                    Collection<MessageHeaderInfo> notUnderstoodHeaders = new Collection<MessageHeaderInfo> {
                        headers[num]
                    };
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MustUnderstandSoapException(notUnderstoodHeaders, messageVersion.Envelope));
                }
                if (num5 > -1)
                {
                    BufferedMessage message2 = message as BufferedMessage;
                    if ((message2 != null) && message2.Headers.ContainsOnlyBufferedMessageHeaders)
                    {
                        flag7 = true;
                        using (XmlDictionaryReader reader = headers.GetReaderAtHeader(num6))
                        {
                            for (int j = num6; j <= num5; j++)
                            {
                                MessageHeaderInfo header = headers[j];
                                if (flag5)
                                {
                                    if (flag4 && (j == num8))
                                    {
                                        info.usesSequenceSSLInfo = WsrmUsesSequenceSSLInfo.ReadHeader(reader, header);
                                        headers.UnderstoodHeaders.Add(header);
                                    }
                                    else if (flag4 && (j == num9))
                                    {
                                        info.usesSequenceSTRInfo = WsrmUsesSequenceSTRInfo.ReadHeader(reader, header);
                                        headers.UnderstoodHeaders.Add(header);
                                    }
                                    else
                                    {
                                        reader.Skip();
                                    }
                                }
                                else if (j == headerIndex)
                                {
                                    info.sequencedMessageInfo = WsrmSequencedMessageInfo.ReadHeader(reliableMessagingVersion, reader, header);
                                    headers.UnderstoodHeaders.Add(header);
                                }
                                else if (j == num3)
                                {
                                    info.acknowledgementInfo = WsrmAcknowledgmentInfo.ReadHeader(reliableMessagingVersion, reader, header);
                                    headers.UnderstoodHeaders.Add(header);
                                }
                                else if (j == num4)
                                {
                                    info.ackRequestedInfo = WsrmAckRequestedInfo.ReadHeader(reliableMessagingVersion, reader, header);
                                    headers.UnderstoodHeaders.Add(header);
                                }
                                else
                                {
                                    reader.Skip();
                                }
                            }
                        }
                    }
                }
                if ((num5 > -1) && !flag7)
                {
                    flag7 = true;
                    if (flag5)
                    {
                        if (num8 != -1)
                        {
                            using (XmlDictionaryReader reader2 = headers.GetReaderAtHeader(num8))
                            {
                                MessageHeaderInfo info4 = headers[num8];
                                info.usesSequenceSSLInfo = WsrmUsesSequenceSSLInfo.ReadHeader(reader2, info4);
                                headers.UnderstoodHeaders.Add(info4);
                            }
                        }
                        if (num9 == -1)
                        {
                            goto Label_05CB;
                        }
                        using (XmlDictionaryReader reader3 = headers.GetReaderAtHeader(num9))
                        {
                            MessageHeaderInfo info5 = headers[num9];
                            info.usesSequenceSTRInfo = WsrmUsesSequenceSTRInfo.ReadHeader(reader3, info5);
                            headers.UnderstoodHeaders.Add(info5);
                            goto Label_05CB;
                        }
                    }
                    if (headerIndex != -1)
                    {
                        using (XmlDictionaryReader reader4 = headers.GetReaderAtHeader(headerIndex))
                        {
                            MessageHeaderInfo info6 = headers[headerIndex];
                            info.sequencedMessageInfo = WsrmSequencedMessageInfo.ReadHeader(reliableMessagingVersion, reader4, info6);
                            headers.UnderstoodHeaders.Add(info6);
                        }
                    }
                    if (num3 != -1)
                    {
                        using (XmlDictionaryReader reader5 = headers.GetReaderAtHeader(num3))
                        {
                            MessageHeaderInfo info7 = headers[num3];
                            info.acknowledgementInfo = WsrmAcknowledgmentInfo.ReadHeader(reliableMessagingVersion, reader5, info7);
                            headers.UnderstoodHeaders.Add(info7);
                        }
                    }
                    if (num4 != -1)
                    {
                        using (XmlDictionaryReader reader6 = headers.GetReaderAtHeader(num4))
                        {
                            MessageHeaderInfo info8 = headers[num4];
                            info.ackRequestedInfo = WsrmAckRequestedInfo.ReadHeader(reliableMessagingVersion, reader6, info8);
                            headers.UnderstoodHeaders.Add(info8);
                        }
                    }
                }
            Label_05CB:
                if (flag5)
                {
                    System.ServiceModel.Channels.CreateSequenceInfo.ValidateCreateSequenceHeaders(messageVersion, session as ISecureConversationSession, info);
                    ValidateMustUnderstand(messageVersion, message);
                    return info;
                }
                if ((info.sequencedMessageInfo == null) && (info.action == null))
                {
                    if (flag3)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageHeaderException(System.ServiceModel.SR.GetString("NoActionNoSequenceHeaderReason"), messageVersion.Addressing.Namespace, "Action", false));
                    }
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateWsrmRequiredException(messageVersion));
                }
                if ((info.sequencedMessageInfo == null) && message.IsFault)
                {
                    System.ServiceModel.Channels.WsrmHeaderFault fault;
                    info.faultInfo = System.ServiceModel.Channels.MessageFault.CreateFault(message, 0x10000);
                    if (flag6)
                    {
                        if (System.ServiceModel.Channels.WsrmHeaderFault.TryCreateFault11(reliableMessagingVersion, message, info.faultInfo, index, out fault))
                        {
                            info.faultInfo = fault;
                            info.faultException = WsrmFault.CreateException(fault);
                        }
                    }
                    else if (System.ServiceModel.Channels.WsrmHeaderFault.TryCreateFault12(reliableMessagingVersion, message, info.faultInfo, out fault))
                    {
                        info.faultInfo = fault;
                        info.faultException = WsrmFault.CreateException(fault);
                    }
                    if (fault == null)
                    {
                        FaultConverter property = channel.GetProperty<FaultConverter>();
                        if (property == null)
                        {
                            property = FaultConverter.GetDefaultFaultConverter(messageVersion);
                        }
                        if (!property.TryCreateException(message, info.faultInfo, out info.faultException))
                        {
                            info.faultException = new ProtocolException(System.ServiceModel.SR.GetString("UnrecognizedFaultReceived", new object[] { info.faultInfo.Code.Namespace, info.faultInfo.Code.Name, System.ServiceModel.FaultException.GetSafeReasonText(info.faultInfo) }));
                        }
                    }
                    flag2 = true;
                }
                if (!flag7 && !flag2)
                {
                    if (flag3)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ActionNotSupportedException(System.ServiceModel.SR.GetString("NonWsrmFeb2005ActionNotSupported", new object[] { action })));
                    }
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateWsrmRequiredException(messageVersion));
                }
                if (!flag2 && !WsrmUtilities.IsWsrmAction(reliableMessagingVersion, action))
                {
                    return info;
                }
                ValidateMustUnderstand(messageVersion, message);
            }
            catch (InternalFaultException exception)
            {
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceInformation)
                {
                    System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
                info.FaultReply = exception.FaultReply;
                info.faultException = exception.InnerException;
            }
            catch (CommunicationException exception2)
            {
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceInformation)
                {
                    System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                }
                if (isFault)
                {
                    info.parsingException = exception2;
                    return info;
                }
                FaultConverter defaultFaultConverter = channel.GetProperty<FaultConverter>();
                if (defaultFaultConverter == null)
                {
                    defaultFaultConverter = FaultConverter.GetDefaultFaultConverter(messageVersion);
                }
                if (defaultFaultConverter.TryCreateFaultMessage(exception2, out info.faultReply))
                {
                    info.faultException = new ProtocolException(System.ServiceModel.SR.GetString("MessageExceptionOccurred"), exception2);
                    return info;
                }
                info.parsingException = new ProtocolException(System.ServiceModel.SR.GetString("MessageExceptionOccurred"), exception2);
            }
            catch (XmlException exception3)
            {
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceInformation)
                {
                    System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                }
                info.parsingException = new ProtocolException(System.ServiceModel.SR.GetString("MessageExceptionOccurred"), exception3);
            }
            return info;
        }

        private static void ValidateMustUnderstand(MessageVersion version, System.ServiceModel.Channels.Message message)
        {
            Collection<MessageHeaderInfo> headersNotUnderstood = message.Headers.GetHeadersNotUnderstood();
            if ((headersNotUnderstood != null) && (headersNotUnderstood.Count > 0))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MustUnderstandSoapException(headersNotUnderstood, version.Envelope));
            }
        }

        public WsrmAcknowledgmentInfo AcknowledgementInfo
        {
            get
            {
                return this.acknowledgementInfo;
            }
        }

        public WsrmAckRequestedInfo AckRequestedInfo
        {
            get
            {
                return this.ackRequestedInfo;
            }
        }

        public string Action
        {
            get
            {
                return this.action;
            }
        }

        public System.ServiceModel.Channels.CloseSequenceInfo CloseSequenceInfo
        {
            get
            {
                return this.closeSequenceInfo;
            }
        }

        public System.ServiceModel.Channels.CloseSequenceResponseInfo CloseSequenceResponseInfo
        {
            get
            {
                return this.closeSequenceResponseInfo;
            }
        }

        public System.ServiceModel.Channels.CreateSequenceInfo CreateSequenceInfo
        {
            get
            {
                return this.createSequenceInfo;
            }
        }

        public System.ServiceModel.Channels.CreateSequenceResponseInfo CreateSequenceResponseInfo
        {
            get
            {
                return this.createSequenceResponseInfo;
            }
        }

        public Exception FaultException
        {
            get
            {
                return this.faultException;
            }
            set
            {
                if (this.faultException != null)
                {
                    throw Fx.AssertAndThrow("FaultException can only be set once.");
                }
                this.faultException = value;
            }
        }

        public System.ServiceModel.Channels.MessageFault FaultInfo
        {
            get
            {
                return this.faultInfo;
            }
        }

        public System.ServiceModel.Channels.Message FaultReply
        {
            get
            {
                return this.faultReply;
            }
            set
            {
                if (this.faultReply != null)
                {
                    throw Fx.AssertAndThrow("FaultReply can only be set once.");
                }
                this.faultReply = value;
            }
        }

        public System.ServiceModel.Channels.Message Message
        {
            get
            {
                return this.message;
            }
        }

        public System.ServiceModel.Channels.MessageFault MessageFault
        {
            get
            {
                return this.faultInfo;
            }
        }

        public Exception ParsingException
        {
            get
            {
                return this.parsingException;
            }
        }

        public WsrmSequencedMessageInfo SequencedMessageInfo
        {
            get
            {
                return this.sequencedMessageInfo;
            }
        }

        public System.ServiceModel.Channels.TerminateSequenceInfo TerminateSequenceInfo
        {
            get
            {
                return this.terminateSequenceInfo;
            }
        }

        public System.ServiceModel.Channels.TerminateSequenceResponseInfo TerminateSequenceResponseInfo
        {
            get
            {
                return this.terminateSequenceResponseInfo;
            }
        }

        public WsrmUsesSequenceSSLInfo UsesSequenceSSLInfo
        {
            get
            {
                return this.usesSequenceSSLInfo;
            }
        }

        public WsrmUsesSequenceSTRInfo UsesSequenceSTRInfo
        {
            get
            {
                return this.usesSequenceSTRInfo;
            }
        }

        public System.ServiceModel.Channels.WsrmHeaderFault WsrmHeaderFault
        {
            get
            {
                return (this.faultInfo as System.ServiceModel.Channels.WsrmHeaderFault);
            }
        }

        [Serializable]
        private class InternalFaultException : ProtocolException
        {
            private Message faultReply;

            public InternalFaultException()
            {
            }

            protected InternalFaultException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }

            public InternalFaultException(Message faultReply, string message, Exception inner) : base(message, inner)
            {
                this.faultReply = faultReply;
            }

            public Message FaultReply
            {
                get
                {
                    return this.faultReply;
                }
            }
        }
    }
}

