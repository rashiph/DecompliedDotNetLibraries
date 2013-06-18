namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal class TransportManagerContainer
    {
        private bool closed;
        private TransportChannelListener listener;
        private object tableLock;
        private IList<TransportManager> transportManagers;

        public TransportManagerContainer(TransportChannelListener listener)
        {
            this.listener = listener;
            this.tableLock = listener.TransportManagerTable;
            this.transportManagers = new List<TransportManager>();
        }

        private TransportManagerContainer(TransportManagerContainer source)
        {
            this.listener = source.listener;
            this.tableLock = source.tableLock;
            this.transportManagers = new List<TransportManager>();
            for (int i = 0; i < source.transportManagers.Count; i++)
            {
                this.transportManagers.Add(source.transportManagers[i]);
            }
        }

        public void Abort()
        {
            this.Close(true, TimeSpan.Zero);
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseAsyncResult(this, callback, timeout, state);
        }

        public IAsyncResult BeginOpen(SelectTransportManagersCallback selectTransportManagerCallback, AsyncCallback callback, object state)
        {
            return new OpenAsyncResult(selectTransportManagerCallback, this, callback, state);
        }

        public void Close(TimeSpan timeout)
        {
            this.Close(false, timeout);
        }

        public void Close(bool aborting, TimeSpan timeout)
        {
            if (!this.closed)
            {
                lock (this.tableLock)
                {
                    if (!this.closed)
                    {
                        this.closed = true;
                        TimeoutHelper helper = new TimeoutHelper(timeout);
                        TimeoutException innerException = null;
                        foreach (TransportManager manager in this.transportManagers)
                        {
                            try
                            {
                                if (!aborting && (innerException == null))
                                {
                                    manager.Close(this.listener, helper.RemainingTime());
                                }
                                else
                                {
                                    manager.Abort(this.listener);
                                }
                            }
                            catch (TimeoutException exception2)
                            {
                                innerException = exception2;
                                manager.Abort(this.listener);
                            }
                        }
                        this.transportManagers.Clear();
                        if (innerException != null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("TimeoutOnClose", new object[] { timeout }), innerException));
                        }
                    }
                }
            }
        }

        public void EndClose(IAsyncResult result)
        {
            CloseAsyncResult.End(result);
        }

        public void EndOpen(IAsyncResult result)
        {
            OpenAsyncResult.End(result);
        }

        public void Open(SelectTransportManagersCallback selectTransportManagerCallback)
        {
            lock (this.tableLock)
            {
                if (!this.closed)
                {
                    IList<TransportManager> list = selectTransportManagerCallback();
                    if (list != null)
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            TransportManager item = list[i];
                            item.Open(this.listener);
                            this.transportManagers.Add(item);
                        }
                    }
                }
            }
        }

        public static TransportManagerContainer TransferTransportManagers(TransportManagerContainer source)
        {
            TransportManagerContainer container = null;
            lock (source.tableLock)
            {
                if (source.transportManagers.Count > 0)
                {
                    container = new TransportManagerContainer(source);
                    source.transportManagers.Clear();
                }
            }
            return container;
        }

        private sealed class CloseAsyncResult : TransportManagerContainer.OpenOrCloseAsyncResult
        {
            private TimeoutHelper timeoutHelper;

            public CloseAsyncResult(TransportManagerContainer parent, AsyncCallback callback, TimeSpan timeout, object state) : base(parent, callback, state)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.timeoutHelper.RemainingTime();
                base.Begin();
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<TransportManagerContainer.CloseAsyncResult>(result);
            }

            protected override void OnScheduled(TransportManagerContainer parent)
            {
                parent.Close(this.timeoutHelper.RemainingTime());
            }
        }

        private sealed class OpenAsyncResult : TransportManagerContainer.OpenOrCloseAsyncResult
        {
            private SelectTransportManagersCallback selectTransportManagerCallback;

            public OpenAsyncResult(SelectTransportManagersCallback selectTransportManagerCallback, TransportManagerContainer parent, AsyncCallback callback, object state) : base(parent, callback, state)
            {
                this.selectTransportManagerCallback = selectTransportManagerCallback;
                base.Begin();
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<TransportManagerContainer.OpenAsyncResult>(result);
            }

            protected override void OnScheduled(TransportManagerContainer parent)
            {
                parent.Open(this.selectTransportManagerCallback);
            }
        }

        private abstract class OpenOrCloseAsyncResult : TraceAsyncResult
        {
            private TransportManagerContainer parent;
            private static Action<object> scheduledCallback = new Action<object>(TransportManagerContainer.OpenOrCloseAsyncResult.OnScheduled);

            protected OpenOrCloseAsyncResult(TransportManagerContainer parent, AsyncCallback callback, object state) : base(callback, state)
            {
                this.parent = parent;
            }

            protected void Begin()
            {
                ActionItem.Schedule(scheduledCallback, this);
            }

            private void OnScheduled()
            {
                using (ServiceModelActivity.BoundOperation(base.CallbackActivity))
                {
                    Exception exception = null;
                    try
                    {
                        this.OnScheduled(this.parent);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    base.Complete(false, exception);
                }
            }

            private static void OnScheduled(object state)
            {
                ((TransportManagerContainer.OpenOrCloseAsyncResult) state).OnScheduled();
            }

            protected abstract void OnScheduled(TransportManagerContainer parent);
        }
    }
}

