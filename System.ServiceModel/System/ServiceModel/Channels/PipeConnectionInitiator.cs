namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal class PipeConnectionInitiator : IConnectionInitiator
    {
        private int bufferSize;
        private bool includeSecurityIdentity;

        public PipeConnectionInitiator(bool includeSecurityIdentity, int bufferSize)
        {
            this.includeSecurityIdentity = includeSecurityIdentity;
            this.bufferSize = bufferSize;
        }

        public IAsyncResult BeginConnect(Uri uri, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ConnectAsyncResult(this, uri, timeout, callback, state);
        }

        public IConnection Connect(Uri remoteUri, TimeSpan timeout)
        {
            string str;
            BackoffTimeoutHelper helper;
            TimeoutHelper helper2 = new TimeoutHelper(timeout);
            this.PrepareConnect(remoteUri, helper2.RemainingTime(), out str, out helper);
            IConnection connection = null;
            while (connection == null)
            {
                connection = this.TryConnect(remoteUri, str, helper);
                if (connection == null)
                {
                    helper.WaitAndBackoff();
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Information, 0x40031, System.ServiceModel.SR.GetString("TraceCodeFailedPipeConnect", new object[] { helper2.RemainingTime(), remoteUri }));
                    }
                }
            }
            return connection;
        }

        private Exception CreateConnectFailedException(Uri remoteUri, PipeException innerException)
        {
            return new CommunicationException(System.ServiceModel.SR.GetString("PipeConnectFailed", new object[] { remoteUri.AbsoluteUri }), innerException);
        }

        public IConnection EndConnect(IAsyncResult result)
        {
            return ConnectAsyncResult.End(result);
        }

        internal static string GetPipeName(Uri uri)
        {
            string[] strArray = new string[] { "+", uri.Host, "*" };
            bool[] flagArray2 = new bool[2];
            flagArray2[0] = true;
            bool[] flagArray = flagArray2;
            for (int i = 0; i < strArray.Length; i++)
            {
                for (int j = 0; j < flagArray.Length; j++)
                {
                    for (string str = PipeUri.GetPath(uri); str.Length > 0; str = PipeUri.GetParentPath(str))
                    {
                        string sharedMemoryName = PipeUri.BuildSharedMemoryName(strArray[i], str, flagArray[j]);
                        try
                        {
                            PipeSharedMemory memory = PipeSharedMemory.Open(sharedMemoryName, uri);
                            if (memory != null)
                            {
                                try
                                {
                                    string pipeName = memory.PipeName;
                                    if (pipeName != null)
                                    {
                                        return pipeName;
                                    }
                                }
                                finally
                                {
                                    memory.Dispose();
                                }
                            }
                        }
                        catch (AddressAccessDeniedException exception)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new EndpointNotFoundException(System.ServiceModel.SR.GetString("EndpointNotFound", new object[] { uri.AbsoluteUri }), exception));
                        }
                    }
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new EndpointNotFoundException(System.ServiceModel.SR.GetString("EndpointNotFound", new object[] { uri.AbsoluteUri }), new PipeException(System.ServiceModel.SR.GetString("PipeEndpointNotFound", new object[] { uri.AbsoluteUri }))));
        }

        private void PrepareConnect(Uri remoteUri, TimeSpan timeout, out string resolvedAddress, out BackoffTimeoutHelper backoffHelper)
        {
            TimeSpan span;
            PipeUri.Validate(remoteUri);
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x4002a, System.ServiceModel.SR.GetString("TraceCodeInitiatingNamedPipeConnection"), new StringTraceRecord("Uri", remoteUri.ToString()), this, null);
            }
            resolvedAddress = GetPipeName(remoteUri);
            if (timeout >= TimeSpan.FromMilliseconds(300.0))
            {
                span = TimeoutHelper.Add(timeout, TimeSpan.Zero - TimeSpan.FromMilliseconds(150.0));
            }
            else
            {
                span = Ticks.ToTimeSpan((Ticks.FromMilliseconds(150) / 2L) + 1L);
            }
            backoffHelper = new BackoffTimeoutHelper(span, TimeSpan.FromMinutes(5.0));
        }

        private IConnection TryConnect(Uri remoteUri, string resolvedAddress, BackoffTimeoutHelper backoffHelper)
        {
            bool flag = backoffHelper.IsExpired();
            int dwFlagsAndAttributes = 0x40000000;
            if (this.includeSecurityIdentity)
            {
                dwFlagsAndAttributes |= 0x110000;
            }
            PipeHandle handle = UnsafeNativeMethods.CreateFile(resolvedAddress, -1073741824, 0, IntPtr.Zero, 3, dwFlagsAndAttributes, IntPtr.Zero);
            int errorCode = Marshal.GetLastWin32Error();
            if (handle.IsInvalid)
            {
                handle.SetHandleAsInvalid();
            }
            else
            {
                int mode = 2;
                if (UnsafeNativeMethods.SetNamedPipeHandleState(handle, ref mode, IntPtr.Zero, IntPtr.Zero) == 0)
                {
                    errorCode = Marshal.GetLastWin32Error();
                    handle.Close();
                    PipeException exception = new PipeException(System.ServiceModel.SR.GetString("PipeModeChangeFailed", new object[] { PipeError.GetErrorString(errorCode) }), errorCode);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateConnectFailedException(remoteUri, exception));
                }
                return new PipeConnection(handle, this.bufferSize, false, true);
            }
            if ((errorCode == 2) || (errorCode == 0xe7))
            {
                TimeoutException exception3;
                if (!flag)
                {
                    return null;
                }
                Exception exception2 = new PipeException(System.ServiceModel.SR.GetString("PipeConnectAddressFailed", new object[] { resolvedAddress, PipeError.GetErrorString(errorCode) }), errorCode);
                string absoluteUri = remoteUri.AbsoluteUri;
                if (errorCode == 0xe7)
                {
                    exception3 = new TimeoutException(System.ServiceModel.SR.GetString("PipeConnectTimedOutServerTooBusy", new object[] { absoluteUri, backoffHelper.OriginalTimeout }), exception2);
                }
                else
                {
                    exception3 = new TimeoutException(System.ServiceModel.SR.GetString("PipeConnectTimedOut", new object[] { absoluteUri, backoffHelper.OriginalTimeout }), exception2);
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception3);
            }
            PipeException innerException = new PipeException(System.ServiceModel.SR.GetString("PipeConnectAddressFailed", new object[] { resolvedAddress, PipeError.GetErrorString(errorCode) }), errorCode);
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateConnectFailedException(remoteUri, innerException));
        }

        private class ConnectAsyncResult : AsyncResult
        {
            private BackoffTimeoutHelper backoffHelper;
            private IConnection connection;
            private PipeConnectionInitiator parent;
            private Uri remoteUri;
            private string resolvedAddress;
            private TimeoutHelper timeoutHelper;
            private static Action<object> waitCompleteCallback;

            public ConnectAsyncResult(PipeConnectionInitiator parent, Uri remoteUri, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.parent = parent;
                this.remoteUri = remoteUri;
                this.timeoutHelper = new TimeoutHelper(timeout);
                parent.PrepareConnect(remoteUri, this.timeoutHelper.RemainingTime(), out this.resolvedAddress, out this.backoffHelper);
                if (this.ConnectAndWait())
                {
                    base.Complete(true);
                }
            }

            private bool ConnectAndWait()
            {
                this.connection = this.parent.TryConnect(this.remoteUri, this.resolvedAddress, this.backoffHelper);
                bool flag = this.connection != null;
                if (!flag)
                {
                    if (waitCompleteCallback == null)
                    {
                        waitCompleteCallback = new Action<object>(PipeConnectionInitiator.ConnectAsyncResult.OnWaitComplete);
                    }
                    this.backoffHelper.WaitAndBackoff(waitCompleteCallback, this);
                }
                return flag;
            }

            public static IConnection End(IAsyncResult result)
            {
                return AsyncResult.End<PipeConnectionInitiator.ConnectAsyncResult>(result).connection;
            }

            private static void OnWaitComplete(object state)
            {
                Exception exception = null;
                PipeConnectionInitiator.ConnectAsyncResult result = (PipeConnectionInitiator.ConnectAsyncResult) state;
                bool flag = true;
                try
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Information, 0x40031, System.ServiceModel.SR.GetString("TraceCodeFailedPipeConnect", new object[] { result.timeoutHelper.RemainingTime(), result.remoteUri }));
                    }
                    flag = result.ConnectAndWait();
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                if (flag)
                {
                    result.Complete(false, exception);
                }
            }
        }
    }
}

