namespace System.ServiceModel.MsmqIntegration
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security.Tokens;

    internal sealed class MsmqIntegrationOutputChannel : TransportOutputChannel
    {
        private SecurityTokenProviderContainer certificateTokenProvider;
        private MsmqIntegrationChannelFactory factory;
        private MsmqQueue msmqQueue;
        private MsmqTransactionMode transactionMode;

        public MsmqIntegrationOutputChannel(MsmqIntegrationChannelFactory factory, EndpointAddress to, Uri via, bool manualAddressing) : base(factory, to, via, manualAddressing, factory.MessageVersion)
        {
            this.factory = factory;
            if (factory.IsMsmqX509SecurityConfigured)
            {
                this.certificateTokenProvider = factory.CreateX509TokenProvider(to, via);
            }
        }

        private void CloseQueue()
        {
            if (this.msmqQueue != null)
            {
                this.msmqQueue.Dispose();
            }
            this.msmqQueue = null;
        }

        protected override void OnAbort()
        {
            this.OnCloseCore(true, TimeSpan.Zero);
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnCloseCore(false, timeout);
            return new CompletedAsyncResult(callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnOpenCore(timeout);
            return new CompletedAsyncResult(callback, state);
        }

        protected override IAsyncResult OnBeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnSend(message, timeout);
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.OnCloseCore(false, timeout);
        }

        private void OnCloseCore(bool isAborting, TimeSpan timeout)
        {
            this.CloseQueue();
            if (this.certificateTokenProvider != null)
            {
                if (isAborting)
                {
                    this.certificateTokenProvider.Abort();
                }
                else
                {
                    this.certificateTokenProvider.Close(timeout);
                }
            }
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnEndSend(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.OnOpenCore(timeout);
        }

        private void OnOpenCore(TimeSpan timeout)
        {
            this.OpenQueue();
            if (this.certificateTokenProvider != null)
            {
                this.certificateTokenProvider.Open(timeout);
            }
        }

        protected override void OnSend(Message message, TimeSpan timeout)
        {
            int length;
            MessageProperties properties = message.Properties;
            Stream stream = null;
            MsmqIntegrationMessageProperty property = MsmqIntegrationMessageProperty.Get(message);
            if (property == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("MsmqMessageDoesntHaveIntegrationProperty")));
            }
            if (property.Body != null)
            {
                stream = this.factory.Serialize(property);
            }
            if (stream == null)
            {
                length = 0;
            }
            else
            {
                if (stream.Length > 0x7fffffffL)
                {
                    throw TraceUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("MessageSizeMustBeInIntegerRange")), message);
                }
                length = (int) stream.Length;
            }
            using (MsmqIntegrationOutputMessage message2 = new MsmqIntegrationOutputMessage(this.factory, length, this.RemoteAddress, property))
            {
                message2.ApplyCertificateIfNeeded(this.certificateTokenProvider, this.factory.MsmqTransportSecurity.MsmqAuthenticationMode, timeout);
                if (stream != null)
                {
                    int num3;
                    stream.Position = 0L;
                    for (int i = length; i > 0; i -= num3)
                    {
                        num3 = stream.Read(message2.Body.Buffer, 0, i);
                    }
                }
                bool lockHeld = false;
                try
                {
                    Msmq.EnterXPSendLock(out lockHeld, this.factory.MsmqTransportSecurity.MsmqProtectionLevel);
                    this.msmqQueue.Send(message2, this.transactionMode);
                    MsmqDiagnostics.DatagramSent(message2.MessageId, message);
                    property.Id = MsmqMessageId.ToString(message2.MessageId.Buffer);
                }
                catch (MsmqException exception)
                {
                    if (exception.FaultSender)
                    {
                        base.Fault();
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.Normalized);
                }
                finally
                {
                    if (lockHeld)
                    {
                        Msmq.LeaveXPSendLock();
                    }
                }
            }
        }

        private void OpenQueue()
        {
            try
            {
                this.msmqQueue = new MsmqQueue(this.factory.AddressTranslator.UriToFormatName(this.RemoteAddress.Uri), 2);
            }
            catch (MsmqException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.Normalized);
            }
            if (this.factory.ExactlyOnce)
            {
                this.transactionMode = MsmqTransactionMode.CurrentOrSingle;
            }
            else
            {
                this.transactionMode = MsmqTransactionMode.None;
            }
        }

        private class MsmqIntegrationOutputMessage : MsmqOutputMessage<IOutputChannel>
        {
            private NativeMsmqMessage.ByteProperty acknowledge;
            private NativeMsmqMessage.StringProperty adminQueue;
            private NativeMsmqMessage.IntProperty appSpecific;
            private NativeMsmqMessage.BufferProperty correlationId;
            private NativeMsmqMessage.BufferProperty extension;
            private NativeMsmqMessage.StringProperty label;
            private NativeMsmqMessage.ByteProperty priority;
            private NativeMsmqMessage.StringProperty responseQueue;

            public MsmqIntegrationOutputMessage(MsmqChannelFactoryBase<IOutputChannel> factory, int bodySize, EndpointAddress remoteAddress, MsmqIntegrationMessageProperty property) : base(factory, bodySize, remoteAddress, 8)
            {
                if (property.AcknowledgeType.HasValue)
                {
                    this.EnsureAcknowledgeProperty((byte) property.AcknowledgeType.Value);
                }
                if (null != property.AdministrationQueue)
                {
                    this.EnsureAdminQueueProperty(property.AdministrationQueue, false);
                }
                if (property.AppSpecific.HasValue)
                {
                    this.appSpecific = new NativeMsmqMessage.IntProperty(this, 8, property.AppSpecific.Value);
                }
                if (property.BodyType.HasValue)
                {
                    base.EnsureBodyTypeProperty(property.BodyType.Value);
                }
                if (property.CorrelationId != null)
                {
                    this.correlationId = new NativeMsmqMessage.BufferProperty(this, 3, MsmqMessageId.FromString(property.CorrelationId));
                }
                if (property.Extension != null)
                {
                    this.extension = new NativeMsmqMessage.BufferProperty(this, 0x23, property.Extension);
                }
                if (property.Label != null)
                {
                    this.label = new NativeMsmqMessage.StringProperty(this, 11, property.Label);
                }
                if (property.Priority.HasValue)
                {
                    this.priority = new NativeMsmqMessage.ByteProperty(this, 4, (byte) property.Priority.Value);
                }
                if (null != property.ResponseQueue)
                {
                    this.EnsureResponseQueueProperty(property.ResponseQueue);
                }
                if (property.TimeToReachQueue.HasValue)
                {
                    base.EnsureTimeToReachQueueProperty(MsmqDuration.FromTimeSpan(property.TimeToReachQueue.Value));
                }
            }

            private void EnsureAcknowledgeProperty(byte value)
            {
                if (this.acknowledge == null)
                {
                    this.acknowledge = new NativeMsmqMessage.ByteProperty(this, 6);
                }
                this.acknowledge.Value = value;
            }

            private void EnsureAdminQueueProperty(Uri value, bool useNetMsmqTranslator)
            {
                if (null != value)
                {
                    string str = useNetMsmqTranslator ? MsmqUri.NetMsmqAddressTranslator.UriToFormatName(value) : MsmqUri.FormatNameAddressTranslator.UriToFormatName(value);
                    if (this.adminQueue == null)
                    {
                        this.adminQueue = new NativeMsmqMessage.StringProperty(this, 0x11, str);
                    }
                    else
                    {
                        this.adminQueue.SetValue(str);
                    }
                }
            }

            private void EnsureResponseQueueProperty(Uri value)
            {
                if (null != value)
                {
                    string str = MsmqUri.FormatNameAddressTranslator.UriToFormatName(value);
                    if (this.responseQueue == null)
                    {
                        this.responseQueue = new NativeMsmqMessage.StringProperty(this, 0x36, str);
                    }
                    else
                    {
                        this.responseQueue.SetValue(str);
                    }
                }
            }
        }
    }
}

