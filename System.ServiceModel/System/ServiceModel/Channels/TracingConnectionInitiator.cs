namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel.Diagnostics;

    internal class TracingConnectionInitiator : IConnectionInitiator
    {
        private ServiceModelActivity activity;
        private Uri connectedUri;
        private IConnectionInitiator connectionInitiator;
        private bool isClient;

        internal TracingConnectionInitiator(IConnectionInitiator connectionInitiator, bool isClient)
        {
            this.connectionInitiator = connectionInitiator;
            this.activity = ServiceModelActivity.CreateActivity(DiagnosticTrace.ActivityId);
            this.isClient = isClient;
        }

        public IAsyncResult BeginConnect(Uri uri, TimeSpan timeout, AsyncCallback callback, object state)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                this.connectedUri = uri;
                return this.connectionInitiator.BeginConnect(uri, timeout, callback, state);
            }
        }

        public IConnection Connect(Uri uri, TimeSpan timeout)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                IConnection connection = this.connectionInitiator.Connect(uri, timeout);
                if (!this.isClient)
                {
                    TracingConnection connection2 = new TracingConnection(connection, false);
                    connection2.ActivityStart(uri);
                    connection = connection2;
                }
                return connection;
            }
        }

        public IConnection EndConnect(IAsyncResult result)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                TracingConnection connection = new TracingConnection(this.connectionInitiator.EndConnect(result), false);
                connection.ActivityStart(this.connectedUri);
                return connection;
            }
        }
    }
}

