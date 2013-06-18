namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal abstract class MsmqInputChannelBase : ChannelBase, IInputChannel, IChannel, ICommunicationObject
    {
        private MsmqInputChannelListenerBase listener;
        private EndpointAddress localAddress;
        private MsmqReceiveContextLockManager receiveContextManager;
        private MsmqReceiveParameters receiveParameters;
        private System.ServiceModel.Channels.MsmqReceiveHelper receiver;

        public MsmqInputChannelBase(MsmqInputChannelListenerBase listener, IMsmqMessagePool messagePool) : base(listener)
        {
            this.receiveParameters = listener.ReceiveParameters;
            this.receiver = new System.ServiceModel.Channels.MsmqReceiveHelper(listener.ReceiveParameters, listener.Uri, messagePool, this, listener);
            this.localAddress = new EndpointAddress(listener.Uri, new AddressHeader[0]);
            this.listener = listener;
            if (this.receiveParameters.ReceiveContextSettings.Enabled)
            {
                this.receiveContextManager = new MsmqReceiveContextLockManager(this.receiveParameters.ReceiveContextSettings, this.receiver.Queue);
            }
        }

        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            return this.BeginReceive(base.DefaultReceiveTimeout, callback, state);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            using (MsmqDiagnostics.BoundReceiveOperation(this.receiver))
            {
                return InputChannel.HelpBeginReceive(this, timeout, callback, state);
            }
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (base.DoneReceivingInCurrentState())
            {
                return new DoneReceivingAsyncResult(callback, state);
            }
            MsmqInputMessage msmqMessage = this.receiver.TakeMessage();
            return this.receiver.BeginTryReceive(msmqMessage, timeout, this.ReceiveParameters.ExactlyOnce ? MsmqTransactionMode.CurrentOrNone : MsmqTransactionMode.None, callback, state);
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (base.DoneReceivingInCurrentState())
            {
                return new DoneReceivingAsyncResult(callback, state);
            }
            return this.receiver.BeginWaitForMessage(timeout, callback, state);
        }

        protected abstract Message DecodeMsmqMessage(MsmqInputMessage msmqMessage, MsmqMessageProperty property);
        public Message EndReceive(IAsyncResult result)
        {
            return InputChannel.HelpEndReceive(result);
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            bool flag2;
            message = null;
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            DoneReceivingAsyncResult result2 = result as DoneReceivingAsyncResult;
            if (result2 != null)
            {
                return DoneReceivingAsyncResult.End(result2);
            }
            MsmqInputMessage msmqMessage = null;
            MsmqMessageProperty msmqProperty = null;
            try
            {
                bool flag = this.receiver.EndTryReceive(result, out msmqMessage, out msmqProperty);
                if (flag)
                {
                    if (msmqProperty != null)
                    {
                        message = this.DecodeMsmqMessage(msmqMessage, msmqProperty);
                        message.Properties["MsmqMessageProperty"] = msmqProperty;
                        if (this.receiveParameters.ReceiveContextSettings.Enabled)
                        {
                            message.Properties[ReceiveContext.Name] = this.receiveContextManager.CreateMsmqReceiveContext(msmqMessage.LookupId.Value);
                        }
                        MsmqDiagnostics.DatagramReceived(msmqMessage.MessageId, message);
                        this.listener.RaiseMessageReceived();
                    }
                    else if (CommunicationState.Opened == base.State)
                    {
                        this.listener.FaultListener();
                        base.Fault();
                    }
                }
                flag2 = flag;
            }
            catch (MsmqException exception)
            {
                if (exception.FaultReceiver)
                {
                    this.listener.FaultListener();
                    base.Fault();
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.Normalized);
            }
            finally
            {
                if (msmqMessage != null)
                {
                    this.receiver.ReturnMessage(msmqMessage);
                }
            }
            return flag2;
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            bool flag;
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            DoneReceivingAsyncResult result2 = result as DoneReceivingAsyncResult;
            if (result2 != null)
            {
                return DoneReceivingAsyncResult.End(result2);
            }
            try
            {
                flag = this.receiver.EndWaitForMessage(result);
            }
            catch (MsmqException exception)
            {
                if (exception.FaultReceiver)
                {
                    this.listener.FaultListener();
                    base.Fault();
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.Normalized);
            }
            return flag;
        }

        internal void FaultChannel()
        {
            base.Fault();
        }

        protected override void OnAbort()
        {
            this.OnCloseCore(true);
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnCloseCore(false);
            return new CompletedAsyncResult(callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnOpenCore();
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.OnCloseCore(false);
        }

        protected virtual void OnCloseCore(bool isAborting)
        {
            this.receiver.Close();
            if (this.receiveContextManager != null)
            {
                this.receiveContextManager.Dispose();
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

        protected override void OnFaulted()
        {
            this.OnCloseCore(true);
            base.OnFaulted();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.OnOpenCore();
        }

        protected virtual void OnOpenCore()
        {
            try
            {
                this.receiver.Open();
            }
            catch (MsmqException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.Normalized);
            }
        }

        public Message Receive()
        {
            return this.Receive(base.DefaultReceiveTimeout);
        }

        public Message Receive(TimeSpan timeout)
        {
            return InputChannel.HelpReceive(this, timeout);
        }

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            bool flag2;
            message = null;
            if (base.DoneReceivingInCurrentState())
            {
                return true;
            }
            using (MsmqDiagnostics.BoundReceiveOperation(this.receiver))
            {
                MsmqInputMessage msmqMessage = this.receiver.TakeMessage();
                try
                {
                    MsmqMessageProperty property;
                    bool flag = this.receiver.TryReceive(msmqMessage, timeout, this.ReceiveParameters.ExactlyOnce ? MsmqTransactionMode.CurrentOrNone : MsmqTransactionMode.None, out property);
                    if (flag)
                    {
                        if (property != null)
                        {
                            message = this.DecodeMsmqMessage(msmqMessage, property);
                            message.Properties["MsmqMessageProperty"] = property;
                            if (this.receiveParameters.ReceiveContextSettings.Enabled)
                            {
                                message.Properties[ReceiveContext.Name] = this.receiveContextManager.CreateMsmqReceiveContext(msmqMessage.LookupId.Value);
                            }
                            MsmqDiagnostics.DatagramReceived(msmqMessage.MessageId, message);
                            this.listener.RaiseMessageReceived();
                        }
                        else if (CommunicationState.Opened == base.State)
                        {
                            this.listener.FaultListener();
                            base.Fault();
                        }
                    }
                    flag2 = flag;
                }
                catch (MsmqException exception)
                {
                    if (exception.FaultReceiver)
                    {
                        this.listener.FaultListener();
                        base.Fault();
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.Normalized);
                }
                finally
                {
                    this.receiver.ReturnMessage(msmqMessage);
                }
            }
            return flag2;
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            bool flag;
            if (base.DoneReceivingInCurrentState())
            {
                return true;
            }
            try
            {
                flag = this.receiver.WaitForMessage(timeout);
            }
            catch (MsmqException exception)
            {
                if (exception.FaultReceiver)
                {
                    this.listener.FaultListener();
                    base.Fault();
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.Normalized);
            }
            return flag;
        }

        public EndpointAddress LocalAddress
        {
            get
            {
                return this.localAddress;
            }
        }

        protected System.ServiceModel.Channels.MsmqReceiveHelper MsmqReceiveHelper
        {
            get
            {
                return this.receiver;
            }
        }

        protected MsmqReceiveParameters ReceiveParameters
        {
            get
            {
                return this.receiveParameters;
            }
        }
    }
}

