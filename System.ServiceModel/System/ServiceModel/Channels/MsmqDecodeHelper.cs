namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.ServiceModel;
    using System.ServiceModel.MsmqIntegration;
    using System.ServiceModel.Security;
    using System.Transactions;
    using System.Xml;
    using System.Xml.Serialization;

    internal static class MsmqDecodeHelper
    {
        private static System.ServiceModel.MsmqIntegration.ActiveXSerializer activeXSerializer;
        private static System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binaryFormatter;
        private const int defaultMaxContentTypeSize = 0x100;
        private const int defaultMaxViaSize = 0x800;

        internal static Message DecodeIntegrationDatagram(MsmqIntegrationChannelListener listener, MsmqReceiveHelper receiver, MsmqIntegrationInputMessage msmqMessage, MsmqMessageProperty messageProperty)
        {
            Message message2;
            using (MsmqDiagnostics.BoundReceiveBytesOperation())
            {
                Message message = Message.CreateMessage(MessageVersion.None, (string) null);
                bool flag = true;
                try
                {
                    SecurityMessageProperty property = listener.ValidateSecurity(msmqMessage);
                    if (property != null)
                    {
                        message.Properties.Security = property;
                    }
                    MsmqIntegrationMessageProperty property2 = new MsmqIntegrationMessageProperty();
                    msmqMessage.SetMessageProperties(property2);
                    int length = msmqMessage.BodyLength.Value;
                    if (length > listener.MaxReceivedMessageSize)
                    {
                        receiver.FinalDisposition(messageProperty);
                        throw listener.NormalizePoisonException(messageProperty.LookupId, MaxMessageSizeStream.CreateMaxReceivedMessageSizeExceededException(listener.MaxReceivedMessageSize));
                    }
                    byte[] bufferCopy = msmqMessage.Body.GetBufferCopy(length);
                    MemoryStream bodyStream = new MemoryStream(bufferCopy, 0, bufferCopy.Length, false);
                    object obj2 = null;
                    using (MsmqDiagnostics.BoundDecodeOperation())
                    {
                        try
                        {
                            obj2 = DeserializeForIntegration(listener, bodyStream, property2, messageProperty.LookupId);
                        }
                        catch (SerializationException exception)
                        {
                            receiver.FinalDisposition(messageProperty);
                            throw listener.NormalizePoisonException(messageProperty.LookupId, new ProtocolException(System.ServiceModel.SR.GetString("MsmqDeserializationError"), exception));
                        }
                        property2.Body = obj2;
                        message.Properties["MsmqIntegrationMessageProperty"] = property2;
                        bodyStream.Seek(0L, SeekOrigin.Begin);
                        message.Headers.To = listener.Uri;
                        flag = false;
                        MsmqDiagnostics.TransferFromTransport(message);
                    }
                    message2 = message;
                }
                finally
                {
                    if (flag)
                    {
                        message.Close();
                    }
                }
            }
            return message2;
        }

        private static Message DecodeSessiongramMessage(MsmqInputSessionChannelListener listener, MsmqInputSessionChannel channel, MessageEncoder encoder, MsmqMessageProperty messageProperty, byte[] buffer, int offset, int size)
        {
            Message message2;
            if (size > listener.MaxReceivedMessageSize)
            {
                channel.FaultChannel();
                listener.MsmqReceiveHelper.FinalDisposition(messageProperty);
                throw listener.NormalizePoisonException(messageProperty.LookupId, MaxMessageSizeStream.CreateMaxReceivedMessageSizeExceededException(listener.MaxReceivedMessageSize));
            }
            if ((size + offset) > buffer.Length)
            {
                listener.MsmqReceiveHelper.FinalDisposition(messageProperty);
                throw listener.NormalizePoisonException(messageProperty.LookupId, new ProtocolException(System.ServiceModel.SR.GetString("MsmqBadFrame")));
            }
            byte[] dst = listener.BufferManager.TakeBuffer(size);
            Buffer.BlockCopy(buffer, offset, dst, 0, size);
            try
            {
                Message message = null;
                using (MsmqDiagnostics.BoundDecodeOperation())
                {
                    message = encoder.ReadMessage(new ArraySegment<byte>(dst, 0, size), listener.BufferManager);
                    MsmqDiagnostics.TransferFromTransport(message);
                }
                message2 = message;
            }
            catch (XmlException exception)
            {
                channel.FaultChannel();
                listener.MsmqReceiveHelper.FinalDisposition(messageProperty);
                throw listener.NormalizePoisonException(messageProperty.LookupId, new ProtocolException(System.ServiceModel.SR.GetString("MsmqBadXml"), exception));
            }
            return message2;
        }

        internal static Message DecodeTransportDatagram(MsmqInputChannelListener listener, MsmqReceiveHelper receiver, MsmqInputMessage msmqMessage, MsmqMessageProperty messageProperty)
        {
            Message message2;
            using (MsmqDiagnostics.BoundReceiveBytesOperation())
            {
                long num1 = msmqMessage.LookupId.Value;
                int size = msmqMessage.BodyLength.Value;
                int offset = 0;
                byte[] incoming = msmqMessage.Body.Buffer;
                ServerModeDecoder modeDecoder = new ServerModeDecoder();
                try
                {
                    ReadServerMode(listener, modeDecoder, incoming, messageProperty.LookupId, ref offset, ref size);
                }
                catch (ProtocolException exception)
                {
                    receiver.FinalDisposition(messageProperty);
                    throw listener.NormalizePoisonException(messageProperty.LookupId, exception);
                }
                if (modeDecoder.Mode != FramingMode.SingletonSized)
                {
                    receiver.FinalDisposition(messageProperty);
                    throw listener.NormalizePoisonException(messageProperty.LookupId, new ProtocolException(System.ServiceModel.SR.GetString("MsmqBadFrame")));
                }
                ServerSingletonSizedDecoder decoder2 = new ServerSingletonSizedDecoder(0L, 0x800, 0x100);
                try
                {
                    do
                    {
                        if (size <= 0)
                        {
                            throw listener.NormalizePoisonException(messageProperty.LookupId, decoder2.CreatePrematureEOFException());
                        }
                        int num3 = decoder2.Decode(incoming, offset, size);
                        offset += num3;
                        size -= num3;
                    }
                    while (decoder2.CurrentState != ServerSingletonSizedDecoder.State.Start);
                }
                catch (ProtocolException exception2)
                {
                    receiver.FinalDisposition(messageProperty);
                    throw listener.NormalizePoisonException(messageProperty.LookupId, exception2);
                }
                if (size > listener.MaxReceivedMessageSize)
                {
                    receiver.FinalDisposition(messageProperty);
                    throw listener.NormalizePoisonException(messageProperty.LookupId, MaxMessageSizeStream.CreateMaxReceivedMessageSizeExceededException(listener.MaxReceivedMessageSize));
                }
                if (!listener.MessageEncoderFactory.Encoder.IsContentTypeSupported(decoder2.ContentType))
                {
                    receiver.FinalDisposition(messageProperty);
                    throw listener.NormalizePoisonException(messageProperty.LookupId, new ProtocolException(System.ServiceModel.SR.GetString("MsmqBadContentType")));
                }
                byte[] dst = listener.BufferManager.TakeBuffer(size);
                Buffer.BlockCopy(incoming, offset, dst, 0, size);
                Message message = null;
                using (MsmqDiagnostics.BoundDecodeOperation())
                {
                    try
                    {
                        message = listener.MessageEncoderFactory.Encoder.ReadMessage(new ArraySegment<byte>(dst, 0, size), listener.BufferManager);
                    }
                    catch (XmlException exception3)
                    {
                        receiver.FinalDisposition(messageProperty);
                        throw listener.NormalizePoisonException(messageProperty.LookupId, new ProtocolException(System.ServiceModel.SR.GetString("MsmqBadXml"), exception3));
                    }
                    bool flag = true;
                    try
                    {
                        SecurityMessageProperty property = listener.ValidateSecurity(msmqMessage);
                        if (property != null)
                        {
                            message.Properties.Security = property;
                        }
                        flag = false;
                        MsmqDiagnostics.TransferFromTransport(message);
                        message2 = message;
                    }
                    catch (Exception exception4)
                    {
                        if (Fx.IsFatal(exception4))
                        {
                            throw;
                        }
                        receiver.FinalDisposition(messageProperty);
                        throw listener.NormalizePoisonException(messageProperty.LookupId, exception4);
                    }
                    finally
                    {
                        if (flag)
                        {
                            message.Close();
                        }
                    }
                }
            }
            return message2;
        }

        internal static IInputSessionChannel DecodeTransportSessiongram(MsmqInputSessionChannelListener listener, MsmqInputMessage msmqMessage, MsmqMessageProperty messageProperty, MsmqReceiveContextLockManager receiveContextManager)
        {
            using (MsmqDiagnostics.BoundReceiveBytesOperation())
            {
                int num4;
                long num1 = msmqMessage.LookupId.Value;
                int size = msmqMessage.BodyLength.Value;
                int offset = 0;
                byte[] incoming = msmqMessage.Body.Buffer;
                MsmqReceiveHelper msmqReceiveHelper = listener.MsmqReceiveHelper;
                ServerModeDecoder modeDecoder = new ServerModeDecoder();
                try
                {
                    ReadServerMode(listener, modeDecoder, incoming, messageProperty.LookupId, ref offset, ref size);
                }
                catch (ProtocolException exception)
                {
                    msmqReceiveHelper.FinalDisposition(messageProperty);
                    throw listener.NormalizePoisonException(messageProperty.LookupId, exception);
                }
                if (modeDecoder.Mode != FramingMode.Simplex)
                {
                    msmqReceiveHelper.FinalDisposition(messageProperty);
                    throw listener.NormalizePoisonException(messageProperty.LookupId, new ProtocolException(System.ServiceModel.SR.GetString("MsmqBadFrame")));
                }
                MsmqInputSessionChannel channel = null;
                ServerSessionDecoder decoder2 = new ServerSessionDecoder(0L, 0x800, 0x100);
                try
                {
                    do
                    {
                        if (size <= 0)
                        {
                            throw listener.NormalizePoisonException(messageProperty.LookupId, decoder2.CreatePrematureEOFException());
                        }
                        int num3 = decoder2.Decode(incoming, offset, size);
                        offset += num3;
                        size -= num3;
                    }
                    while (ServerSessionDecoder.State.EnvelopeStart != decoder2.CurrentState);
                }
                catch (ProtocolException exception2)
                {
                    msmqReceiveHelper.FinalDisposition(messageProperty);
                    throw listener.NormalizePoisonException(messageProperty.LookupId, exception2);
                }
                MessageEncoder encoder = listener.MessageEncoderFactory.CreateSessionEncoder();
                if (!encoder.IsContentTypeSupported(decoder2.ContentType))
                {
                    msmqReceiveHelper.FinalDisposition(messageProperty);
                    throw listener.NormalizePoisonException(messageProperty.LookupId, new ProtocolException(System.ServiceModel.SR.GetString("MsmqBadContentType")));
                }
                ReceiveContext sessiongramReceiveContext = null;
                if (msmqReceiveHelper.MsmqReceiveParameters.ReceiveContextSettings.Enabled)
                {
                    sessiongramReceiveContext = receiveContextManager.CreateMsmqReceiveContext(msmqMessage.LookupId.Value);
                }
                channel = new MsmqInputSessionChannel(listener, Transaction.Current, sessiongramReceiveContext);
                Message item = DecodeSessiongramMessage(listener, channel, encoder, messageProperty, incoming, offset, decoder2.EnvelopeSize);
                SecurityMessageProperty property = null;
                try
                {
                    property = listener.ValidateSecurity(msmqMessage);
                }
                catch (Exception exception3)
                {
                    if (Fx.IsFatal(exception3))
                    {
                        throw;
                    }
                    channel.FaultChannel();
                    msmqReceiveHelper.FinalDisposition(messageProperty);
                    throw listener.NormalizePoisonException(messageProperty.LookupId, exception3);
                }
                if (property != null)
                {
                    item.Properties.Security = property;
                }
                item.Properties["MsmqMessageProperty"] = messageProperty;
                channel.EnqueueAndDispatch(item);
                listener.RaiseMessageReceived();
            Label_01F6:
                try
                {
                    if (size <= 0)
                    {
                        channel.FaultChannel();
                        msmqReceiveHelper.FinalDisposition(messageProperty);
                        throw listener.NormalizePoisonException(messageProperty.LookupId, decoder2.CreatePrematureEOFException());
                    }
                    num4 = decoder2.Decode(incoming, offset, size);
                }
                catch (ProtocolException exception4)
                {
                    channel.FaultChannel();
                    msmqReceiveHelper.FinalDisposition(messageProperty);
                    throw listener.NormalizePoisonException(messageProperty.LookupId, exception4);
                }
                offset += num4;
                size -= num4;
                if (ServerSessionDecoder.State.End != decoder2.CurrentState)
                {
                    if (ServerSessionDecoder.State.EnvelopeStart == decoder2.CurrentState)
                    {
                        item = DecodeSessiongramMessage(listener, channel, encoder, messageProperty, incoming, offset, decoder2.EnvelopeSize);
                        if (property != null)
                        {
                            item.Properties.Security = (SecurityMessageProperty) property.CreateCopy();
                        }
                        item.Properties["MsmqMessageProperty"] = messageProperty;
                        channel.EnqueueAndDispatch(item);
                        listener.RaiseMessageReceived();
                    }
                    goto Label_01F6;
                }
                channel.Shutdown();
                MsmqDiagnostics.SessiongramReceived(channel.Session.Id, msmqMessage.MessageId, channel.InternalPendingItems);
                return channel;
            }
        }

        private static object DeserializeForIntegration(MsmqIntegrationChannelListener listener, Stream bodyStream, MsmqIntegrationMessageProperty property, long lookupId)
        {
            MsmqMessageSerializationFormat serializationFormat = (listener.ReceiveParameters as MsmqIntegrationReceiveParameters).SerializationFormat;
            switch (serializationFormat)
            {
                case MsmqMessageSerializationFormat.Xml:
                    return XmlDeserializeForIntegration(listener, bodyStream, lookupId);

                case MsmqMessageSerializationFormat.Binary:
                    return BinaryFormatter.Deserialize(bodyStream);

                case MsmqMessageSerializationFormat.ActiveX:
                {
                    int bodyType = property.BodyType.Value;
                    return ActiveXSerializer.Deserialize(bodyStream as MemoryStream, bodyType);
                }
                case MsmqMessageSerializationFormat.ByteArray:
                    return (bodyStream as MemoryStream).ToArray();

                case MsmqMessageSerializationFormat.Stream:
                    return bodyStream;
            }
            throw new SerializationException(System.ServiceModel.SR.GetString("MsmqUnsupportedSerializationFormat", new object[] { serializationFormat }));
        }

        private static void ReadServerMode(MsmqChannelListenerBase listener, ServerModeDecoder modeDecoder, byte[] incoming, long lookupId, ref int offset, ref int size)
        {
            do
            {
                if (size <= 0)
                {
                    throw listener.NormalizePoisonException(lookupId, modeDecoder.CreatePrematureEOFException());
                }
                int num = modeDecoder.Decode(incoming, offset, size);
                offset += num;
                size -= num;
            }
            while (ServerModeDecoder.State.Done != modeDecoder.CurrentState);
        }

        private static object XmlDeserializeForIntegration(MsmqIntegrationChannelListener listener, Stream stream, long lookupId)
        {
            XmlTextReader xmlReader = new XmlTextReader(stream) {
                WhitespaceHandling = WhitespaceHandling.Significant,
                DtdProcessing = DtdProcessing.Prohibit
            };
            try
            {
                foreach (XmlSerializer serializer in listener.XmlSerializerList)
                {
                    if (serializer.CanDeserialize(xmlReader))
                    {
                        return serializer.Deserialize(xmlReader);
                    }
                }
            }
            catch (InvalidOperationException exception)
            {
                throw new SerializationException(exception.Message);
            }
            throw new SerializationException(System.ServiceModel.SR.GetString("MsmqCannotDeserializeXmlMessage"));
        }

        private static System.ServiceModel.MsmqIntegration.ActiveXSerializer ActiveXSerializer
        {
            get
            {
                if (activeXSerializer == null)
                {
                    activeXSerializer = new System.ServiceModel.MsmqIntegration.ActiveXSerializer();
                }
                return activeXSerializer;
            }
        }

        private static System.Runtime.Serialization.Formatters.Binary.BinaryFormatter BinaryFormatter
        {
            get
            {
                if (binaryFormatter == null)
                {
                    binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                }
                return binaryFormatter;
            }
        }
    }
}

