namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal abstract class InitialServerConnectionReader : IDisposable
    {
        private ConnectionClosedCallback closedCallback;
        private IConnection connection;
        private Action connectionDequeuedCallback;
        private bool isClosed;
        private int maxContentTypeSize;
        private int maxViaSize;

        protected InitialServerConnectionReader(IConnection connection, ConnectionClosedCallback closedCallback) : this(connection, closedCallback, 0x800, 0x100)
        {
        }

        protected InitialServerConnectionReader(IConnection connection, ConnectionClosedCallback closedCallback, int maxViaSize, int maxContentTypeSize)
        {
            if (connection == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("connection");
            }
            if (closedCallback == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("closedCallback");
            }
            this.connection = connection;
            this.closedCallback = closedCallback;
            this.maxContentTypeSize = maxContentTypeSize;
            this.maxViaSize = maxViaSize;
        }

        protected void Abort()
        {
            this.Abort(null);
        }

        protected void Abort(Exception e)
        {
            lock (this.ThisLock)
            {
                if (this.isClosed)
                {
                    return;
                }
                this.isClosed = true;
            }
            try
            {
                if ((e != null) && DiagnosticUtility.ShouldTraceError)
                {
                    TraceUtility.TraceEvent(TraceEventType.Error, 0x40026, System.ServiceModel.SR.GetString("TraceCodeChannelConnectionDropped"), this, e);
                }
                this.connection.Abort();
            }
            finally
            {
                if (this.closedCallback != null)
                {
                    this.closedCallback(this);
                }
                if (this.connectionDequeuedCallback != null)
                {
                    this.connectionDequeuedCallback();
                }
            }
        }

        public static IAsyncResult BeginUpgradeConnection(IConnection connection, StreamUpgradeAcceptor upgradeAcceptor, IDefaultCommunicationTimeouts defaultTimeouts, AsyncCallback callback, object state)
        {
            return new UpgradeConnectionAsyncResult(connection, upgradeAcceptor, defaultTimeouts, callback, state);
        }

        protected void Close(TimeSpan timeout)
        {
            lock (this.ThisLock)
            {
                if (this.isClosed)
                {
                    return;
                }
                this.isClosed = true;
            }
            bool flag = false;
            try
            {
                this.connection.Close(timeout, true);
                flag = true;
            }
            finally
            {
                if (!flag)
                {
                    this.connection.Abort();
                }
                if (this.closedCallback != null)
                {
                    this.closedCallback(this);
                }
                if (this.connectionDequeuedCallback != null)
                {
                    this.connectionDequeuedCallback();
                }
            }
        }

        public void CloseFromPool(TimeSpan timeout)
        {
            try
            {
                this.Close(timeout);
            }
            catch (CommunicationException exception)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
            }
            catch (TimeoutException exception2)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                }
            }
        }

        public void Dispose()
        {
            lock (this.ThisLock)
            {
                if (this.isClosed)
                {
                    return;
                }
                this.isClosed = true;
            }
            IConnection connection = this.connection;
            if (connection != null)
            {
                connection.Abort();
            }
            if (this.connectionDequeuedCallback != null)
            {
                this.connectionDequeuedCallback();
            }
        }

        public static IConnection EndUpgradeConnection(IAsyncResult result)
        {
            return UpgradeConnectionAsyncResult.End(result);
        }

        public Action GetConnectionDequeuedCallback()
        {
            Action connectionDequeuedCallback = this.connectionDequeuedCallback;
            this.connectionDequeuedCallback = null;
            return connectionDequeuedCallback;
        }

        public void ReleaseConnection()
        {
            this.isClosed = true;
            this.connection = null;
        }

        internal static void SendFault(IConnection connection, string faultString, byte[] drainBuffer, TimeSpan sendTimeout, int maxRead)
        {
            EncodedFault fault = new EncodedFault(faultString);
            TimeoutHelper helper = new TimeoutHelper(sendTimeout);
            try
            {
                connection.Write(fault.EncodedBytes, 0, fault.EncodedBytes.Length, true, helper.RemainingTime());
                connection.Shutdown(helper.RemainingTime());
            }
            catch (CommunicationException exception)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
                connection.Abort();
                return;
            }
            catch (TimeoutException exception2)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                }
                connection.Abort();
                return;
            }
            int num = 0;
            int num2 = 0;
            do
            {
                try
                {
                    num = connection.Read(drainBuffer, 0, drainBuffer.Length, helper.RemainingTime());
                }
                catch (CommunicationException exception3)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                    }
                    connection.Abort();
                    return;
                }
                catch (TimeoutException exception4)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception4, TraceEventType.Information);
                    }
                    connection.Abort();
                    return;
                }
                if (num == 0)
                {
                    ConnectionUtilities.CloseNoThrow(connection, helper.RemainingTime());
                    return;
                }
                num2 += num;
            }
            while ((num2 <= maxRead) && (helper.RemainingTime() > TimeSpan.Zero));
            connection.Abort();
        }

        public static IConnection UpgradeConnection(IConnection connection, StreamUpgradeAcceptor upgradeAcceptor, IDefaultCommunicationTimeouts defaultTimeouts)
        {
            ConnectionStream stream = new ConnectionStream(connection, defaultTimeouts);
            Stream stream2 = upgradeAcceptor.AcceptUpgrade(stream);
            if ((upgradeAcceptor is StreamSecurityUpgradeAcceptor) && DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x4002e, System.ServiceModel.SR.GetString("TraceCodeStreamSecurityUpgradeAccepted"), new StringTraceRecord("Type", upgradeAcceptor.GetType().ToString()), connection, null);
            }
            return new StreamConnection(stream2, stream);
        }

        public IConnection Connection
        {
            get
            {
                return this.connection;
            }
        }

        public Action ConnectionDequeuedCallback
        {
            get
            {
                return this.connectionDequeuedCallback;
            }
            set
            {
                this.connectionDequeuedCallback = value;
            }
        }

        protected bool IsClosed
        {
            get
            {
                return this.isClosed;
            }
        }

        protected int MaxContentTypeSize
        {
            get
            {
                return this.maxContentTypeSize;
            }
        }

        protected int MaxViaSize
        {
            get
            {
                return this.maxViaSize;
            }
        }

        private object ThisLock
        {
            get
            {
                return this;
            }
        }

        private class UpgradeConnectionAsyncResult : AsyncResult
        {
            private IConnection connection;
            private ConnectionStream connectionStream;
            private static AsyncCallback onAcceptUpgrade = Fx.ThunkCallback(new AsyncCallback(InitialServerConnectionReader.UpgradeConnectionAsyncResult.OnAcceptUpgrade));
            private StreamUpgradeAcceptor upgradeAcceptor;

            public UpgradeConnectionAsyncResult(IConnection connection, StreamUpgradeAcceptor upgradeAcceptor, IDefaultCommunicationTimeouts defaultTimeouts, AsyncCallback callback, object state) : base(callback, state)
            {
                this.upgradeAcceptor = upgradeAcceptor;
                this.connectionStream = new ConnectionStream(connection, defaultTimeouts);
                bool flag = false;
                IAsyncResult result = upgradeAcceptor.BeginAcceptUpgrade(this.connectionStream, onAcceptUpgrade, this);
                if (result.CompletedSynchronously)
                {
                    this.CompleteAcceptUpgrade(result);
                    flag = true;
                }
                if (flag)
                {
                    base.Complete(true);
                }
            }

            private void CompleteAcceptUpgrade(IAsyncResult result)
            {
                Stream stream;
                bool flag = false;
                try
                {
                    stream = this.upgradeAcceptor.EndAcceptUpgrade(result);
                    flag = true;
                }
                finally
                {
                    if (((this.upgradeAcceptor is StreamSecurityUpgradeAcceptor) && DiagnosticUtility.ShouldTraceInformation) && flag)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Information, 0x4002e, System.ServiceModel.SR.GetString("TraceCodeStreamSecurityUpgradeAccepted"), new StringTraceRecord("Type", this.upgradeAcceptor.GetType().ToString()), this, null);
                    }
                }
                this.connection = new StreamConnection(stream, this.connectionStream);
            }

            public static IConnection End(IAsyncResult result)
            {
                return AsyncResult.End<InitialServerConnectionReader.UpgradeConnectionAsyncResult>(result).connection;
            }

            private static void OnAcceptUpgrade(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    InitialServerConnectionReader.UpgradeConnectionAsyncResult asyncState = (InitialServerConnectionReader.UpgradeConnectionAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.CompleteAcceptUpgrade(result);
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

