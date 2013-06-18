namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal abstract class LayeredChannelListener<TChannel> : ChannelListenerBase<TChannel> where TChannel: class, IChannel
    {
        private IChannelListener innerChannelListener;
        private EventHandler onInnerListenerFaulted;
        private bool sharedInnerListener;

        protected LayeredChannelListener(bool sharedInnerListener) : this(sharedInnerListener, null, null)
        {
        }

        protected LayeredChannelListener(bool sharedInnerListener, IDefaultCommunicationTimeouts timeouts) : this(sharedInnerListener, timeouts, null)
        {
        }

        protected LayeredChannelListener(IDefaultCommunicationTimeouts timeouts, IChannelListener innerChannelListener) : this(false, timeouts, innerChannelListener)
        {
        }

        protected LayeredChannelListener(bool sharedInnerListener, IDefaultCommunicationTimeouts timeouts, IChannelListener innerChannelListener) : base(timeouts)
        {
            this.sharedInnerListener = sharedInnerListener;
            this.innerChannelListener = innerChannelListener;
            this.onInnerListenerFaulted = new EventHandler(this.OnInnerListenerFaulted);
            if (this.innerChannelListener != null)
            {
                this.innerChannelListener.Faulted += this.onInnerListenerFaulted;
            }
        }

        internal IChannelListener GetInnerListenerSnapshot()
        {
            IChannelListener innerChannelListener = this.InnerChannelListener;
            if (innerChannelListener == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InnerListenerFactoryNotSet", new object[] { base.GetType().ToString() })));
            }
            return innerChannelListener;
        }

        public override T GetProperty<T>() where T: class
        {
            T property = base.GetProperty<T>();
            if (property != null)
            {
                return property;
            }
            IChannelListener innerChannelListener = this.InnerChannelListener;
            if (innerChannelListener != null)
            {
                return innerChannelListener.GetProperty<T>();
            }
            return default(T);
        }

        protected override void OnAbort()
        {
            lock (base.ThisLock)
            {
                this.OnCloseOrAbort();
            }
            IChannelListener innerChannelListener = this.InnerChannelListener;
            if ((innerChannelListener != null) && !this.sharedInnerListener)
            {
                innerChannelListener.Abort();
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnCloseOrAbort();
            return new CloseAsyncResult<TChannel>(this.InnerChannelListener, this.sharedInnerListener, timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenAsyncResult<TChannel>(this.InnerChannelListener, this.sharedInnerListener, timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.OnCloseOrAbort();
            if ((this.InnerChannelListener != null) && !this.sharedInnerListener)
            {
                this.InnerChannelListener.Close(timeout);
            }
        }

        private void OnCloseOrAbort()
        {
            IChannelListener innerChannelListener = this.InnerChannelListener;
            if (innerChannelListener != null)
            {
                innerChannelListener.Faulted -= this.onInnerListenerFaulted;
            }
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CloseAsyncResult<TChannel>.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            OpenAsyncResult<TChannel>.End(result);
        }

        private void OnInnerListenerFaulted(object sender, EventArgs e)
        {
            base.Fault();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            if ((this.InnerChannelListener != null) && !this.sharedInnerListener)
            {
                this.InnerChannelListener.Open(timeout);
            }
        }

        protected override void OnOpening()
        {
            base.OnOpening();
            this.ThrowIfInnerListenerNotSet();
        }

        internal void ThrowIfInnerListenerNotSet()
        {
            if (this.InnerChannelListener == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InnerListenerFactoryNotSet", new object[] { base.GetType().ToString() })));
            }
        }

        internal virtual IChannelListener InnerChannelListener
        {
            get
            {
                return this.innerChannelListener;
            }
            set
            {
                lock (base.ThisLock)
                {
                    base.ThrowIfDisposedOrImmutable();
                    if (this.innerChannelListener != null)
                    {
                        this.innerChannelListener.Faulted -= this.onInnerListenerFaulted;
                    }
                    this.innerChannelListener = value;
                    if (this.innerChannelListener != null)
                    {
                        this.innerChannelListener.Faulted += this.onInnerListenerFaulted;
                    }
                }
            }
        }

        internal bool SharedInnerListener
        {
            get
            {
                return this.sharedInnerListener;
            }
        }

        public override System.Uri Uri
        {
            get
            {
                return this.GetInnerListenerSnapshot().Uri;
            }
        }

        private class CloseAsyncResult : AsyncResult
        {
            private ICommunicationObject communicationObject;
            private static AsyncCallback onCloseComplete;

            static CloseAsyncResult()
            {
                LayeredChannelListener<TChannel>.CloseAsyncResult.onCloseComplete = Fx.ThunkCallback(new AsyncCallback(LayeredChannelListener<TChannel>.CloseAsyncResult.OnCloseComplete));
            }

            public CloseAsyncResult(ICommunicationObject communicationObject, bool sharedInnerListener, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.communicationObject = communicationObject;
                if ((this.communicationObject == null) || sharedInnerListener)
                {
                    base.Complete(true);
                }
                else
                {
                    IAsyncResult result = this.communicationObject.BeginClose(timeout, LayeredChannelListener<TChannel>.CloseAsyncResult.onCloseComplete, this);
                    if (result.CompletedSynchronously)
                    {
                        this.communicationObject.EndClose(result);
                        base.Complete(true);
                    }
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<LayeredChannelListener<TChannel>.CloseAsyncResult>(result);
            }

            private static void OnCloseComplete(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    LayeredChannelListener<TChannel>.CloseAsyncResult asyncState = (LayeredChannelListener<TChannel>.CloseAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.communicationObject.EndClose(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    asyncState.Complete(false, exception);
                }
            }
        }

        private class OpenAsyncResult : AsyncResult
        {
            private ICommunicationObject communicationObject;
            private static AsyncCallback onOpenComplete;

            static OpenAsyncResult()
            {
                LayeredChannelListener<TChannel>.OpenAsyncResult.onOpenComplete = Fx.ThunkCallback(new AsyncCallback(LayeredChannelListener<TChannel>.OpenAsyncResult.OnOpenComplete));
            }

            public OpenAsyncResult(ICommunicationObject communicationObject, bool sharedInnerListener, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.communicationObject = communicationObject;
                if ((this.communicationObject == null) || sharedInnerListener)
                {
                    base.Complete(true);
                }
                else
                {
                    IAsyncResult result = this.communicationObject.BeginOpen(timeout, LayeredChannelListener<TChannel>.OpenAsyncResult.onOpenComplete, this);
                    if (result.CompletedSynchronously)
                    {
                        this.communicationObject.EndOpen(result);
                        base.Complete(true);
                    }
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<LayeredChannelListener<TChannel>.OpenAsyncResult>(result);
            }

            private static void OnOpenComplete(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    LayeredChannelListener<TChannel>.OpenAsyncResult asyncState = (LayeredChannelListener<TChannel>.OpenAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.communicationObject.EndOpen(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    asyncState.Complete(false, exception);
                }
            }
        }
    }
}

