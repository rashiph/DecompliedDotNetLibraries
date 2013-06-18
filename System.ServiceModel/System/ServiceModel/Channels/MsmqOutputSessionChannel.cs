namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Security.Tokens;
    using System.Transactions;

    internal sealed class MsmqOutputSessionChannel : TransportOutputChannel, IOutputSessionChannel, IOutputChannel, IChannel, ICommunicationObject, ISessionChannel<IOutputSession>
    {
        private Transaction associatedTx;
        private List<ArraySegment<byte>> buffers;
        private SecurityTokenProviderContainer certificateTokenProvider;
        private MessageEncoder encoder;
        private MsmqChannelFactory<IOutputSessionChannel> factory;
        private MsmqQueue msmqQueue;
        private IOutputSession session;

        public MsmqOutputSessionChannel(MsmqChannelFactory<IOutputSessionChannel> factory, EndpointAddress to, Uri via, bool manualAddressing) : base(factory, to, via, manualAddressing, factory.MessageVersion)
        {
            this.factory = factory;
            this.encoder = this.factory.MessageEncoderFactory.CreateSessionEncoder();
            this.buffers = new List<ArraySegment<byte>>();
            this.buffers.Add(this.EncodeSessionPreamble());
            if (factory.IsMsmqX509SecurityConfigured)
            {
                this.certificateTokenProvider = factory.CreateX509TokenProvider(to, via);
            }
            this.session = new OutputSession();
        }

        private int CalcSessionGramSize()
        {
            long num = 0L;
            for (int i = 0; i < this.buffers.Count; i++)
            {
                ArraySegment<byte> segment = this.buffers[i];
                num += segment.Count;
            }
            if (num > 0x7fffffffL)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqSessionGramSizeMustBeInIntegerRange")));
            }
            return (int) num;
        }

        private void CopySessionGramToBuffer(byte[] sessionGramBuffer)
        {
            int dstOffset = 0;
            for (int i = 0; i < this.buffers.Count; i++)
            {
                ArraySegment<byte> segment = this.buffers[i];
                Buffer.BlockCopy(segment.Array, segment.Offset, sessionGramBuffer, dstOffset, segment.Count);
                dstOffset += segment.Count;
            }
        }

        private ArraySegment<byte> EncodeEndMarker()
        {
            return new ArraySegment<byte>(SessionEncoder.EndBytes, 0, SessionEncoder.EndBytes.Length);
        }

        private ArraySegment<byte> EncodeMessage(Message message)
        {
            return SessionEncoder.EncodeMessageFrame(this.encoder.WriteMessage(message, 0x7fffffff, this.Factory.BufferManager, 6));
        }

        private ArraySegment<byte> EncodeSessionPreamble()
        {
            EncodedVia via = new EncodedVia(this.Via.AbsoluteUri);
            EncodedContentType contentType = EncodedContentType.Create(this.encoder.ContentType);
            int bufferSize = (ClientSimplexEncoder.ModeBytes.Length + SessionEncoder.CalcStartSize(via, contentType)) + SessionEncoder.PreambleEndBytes.Length;
            byte[] dst = this.Factory.BufferManager.TakeBuffer(bufferSize);
            Buffer.BlockCopy(ClientSimplexEncoder.ModeBytes, 0, dst, 0, ClientSimplexEncoder.ModeBytes.Length);
            SessionEncoder.EncodeStart(dst, ClientSimplexEncoder.ModeBytes.Length, via, contentType);
            Buffer.BlockCopy(SessionEncoder.PreambleEndBytes, 0, dst, bufferSize - SessionEncoder.PreambleEndBytes.Length, SessionEncoder.PreambleEndBytes.Length);
            return new ArraySegment<byte>(dst, 0, bufferSize);
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
            if (!isAborting && (this.buffers.Count > 1))
            {
                lock (base.ThisLock)
                {
                    this.VerifyTransaction();
                    this.buffers.Add(this.EncodeEndMarker());
                }
                int bodySize = this.CalcSessionGramSize();
                using (MsmqOutputMessage<IOutputSessionChannel> message = new MsmqOutputMessage<IOutputSessionChannel>(this.Factory, bodySize, this.RemoteAddress))
                {
                    message.ApplyCertificateIfNeeded(this.certificateTokenProvider, this.factory.MsmqTransportSecurity.MsmqAuthenticationMode, timeout);
                    message.Body.EnsureBufferLength(bodySize);
                    message.Body.BufferLength = bodySize;
                    this.CopySessionGramToBuffer(message.Body.Buffer);
                    bool lockHeld = false;
                    try
                    {
                        Msmq.EnterXPSendLock(out lockHeld, this.factory.MsmqTransportSecurity.MsmqProtectionLevel);
                        this.msmqQueue.Send(message, MsmqTransactionMode.CurrentOrSingle);
                        MsmqDiagnostics.SessiongramSent(this.Session.Id, message.MessageId, this.buffers.Count);
                    }
                    catch (MsmqException exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.Normalized);
                    }
                    finally
                    {
                        if (lockHeld)
                        {
                            Msmq.LeaveXPSendLock();
                        }
                        this.ReturnSessionGramBuffers();
                    }
                }
            }
            if (this.msmqQueue != null)
            {
                this.msmqQueue.Dispose();
            }
            this.msmqQueue = null;
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
            if (null == Transaction.Current)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqTransactionCurrentRequired")));
            }
            this.associatedTx = Transaction.Current;
            this.associatedTx.EnlistVolatile(new TransactionEnlistment(this, this.associatedTx), EnlistmentOptions.None);
            this.msmqQueue = new MsmqQueue(this.Factory.AddressTranslator.UriToFormatName(this.RemoteAddress.Uri), 2);
            if (this.certificateTokenProvider != null)
            {
                this.certificateTokenProvider.Open(timeout);
            }
        }

        protected override void OnSend(Message message, TimeSpan timeout)
        {
            lock (base.ThisLock)
            {
                base.ThrowIfDisposed();
                this.VerifyTransaction();
                this.buffers.Add(this.EncodeMessage(message));
            }
        }

        private void ReturnSessionGramBuffers()
        {
            for (int i = 0; i < (this.buffers.Count - 1); i++)
            {
                ArraySegment<byte> segment = this.buffers[i];
                this.Factory.BufferManager.ReturnBuffer(segment.Array);
            }
        }

        private void VerifyTransaction()
        {
            if (this.associatedTx != Transaction.Current)
            {
                base.Fault();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqSameTransactionExpected")));
            }
            if (Transaction.Current.TransactionInformation.Status != TransactionStatus.Active)
            {
                base.Fault();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqTransactionNotActive")));
            }
        }

        private MsmqChannelFactory<IOutputSessionChannel> Factory
        {
            get
            {
                return this.factory;
            }
        }

        public IOutputSession Session
        {
            get
            {
                return this.session;
            }
        }

        private class OutputSession : IOutputSession, ISession
        {
            private string id = ("uuid:/session-gram/" + Guid.NewGuid());

            public string Id
            {
                get
                {
                    return this.id;
                }
            }
        }

        private class TransactionEnlistment : IEnlistmentNotification
        {
            private MsmqOutputSessionChannel channel;
            private Transaction transaction;

            public TransactionEnlistment(MsmqOutputSessionChannel channel, Transaction transaction)
            {
                this.channel = channel;
                this.transaction = transaction;
            }

            public void Commit(Enlistment enlistment)
            {
                enlistment.Done();
            }

            public void InDoubt(Enlistment enlistment)
            {
                enlistment.Done();
            }

            public void Prepare(PreparingEnlistment preparingEnlistment)
            {
                if (this.channel.State != CommunicationState.Closed)
                {
                    this.channel.Fault();
                    Exception e = DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqSessionChannelsMustBeClosed")));
                    preparingEnlistment.ForceRollback(e);
                }
                else
                {
                    preparingEnlistment.Prepared();
                }
            }

            public void Rollback(Enlistment enlistment)
            {
                this.channel.Fault();
                enlistment.Done();
            }
        }
    }
}

