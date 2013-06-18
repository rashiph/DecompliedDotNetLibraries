namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Security.Tokens;

    internal sealed class MsmqOutputChannel : TransportOutputChannel
    {
        private SecurityTokenProviderContainer certificateTokenProvider;
        private MsmqChannelFactory<IOutputChannel> factory;
        private MsmqQueue msmqQueue;
        private SynchronizedDisposablePool<MsmqOutputMessage<IOutputChannel>> outputMessages;
        private readonly byte[] preamble;
        private MsmqTransactionMode transactionMode;

        public MsmqOutputChannel(MsmqChannelFactory<IOutputChannel> factory, EndpointAddress to, Uri via, bool manualAddressing) : base(factory, to, via, manualAddressing, factory.MessageVersion)
        {
            byte[] modeBytes = ClientSingletonSizedEncoder.ModeBytes;
            EncodedVia via2 = new EncodedVia(this.Via.AbsoluteUri);
            EncodedContentType contentType = EncodedContentType.Create(factory.MessageEncoderFactory.Encoder.ContentType);
            this.preamble = DiagnosticUtility.Utility.AllocateByteArray(modeBytes.Length + ClientSingletonSizedEncoder.CalcStartSize(via2, contentType));
            Buffer.BlockCopy(modeBytes, 0, this.preamble, 0, modeBytes.Length);
            ClientSingletonSizedEncoder.EncodeStart(this.preamble, modeBytes.Length, via2, contentType);
            this.outputMessages = new SynchronizedDisposablePool<MsmqOutputMessage<IOutputChannel>>(factory.MaxPoolSize);
            if (factory.IsMsmqX509SecurityConfigured)
            {
                this.certificateTokenProvider = factory.CreateX509TokenProvider(to, via);
            }
            this.factory = factory;
        }

        private void CloseQueue()
        {
            this.outputMessages.Dispose();
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
            this.outputMessages.Dispose();
            if (this.factory.IsMsmqX509SecurityConfigured)
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
            if (this.factory.IsMsmqX509SecurityConfigured)
            {
                this.certificateTokenProvider.Open(timeout);
            }
        }

        protected override void OnSend(Message message, TimeSpan timeout)
        {
            ArraySegment<byte> segment = this.factory.MessageEncoderFactory.Encoder.WriteMessage(message, 0x7fffffff, this.factory.BufferManager, this.preamble.Length);
            Buffer.BlockCopy(this.preamble, 0, segment.Array, segment.Offset - this.preamble.Length, this.preamble.Length);
            byte[] array = segment.Array;
            int srcOffset = segment.Offset - this.preamble.Length;
            int bodySize = segment.Count + this.preamble.Length;
            MsmqOutputMessage<IOutputChannel> message2 = this.outputMessages.Take();
            if (message2 == null)
            {
                message2 = new MsmqOutputMessage<IOutputChannel>(this.factory, bodySize, this.RemoteAddress);
                MsmqDiagnostics.PoolFull(this.factory.MaxPoolSize);
            }
            try
            {
                message2.ApplyCertificateIfNeeded(this.certificateTokenProvider, this.factory.MsmqTransportSecurity.MsmqAuthenticationMode, timeout);
                message2.Body.EnsureBufferLength(bodySize);
                message2.Body.BufferLength = bodySize;
                Buffer.BlockCopy(array, srcOffset, message2.Body.Buffer, 0, bodySize);
                this.factory.BufferManager.ReturnBuffer(array);
                bool lockHeld = false;
                try
                {
                    Msmq.EnterXPSendLock(out lockHeld, this.factory.MsmqTransportSecurity.MsmqProtectionLevel);
                    this.msmqQueue.Send(message2, this.transactionMode);
                    MsmqDiagnostics.DatagramSent(message2.MessageId, message);
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
            finally
            {
                if (!this.outputMessages.Return(message2))
                {
                    message2.Dispose();
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
    }
}

