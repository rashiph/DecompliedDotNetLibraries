namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.Threading;

    internal class SecurityListenerSettingsLifetimeManager
    {
        private IChannelListener innerListener;
        private int referenceCount;
        private SecurityProtocolFactory securityProtocolFactory;
        private bool sessionMode;
        private SecuritySessionServerSettings sessionSettings;

        public SecurityListenerSettingsLifetimeManager(SecurityProtocolFactory securityProtocolFactory, SecuritySessionServerSettings sessionSettings, bool sessionMode, IChannelListener innerListener)
        {
            this.securityProtocolFactory = securityProtocolFactory;
            this.sessionSettings = sessionSettings;
            this.sessionMode = sessionMode;
            this.innerListener = innerListener;
            this.referenceCount = 1;
        }

        public void Abort()
        {
            if (Interlocked.Decrement(ref this.referenceCount) == 0)
            {
                this.AbortCore();
            }
        }

        private void AbortCore()
        {
            if (this.securityProtocolFactory != null)
            {
                this.securityProtocolFactory.Close(true, TimeSpan.Zero);
            }
            if (this.sessionMode && (this.sessionSettings != null))
            {
                this.sessionSettings.Abort();
            }
            this.innerListener.Abort();
        }

        public void AddReference()
        {
            Interlocked.Increment(ref this.referenceCount);
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (Interlocked.Decrement(ref this.referenceCount) == 0)
            {
                bool flag = true;
                try
                {
                    List<OperationWithTimeoutBeginCallback> list = new List<OperationWithTimeoutBeginCallback>(3);
                    List<OperationEndCallback> list2 = new List<OperationEndCallback>(3);
                    if (this.securityProtocolFactory != null)
                    {
                        list.Add(new OperationWithTimeoutBeginCallback(this.securityProtocolFactory.BeginClose));
                        list2.Add(new OperationEndCallback(this.securityProtocolFactory.EndClose));
                    }
                    if (this.sessionMode && (this.sessionSettings != null))
                    {
                        list.Add(new OperationWithTimeoutBeginCallback(this.sessionSettings.BeginClose));
                        list2.Add(new OperationEndCallback(this.sessionSettings.EndClose));
                    }
                    list.Add(new OperationWithTimeoutBeginCallback(this.innerListener.BeginClose));
                    list2.Add(new OperationEndCallback(this.innerListener.EndClose));
                    IAsyncResult result = OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout, list.ToArray(), list2.ToArray(), callback, state);
                    flag = false;
                    return result;
                }
                finally
                {
                    if (flag)
                    {
                        this.AbortCore();
                    }
                }
            }
            return new DummyCloseAsyncResult(callback, state);
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            List<OperationWithTimeoutBeginCallback> list = new List<OperationWithTimeoutBeginCallback>(3);
            List<OperationEndCallback> list2 = new List<OperationEndCallback>(3);
            if (this.securityProtocolFactory != null)
            {
                list.Add(new OperationWithTimeoutBeginCallback(this.BeginOpenSecurityProtocolFactory));
                list2.Add(new OperationEndCallback(this.EndOpenSecurityProtocolFactory));
            }
            if (this.sessionMode && (this.sessionSettings != null))
            {
                list.Add(new OperationWithTimeoutBeginCallback(this.sessionSettings.BeginOpen));
                list2.Add(new OperationEndCallback(this.sessionSettings.EndOpen));
            }
            list.Add(new OperationWithTimeoutBeginCallback(this.innerListener.BeginOpen));
            list2.Add(new OperationEndCallback(this.innerListener.EndOpen));
            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout, list.ToArray(), list2.ToArray(), callback, state);
        }

        private IAsyncResult BeginOpenSecurityProtocolFactory(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.securityProtocolFactory.BeginOpen(false, timeout, callback, state);
        }

        public void Close(TimeSpan timeout)
        {
            if (Interlocked.Decrement(ref this.referenceCount) == 0)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                bool flag = true;
                try
                {
                    if (this.securityProtocolFactory != null)
                    {
                        this.securityProtocolFactory.Close(false, helper.RemainingTime());
                    }
                    if (this.sessionMode && (this.sessionSettings != null))
                    {
                        this.sessionSettings.Close(helper.RemainingTime());
                    }
                    this.innerListener.Close(helper.RemainingTime());
                    flag = false;
                }
                finally
                {
                    if (flag)
                    {
                        this.AbortCore();
                    }
                }
            }
        }

        public void EndClose(IAsyncResult result)
        {
            if (result is DummyCloseAsyncResult)
            {
                DummyCloseAsyncResult.End(result);
            }
            else
            {
                bool flag = true;
                try
                {
                    OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
                    flag = false;
                }
                finally
                {
                    if (flag)
                    {
                        this.AbortCore();
                    }
                }
            }
        }

        public void EndOpen(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
        }

        private void EndOpenSecurityProtocolFactory(IAsyncResult result)
        {
            this.securityProtocolFactory.EndOpen(result);
        }

        public void Open(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.securityProtocolFactory != null)
            {
                this.securityProtocolFactory.Open(false, helper.RemainingTime());
            }
            if (this.sessionMode && (this.sessionSettings != null))
            {
                this.sessionSettings.Open(helper.RemainingTime());
            }
            this.innerListener.Open(helper.RemainingTime());
        }

        private class DummyCloseAsyncResult : CompletedAsyncResult
        {
            public DummyCloseAsyncResult(AsyncCallback callback, object state) : base(callback, state)
            {
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<SecurityListenerSettingsLifetimeManager.DummyCloseAsyncResult>(result);
            }
        }
    }
}

