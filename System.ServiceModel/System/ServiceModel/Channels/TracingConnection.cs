namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.Threading;

    internal class TracingConnection : DelegatingConnection
    {
        private ServiceModelActivity activity;
        private static System.Threading.WaitCallback callback;

        public TracingConnection(IConnection connection, bool inheritCurrentActivity) : base(connection)
        {
            this.activity = inheritCurrentActivity ? ServiceModelActivity.CreateActivity(DiagnosticTrace.ActivityId, false) : ServiceModelActivity.CreateActivity();
            if ((DiagnosticUtility.ShouldUseActivity && !inheritCurrentActivity) && (FxTrace.Trace != null))
            {
                FxTrace.Trace.TraceTransfer(this.activity.Id);
            }
        }

        public TracingConnection(IConnection connection, ServiceModelActivity activity) : base(connection)
        {
            this.activity = activity;
        }

        public override void Abort()
        {
            try
            {
                using (ServiceModelActivity.BoundOperation(this.activity))
                {
                    base.Abort();
                }
            }
            finally
            {
                if (this.activity != null)
                {
                    this.activity.Dispose();
                }
            }
        }

        internal void ActivityStart(string name)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                ServiceModelActivity.Start(this.activity, System.ServiceModel.SR.GetString("ActivityReceiveBytes", new object[] { name }), ActivityType.ReceiveBytes);
            }
        }

        internal void ActivityStart(Uri uri)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                ServiceModelActivity.Start(this.activity, System.ServiceModel.SR.GetString("ActivityReceiveBytes", new object[] { uri.ToString() }), ActivityType.ReceiveBytes);
            }
        }

        public override AsyncReadResult BeginRead(int offset, int size, TimeSpan timeout, System.Threading.WaitCallback callback, object state)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                TracingConnectionState state2 = new TracingConnectionState(callback, this.activity, state);
                return base.BeginRead(offset, size, timeout, Callback, state2);
            }
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout, AsyncCallback callback, object state)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                return base.BeginWrite(buffer, offset, size, immediate, timeout, callback, state);
            }
        }

        public override void Close(TimeSpan timeout, bool asyncAndLinger)
        {
            try
            {
                using (ServiceModelActivity.BoundOperation(this.activity, true))
                {
                    base.Close(timeout, asyncAndLinger);
                }
            }
            finally
            {
                if (this.activity != null)
                {
                    this.activity.Dispose();
                }
            }
        }

        public override object DuplicateAndClose(int targetProcessId)
        {
            using (ServiceModelActivity.BoundOperation(this.activity, true))
            {
                return base.DuplicateAndClose(targetProcessId);
            }
        }

        public override int EndRead()
        {
            int num = 0;
            try
            {
                if (this.activity != null)
                {
                    ExceptionUtility.UseActivityId(this.activity.Id);
                }
                num = base.EndRead();
            }
            finally
            {
                ExceptionUtility.ClearActivityId();
            }
            return num;
        }

        public override void EndWrite(IAsyncResult result)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                base.EndWrite(result);
            }
        }

        public override int Read(byte[] buffer, int offset, int size, TimeSpan timeout)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                return base.Read(buffer, offset, size, timeout);
            }
        }

        public override void Shutdown(TimeSpan timeout)
        {
            using (ServiceModelActivity.BoundOperation(this.activity, true))
            {
                base.Shutdown(timeout);
            }
        }

        private static void WaitCallback(object state)
        {
            ((TracingConnectionState) state).ExecuteCallback();
        }

        public override void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                base.Write(buffer, offset, size, immediate, timeout);
            }
        }

        public override void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout, BufferManager bufferManager)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                base.Write(buffer, offset, size, immediate, timeout, bufferManager);
            }
        }

        private static System.Threading.WaitCallback Callback
        {
            get
            {
                if (callback == null)
                {
                    callback = new System.Threading.WaitCallback(TracingConnection.WaitCallback);
                }
                return callback;
            }
        }

        private class TracingConnectionState
        {
            private ServiceModelActivity activity;
            private WaitCallback callback;
            private object state;

            internal TracingConnectionState(WaitCallback callback, ServiceModelActivity activity, object state)
            {
                this.activity = activity;
                this.callback = callback;
                this.state = state;
            }

            internal void ExecuteCallback()
            {
                using (ServiceModelActivity.BoundOperation(this.activity))
                {
                    this.callback(this.state);
                }
            }
        }
    }
}

