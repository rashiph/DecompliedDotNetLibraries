namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Threading;

    internal class PipeConnectionListener : IConnectionListener, IDisposable
    {
        private List<SecurityIdentifier> allowedSids;
        private bool anyPipesCreated;
        private int bufferSize;
        private HostNameComparisonMode hostNameComparisonMode;
        private bool isDisposed;
        private bool isListening;
        private int maxInstances;
        private List<PendingAccept> pendingAccepts;
        private Uri pipeUri;
        private PipeSharedMemory sharedMemory;
        private bool useCompletionPort;

        public PipeConnectionListener(Uri pipeUri, HostNameComparisonMode hostNameComparisonMode, int bufferSize, List<SecurityIdentifier> allowedSids, bool useCompletionPort, int maxConnections)
        {
            PipeUri.Validate(pipeUri);
            this.pipeUri = pipeUri;
            this.hostNameComparisonMode = hostNameComparisonMode;
            this.allowedSids = allowedSids;
            this.bufferSize = bufferSize;
            this.pendingAccepts = new List<PendingAccept>();
            this.useCompletionPort = useCompletionPort;
            this.maxInstances = Math.Min(maxConnections, 0xff);
        }

        public IAsyncResult BeginAccept(AsyncCallback callback, object state)
        {
            lock (this.ThisLock)
            {
                if (this.isDisposed)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException("", System.ServiceModel.SR.GetString("PipeListenerDisposed")));
                }
                if (!this.isListening)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("PipeListenerNotListening")));
                }
                PipeHandle pipeHandle = this.CreatePipe();
                PendingAccept item = new PendingAccept(this, pipeHandle, this.useCompletionPort, callback, state);
                if (!item.CompletedSynchronously)
                {
                    this.pendingAccepts.Add(item);
                }
                return item;
            }
        }

        private unsafe PipeHandle CreatePipe()
        {
            byte[] buffer;
            PipeHandle handle;
            int num2;
            PipeHandle handle2;
            int openMode = 0x40000003;
            if (!this.anyPipesCreated)
            {
                openMode |= 0x80000;
            }
            try
            {
                buffer = SecurityDescriptorHelper.FromSecurityIdentifiers(this.allowedSids, -1073741824);
            }
            catch (Win32Exception exception)
            {
                Exception innerException = new PipeException(exception.Message, exception);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(innerException.Message, innerException));
            }
            fixed (byte* numRef = buffer)
            {
                UnsafeNativeMethods.SECURITY_ATTRIBUTES securityAttributes = new UnsafeNativeMethods.SECURITY_ATTRIBUTES {
                    lpSecurityDescriptor = numRef
                };
                handle = UnsafeNativeMethods.CreateNamedPipe(this.sharedMemory.PipeName, openMode, 6, this.maxInstances, this.bufferSize, this.bufferSize, 0, securityAttributes);
                num2 = Marshal.GetLastWin32Error();
            }
            if (handle.IsInvalid)
            {
                handle.SetHandleAsInvalid();
                Exception exception3 = new PipeException(System.ServiceModel.SR.GetString("PipeListenFailed", new object[] { this.pipeUri.AbsoluteUri, PipeError.GetErrorString(num2) }), num2);
                switch (num2)
                {
                    case 5:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new AddressAccessDeniedException(exception3.Message, exception3));

                    case 0xb7:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new AddressAlreadyInUseException(exception3.Message, exception3));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(exception3.Message, exception3));
            }
            bool flag = true;
            try
            {
                if (this.useCompletionPort)
                {
                    ThreadPool.BindHandle(handle);
                }
                this.anyPipesCreated = true;
                flag = false;
                handle2 = handle;
            }
            finally
            {
                if (flag)
                {
                    handle.Close();
                }
            }
            return handle2;
        }

        public void Dispose()
        {
            lock (this.ThisLock)
            {
                if (!this.isDisposed)
                {
                    if (this.sharedMemory != null)
                    {
                        this.sharedMemory.Dispose();
                    }
                    for (int i = 0; i < this.pendingAccepts.Count; i++)
                    {
                        this.pendingAccepts[i].Abort();
                    }
                    this.isDisposed = true;
                }
            }
        }

        public IConnection EndAccept(IAsyncResult result)
        {
            PendingAccept accept = result as PendingAccept;
            if (accept == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("result", System.ServiceModel.SR.GetString("InvalidAsyncResult"));
            }
            PipeHandle pipe = accept.End();
            if (pipe == null)
            {
                return null;
            }
            return new PipeConnection(pipe, this.bufferSize, accept.IsBoundToCompletionPort, accept.IsBoundToCompletionPort);
        }

        public void Listen()
        {
            lock (this.ThisLock)
            {
                if (!this.isListening)
                {
                    string sharedMemoryName = PipeUri.BuildSharedMemoryName(this.pipeUri, this.hostNameComparisonMode, true);
                    if (!PipeSharedMemory.TryCreate(this.allowedSids, this.pipeUri, sharedMemoryName, out this.sharedMemory))
                    {
                        PipeSharedMemory result = null;
                        Uri uri = new Uri(this.pipeUri, Guid.NewGuid().ToString());
                        string str2 = PipeUri.BuildSharedMemoryName(uri, this.hostNameComparisonMode, true);
                        if (PipeSharedMemory.TryCreate(this.allowedSids, uri, str2, out result))
                        {
                            result.Dispose();
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(PipeSharedMemory.CreatePipeNameInUseException(5, this.pipeUri));
                        }
                        sharedMemoryName = PipeUri.BuildSharedMemoryName(this.pipeUri, this.hostNameComparisonMode, false);
                        this.sharedMemory = PipeSharedMemory.Create(this.allowedSids, this.pipeUri, sharedMemoryName);
                    }
                    this.isListening = true;
                }
            }
        }

        private void RemovePendingAccept(PendingAccept pendingAccept)
        {
            lock (this.ThisLock)
            {
                this.pendingAccepts.Remove(pendingAccept);
            }
        }

        public string PipeName
        {
            get
            {
                return this.sharedMemory.PipeName;
            }
        }

        private object ThisLock
        {
            get
            {
                return this;
            }
        }

        private class PendingAccept : AsyncResult
        {
            private bool isBoundToCompletionPort;
            private PipeConnectionListener listener;
            private OverlappedIOCompleteCallback onAcceptComplete;
            private static Action<object> onStartAccept;
            private OverlappedContext overlapped;
            private PipeHandle pipeHandle;
            private PipeHandle result;

            public PendingAccept(PipeConnectionListener listener, PipeHandle pipeHandle, bool isBoundToCompletionPort, AsyncCallback callback, object state) : base(callback, state)
            {
                this.pipeHandle = pipeHandle;
                this.result = pipeHandle;
                this.listener = listener;
                this.onAcceptComplete = new OverlappedIOCompleteCallback(this.OnAcceptComplete);
                this.overlapped = new OverlappedContext();
                this.isBoundToCompletionPort = isBoundToCompletionPort;
                if (!Thread.CurrentThread.IsThreadPoolThread)
                {
                    if (onStartAccept == null)
                    {
                        onStartAccept = new Action<object>(PipeConnectionListener.PendingAccept.OnStartAccept);
                    }
                    ActionItem.Schedule(onStartAccept, this);
                }
                else
                {
                    this.StartAccept(true);
                }
            }

            public void Abort()
            {
                this.result = null;
                this.pipeHandle.Close();
            }

            private Exception CreatePipeAcceptFailedException(int errorCode)
            {
                Exception innerException = new PipeException(System.ServiceModel.SR.GetString("PipeAcceptFailed", new object[] { PipeError.GetErrorString(errorCode) }), errorCode);
                return new CommunicationException(innerException.Message, innerException);
            }

            public PipeHandle End()
            {
                AsyncResult.End<PipeConnectionListener.PendingAccept>(this);
                return this.result;
            }

            private unsafe void OnAcceptComplete(bool haveResult, int error, int numBytes)
            {
                this.listener.RemovePendingAccept(this);
                if (!haveResult)
                {
                    if ((this.result != null) && (UnsafeNativeMethods.GetOverlappedResult(this.pipeHandle, this.overlapped.NativeOverlapped, out numBytes, 0) == 0))
                    {
                        error = Marshal.GetLastWin32Error();
                    }
                    else
                    {
                        error = 0;
                    }
                }
                this.overlapped.Free();
                if (error != 0)
                {
                    this.pipeHandle.Close();
                    base.Complete(false, this.CreatePipeAcceptFailedException(error));
                }
                else
                {
                    base.Complete(false);
                }
            }

            private static void OnStartAccept(object state)
            {
                ((PipeConnectionListener.PendingAccept) state).StartAccept(false);
            }

            private unsafe void StartAccept(bool synchronous)
            {
                Exception exception = null;
                bool flag = false;
                try
                {
                    try
                    {
                        this.overlapped.StartAsyncOperation(null, this.onAcceptComplete, this.isBoundToCompletionPort);
                    Label_001C:
                        if (UnsafeNativeMethods.ConnectNamedPipe(this.pipeHandle, this.overlapped.NativeOverlapped) == 0)
                        {
                            int errorCode = Marshal.GetLastWin32Error();
                            switch (errorCode)
                            {
                                case 0xe8:
                                    if (UnsafeNativeMethods.DisconnectNamedPipe(this.pipeHandle) == 0)
                                    {
                                        flag = true;
                                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreatePipeAcceptFailedException(errorCode));
                                    }
                                    goto Label_001C;

                                case 0x217:
                                    flag = true;
                                    goto Label_00E1;

                                case 0x3e5:
                                    goto Label_00E1;
                            }
                            flag = true;
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreatePipeAcceptFailedException(errorCode));
                        }
                        flag = true;
                    }
                    catch (ObjectDisposedException exception2)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                        }
                        flag = true;
                    }
                    finally
                    {
                        if (flag)
                        {
                            this.overlapped.CancelAsyncOperation();
                            this.overlapped.Free();
                        }
                    }
                }
                catch (Exception exception3)
                {
                    if (Fx.IsFatal(exception3))
                    {
                        throw;
                    }
                    flag = true;
                    exception = exception3;
                }
            Label_00E1:
                if (flag)
                {
                    if (!synchronous)
                    {
                        this.listener.RemovePendingAccept(this);
                    }
                    base.Complete(synchronous, exception);
                }
            }

            public bool IsBoundToCompletionPort
            {
                get
                {
                    return this.isBoundToCompletionPort;
                }
            }
        }
    }
}

