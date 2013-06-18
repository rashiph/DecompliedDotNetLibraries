namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;

    internal class TracingConnectionListener : IConnectionListener, IDisposable
    {
        private ServiceModelActivity activity;
        private IConnectionListener listener;

        internal TracingConnectionListener(IConnectionListener listener)
        {
            this.listener = listener;
            this.activity = ServiceModelActivity.CreateActivity(DiagnosticTrace.ActivityId, false);
        }

        internal TracingConnectionListener(IConnectionListener listener, string traceStartInfo) : this(listener, traceStartInfo, true)
        {
        }

        internal TracingConnectionListener(IConnectionListener listener, Uri uri) : this(listener, uri.ToString())
        {
        }

        internal TracingConnectionListener(IConnectionListener listener, string traceStartInfo, bool newActivity)
        {
            this.listener = listener;
            if (newActivity)
            {
                this.activity = ServiceModelActivity.CreateActivity();
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    if (FxTrace.Trace != null)
                    {
                        FxTrace.Trace.TraceTransfer(this.activity.Id);
                    }
                    ServiceModelActivity.Start(this.activity, System.ServiceModel.SR.GetString("ActivityListenAt", new object[] { traceStartInfo }), ActivityType.ListenAt);
                }
            }
            else
            {
                this.activity = ServiceModelActivity.CreateActivity(DiagnosticTrace.ActivityId, false);
                if (this.activity != null)
                {
                    this.activity.Name = traceStartInfo;
                }
            }
        }

        public IAsyncResult BeginAccept(AsyncCallback callback, object state)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                return this.listener.BeginAccept(callback, state);
            }
        }

        public void Dispose()
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                this.listener.Dispose();
                this.activity.Dispose();
            }
        }

        public IConnection EndAccept(IAsyncResult result)
        {
            IConnection connection3;
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                ServiceModelActivity activity = ServiceModelActivity.CreateActivity();
                if ((activity != null) && (FxTrace.Trace != null))
                {
                    FxTrace.Trace.TraceTransfer(activity.Id);
                }
                using (ServiceModelActivity.BoundOperation(activity))
                {
                    ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityReceiveBytes", new object[] { this.activity.Name }), ActivityType.ReceiveBytes);
                    IConnection connection = this.listener.EndAccept(result);
                    if (connection == null)
                    {
                        return null;
                    }
                    TracingConnection connection2 = new TracingConnection(connection, activity);
                    connection3 = connection2;
                }
            }
            return connection3;
        }

        public void Listen()
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                this.listener.Listen();
            }
        }
    }
}

