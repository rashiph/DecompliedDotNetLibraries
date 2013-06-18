namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class InstanceContextManager : LifetimeManager, IInstanceContextManager
    {
        private int firstFreeIndex;
        private Item[] items;

        public InstanceContextManager(object mutex) : base(mutex)
        {
        }

        public void Add(InstanceContext instanceContext)
        {
            bool flag = false;
            lock (base.ThisLock)
            {
                if (base.State == LifetimeState.Opened)
                {
                    if (instanceContext.InstanceContextManagerIndex != 0)
                    {
                        return;
                    }
                    if (this.firstFreeIndex == 0)
                    {
                        this.GrowItems();
                    }
                    this.AddItem(instanceContext);
                    base.IncrementBusyCountWithoutLock();
                    flag = true;
                }
            }
            if (!flag)
            {
                instanceContext.Abort();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(base.GetType().ToString()));
            }
        }

        private void AddItem(InstanceContext instanceContext)
        {
            int firstFreeIndex = this.firstFreeIndex;
            this.firstFreeIndex = this.items[firstFreeIndex].nextFreeIndex;
            this.items[firstFreeIndex].instanceContext = instanceContext;
            instanceContext.InstanceContextManagerIndex = firstFreeIndex;
        }

        public IAsyncResult BeginCloseInput(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseInputAsyncResult(timeout, callback, state, this.ToArray());
        }

        private void CloseInitiate(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            foreach (InstanceContext context in this.ToArray())
            {
                try
                {
                    if (context.State == CommunicationState.Opened)
                    {
                        IAsyncResult result = context.BeginClose(helper.RemainingTime(), Fx.ThunkCallback(new AsyncCallback(InstanceContextManager.CloseInstanceContextCallback)), context);
                        if (result.CompletedSynchronously)
                        {
                            context.EndClose(result);
                        }
                    }
                    else
                    {
                        context.Abort();
                    }
                }
                catch (ObjectDisposedException exception)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                }
                catch (InvalidOperationException exception2)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                    }
                }
                catch (CommunicationException exception3)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                    }
                }
                catch (TimeoutException exception4)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception4, TraceEventType.Information);
                    }
                }
            }
        }

        public void CloseInput(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            InstanceContext[] contextArray = this.ToArray();
            for (int i = 0; i < contextArray.Length; i++)
            {
                contextArray[i].CloseInput(helper.RemainingTime());
            }
        }

        private static void CloseInstanceContextCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                InstanceContext asyncState = (InstanceContext) result.AsyncState;
                try
                {
                    asyncState.EndClose(result);
                }
                catch (ObjectDisposedException exception)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                }
                catch (InvalidOperationException exception2)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                    }
                }
                catch (CommunicationException exception3)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                    }
                }
                catch (TimeoutException exception4)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception4, TraceEventType.Information);
                    }
                }
            }
        }

        public void EndCloseInput(IAsyncResult result)
        {
            CloseInputAsyncResult.End(result);
        }

        private void GrowItems()
        {
            Item[] items = this.items;
            if (items != null)
            {
                this.InitItems(items.Length * 2);
                for (int i = 1; i < items.Length; i++)
                {
                    this.AddItem(items[i].instanceContext);
                }
            }
            else
            {
                this.InitItems(4);
            }
        }

        private void InitItems(int count)
        {
            this.items = new Item[count];
            for (int i = count - 2; i > 0; i--)
            {
                this.items[i].nextFreeIndex = i + 1;
            }
            this.firstFreeIndex = 1;
        }

        protected override void OnAbort()
        {
            InstanceContext[] contextArray = this.ToArray();
            for (int i = 0; i < contextArray.Length; i++)
            {
                contextArray[i].Abort();
            }
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.CloseInitiate(helper.RemainingTime());
            return base.OnBeginClose(helper.RemainingTime(), callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.CloseInitiate(helper.RemainingTime());
            base.OnClose(helper.RemainingTime());
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            base.OnEndClose(result);
        }

        public bool Remove(InstanceContext instanceContext)
        {
            if (instanceContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("instanceContext"));
            }
            lock (base.ThisLock)
            {
                int instanceContextManagerIndex = instanceContext.InstanceContextManagerIndex;
                if (instanceContextManagerIndex == 0)
                {
                    return false;
                }
                instanceContext.InstanceContextManagerIndex = 0;
                this.items[instanceContextManagerIndex].nextFreeIndex = this.firstFreeIndex;
                this.items[instanceContextManagerIndex].instanceContext = null;
                this.firstFreeIndex = instanceContextManagerIndex;
            }
            base.DecrementBusyCount();
            return true;
        }

        public InstanceContext[] ToArray()
        {
            if (this.items == null)
            {
                return EmptyArray<InstanceContext>.Instance;
            }
            lock (base.ThisLock)
            {
                int num = 0;
                for (int i = 1; i < this.items.Length; i++)
                {
                    if (this.items[i].instanceContext != null)
                    {
                        num++;
                    }
                }
                if (num == 0)
                {
                    return EmptyArray<InstanceContext>.Instance;
                }
                InstanceContext[] contextArray = new InstanceContext[num];
                num = 0;
                for (int j = 1; j < this.items.Length; j++)
                {
                    InstanceContext instanceContext = this.items[j].instanceContext;
                    if (instanceContext != null)
                    {
                        contextArray[num++] = instanceContext;
                    }
                }
                return contextArray;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Item
        {
            public int nextFreeIndex;
            public InstanceContext instanceContext;
        }
    }
}

