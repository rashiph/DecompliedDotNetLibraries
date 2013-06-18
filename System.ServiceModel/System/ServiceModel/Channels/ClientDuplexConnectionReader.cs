namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    internal class ClientDuplexConnectionReader : SessionConnectionReader
    {
        private BufferManager bufferManager;
        private ClientFramingDuplexSessionChannel channel;
        private ClientDuplexDecoder decoder;
        private int maxBufferSize;
        private MessageEncoder messageEncoder;

        public ClientDuplexConnectionReader(ClientFramingDuplexSessionChannel channel, IConnection connection, ClientDuplexDecoder decoder, IConnectionOrientedTransportFactorySettings settings, MessageEncoder messageEncoder) : base(connection, null, 0, 0, null)
        {
            this.decoder = decoder;
            this.maxBufferSize = settings.MaxBufferSize;
            this.bufferManager = settings.BufferManager;
            this.messageEncoder = messageEncoder;
            this.channel = channel;
        }

        private static IDisposable CreateProcessActionActivity()
        {
            IDisposable disposable = null;
            if (!DiagnosticUtility.ShouldUseActivity || ((ServiceModelActivity.Current != null) && (ServiceModelActivity.Current.ActivityType == ActivityType.ProcessAction)))
            {
                return disposable;
            }
            if (((ServiceModelActivity.Current != null) && (ServiceModelActivity.Current.PreviousActivity != null)) && (ServiceModelActivity.Current.PreviousActivity.ActivityType == ActivityType.ProcessAction))
            {
                return ServiceModelActivity.BoundOperation(ServiceModelActivity.Current.PreviousActivity);
            }
            ServiceModelActivity activity = ServiceModelActivity.CreateBoundedActivity(true);
            ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityProcessingMessage", new object[] { TraceUtility.RetrieveMessageNumber() }), ActivityType.ProcessMessage);
            return activity;
        }

        protected override Message DecodeMessage(byte[] buffer, ref int offset, ref int size, ref bool isAtEOF, TimeSpan timeout)
        {
            while (size > 0)
            {
                int envelopeSize;
                int count = this.decoder.Decode(buffer, offset, size);
                if (count > 0)
                {
                    if (base.EnvelopeBuffer != null)
                    {
                        if (!object.ReferenceEquals(buffer, base.EnvelopeBuffer))
                        {
                            Buffer.BlockCopy(buffer, offset, base.EnvelopeBuffer, base.EnvelopeOffset, count);
                        }
                        base.EnvelopeOffset += count;
                    }
                    offset += count;
                    size -= count;
                }
                switch (this.decoder.CurrentState)
                {
                    case ClientFramingDecoderState.EnvelopeStart:
                        envelopeSize = this.decoder.EnvelopeSize;
                        if (envelopeSize > this.maxBufferSize)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(MaxMessageSizeStream.CreateMaxReceivedMessageSizeExceededException((long) this.maxBufferSize));
                        }
                        break;

                    case ClientFramingDecoderState.ReadingEnvelopeBytes:
                    case ClientFramingDecoderState.ReadingEndRecord:
                    {
                        continue;
                    }
                    case ClientFramingDecoderState.EnvelopeEnd:
                    {
                        if (base.EnvelopeBuffer == null)
                        {
                            continue;
                        }
                        Message message = null;
                        try
                        {
                            using (CreateProcessActionActivity())
                            {
                                message = this.messageEncoder.ReadMessage(new ArraySegment<byte>(base.EnvelopeBuffer, 0, base.EnvelopeSize), this.bufferManager);
                                if (DiagnosticUtility.ShouldUseActivity)
                                {
                                    TraceUtility.TransferFromTransport(message);
                                }
                            }
                        }
                        catch (XmlException exception)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("MessageXmlProtocolError"), exception));
                        }
                        base.EnvelopeBuffer = null;
                        return message;
                    }
                    case ClientFramingDecoderState.End:
                        isAtEOF = true;
                        return null;

                    case ClientFramingDecoderState.Fault:
                        this.channel.Session.CloseOutputSession(this.channel.InternalCloseTimeout);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(FaultStringDecoder.GetFaultException(this.decoder.Fault, this.channel.RemoteAddress.Uri.ToString(), this.messageEncoder.ContentType));

                    default:
                    {
                        continue;
                    }
                }
                base.EnvelopeBuffer = this.bufferManager.TakeBuffer(envelopeSize);
                base.EnvelopeOffset = 0;
                base.EnvelopeSize = envelopeSize;
            }
            return null;
        }

        protected override void EnsureDecoderAtEof()
        {
            if (((this.decoder.CurrentState != ClientFramingDecoderState.End) && (this.decoder.CurrentState != ClientFramingDecoderState.EnvelopeEnd)) && ((this.decoder.CurrentState != ClientFramingDecoderState.ReadingUpgradeRecord) && (this.decoder.CurrentState != ClientFramingDecoderState.UpgradeResponse)))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.decoder.CreatePrematureEOFException());
            }
        }
    }
}

