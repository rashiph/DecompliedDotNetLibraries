namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;

    internal abstract class TransportManager
    {
        private ServiceModelActivity activity;
        private int openCount;
        private object thisLock = new object();

        protected TransportManager()
        {
        }

        internal void Abort(TransportChannelListener channelListener)
        {
            this.Cleanup(channelListener, TimeSpan.Zero, true);
        }

        private void Cleanup(TransportChannelListener channelListener, TimeSpan timeout, bool aborting)
        {
            using (ServiceModelActivity.BoundOperation(this.Activity))
            {
                this.Unregister(channelListener);
            }
            lock (this.ThisLock)
            {
                if (this.openCount <= 0)
                {
                    throw Fx.AssertAndThrow("Invalid Open/Close state machine.");
                }
                this.openCount--;
                if (this.openCount == 0)
                {
                    using (ServiceModelActivity.BoundOperation(this.Activity, true))
                    {
                        if (aborting)
                        {
                            this.OnAbort();
                        }
                        else
                        {
                            this.OnClose(timeout);
                        }
                    }
                    if (this.Activity != null)
                    {
                        this.Activity.Dispose();
                    }
                }
            }
        }

        internal void Close(TransportChannelListener channelListener, TimeSpan timeout)
        {
            this.Cleanup(channelListener, timeout, false);
        }

        internal static void EnsureRegistered<TChannelListener>(UriPrefixTable<TChannelListener> addressTable, TChannelListener channelListener, HostNameComparisonMode registeredComparisonMode) where TChannelListener: TransportChannelListener
        {
            TChannelListener local;
            if (!addressTable.TryLookupUri(channelListener.Uri, registeredComparisonMode, out local) || (local != channelListener))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ListenerFactoryNotRegistered", new object[] { channelListener.Uri })));
            }
        }

        protected void Fault<TChannelListener>(UriPrefixTable<TChannelListener> addressTable, Exception exception) where TChannelListener: ChannelListenerBase
        {
            foreach (KeyValuePair<BaseUriWithWildcard, TChannelListener> pair in addressTable.GetAll())
            {
                TChannelListener local = pair.Value;
                local.Fault(exception);
                local.Abort();
            }
        }

        internal virtual void OnAbort()
        {
        }

        internal abstract void OnClose(TimeSpan timeout);
        internal abstract void OnOpen();
        internal void Open(TransportChannelListener channelListener)
        {
            if (DiagnosticUtility.ShouldUseActivity)
            {
                if (this.activity == null)
                {
                    this.activity = ServiceModelActivity.CreateActivity(true);
                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        if (FxTrace.Trace != null)
                        {
                            FxTrace.Trace.TraceTransfer(this.Activity.Id);
                        }
                        ServiceModelActivity.Start(this.Activity, System.ServiceModel.SR.GetString("ActivityListenAt", new object[] { channelListener.Uri.ToString() }), ActivityType.ListenAt);
                    }
                }
                channelListener.Activity = this.Activity;
            }
            using (ServiceModelActivity.BoundOperation(this.Activity))
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information, 0x80036, System.ServiceModel.SR.GetString("TraceCodeTransportListen", new object[] { channelListener.Uri.ToString() }), this);
                }
                this.Register(channelListener);
                try
                {
                    lock (this.ThisLock)
                    {
                        if (this.openCount == 0)
                        {
                            this.OnOpen();
                        }
                        this.openCount++;
                    }
                }
                catch
                {
                    this.Unregister(channelListener);
                    throw;
                }
            }
        }

        internal abstract void Register(TransportChannelListener channelListener);
        protected void ThrowIfOpen()
        {
            if (this.openCount > 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TransportManagerOpen")));
            }
        }

        internal abstract void Unregister(TransportChannelListener channelListener);

        protected ServiceModelActivity Activity
        {
            get
            {
                return this.activity;
            }
        }

        internal abstract string Scheme { get; }

        internal object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }
    }
}

