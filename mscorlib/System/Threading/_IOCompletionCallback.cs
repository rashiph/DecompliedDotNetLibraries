namespace System.Threading
{
    using System;
    using System.Security;

    internal class _IOCompletionCallback
    {
        internal static ContextCallback _ccb = new ContextCallback(_IOCompletionCallback.IOCompletionCallback_Context);
        private uint _errorCode;
        private ExecutionContext _executionContext;
        [SecurityCritical]
        private IOCompletionCallback _ioCompletionCallback;
        private uint _numBytes;
        private unsafe NativeOverlapped* _pOVERLAP;

        [SecurityCritical]
        internal _IOCompletionCallback(IOCompletionCallback ioCompletionCallback, ref StackCrawlMark stackMark)
        {
            this._ioCompletionCallback = ioCompletionCallback;
            this._executionContext = ExecutionContext.Capture(ref stackMark, ExecutionContext.CaptureOptions.OptimizeDefaultCase | ExecutionContext.CaptureOptions.IgnoreSyncCtx);
        }

        [SecurityCritical]
        internal static unsafe void IOCompletionCallback_Context(object state)
        {
            _IOCompletionCallback callback = (_IOCompletionCallback) state;
            callback._ioCompletionCallback(callback._errorCode, callback._numBytes, callback._pOVERLAP);
        }

        [SecurityCritical]
        internal static unsafe void PerformIOCompletionCallback(uint errorCode, uint numBytes, NativeOverlapped* pOVERLAP)
        {
            do
            {
                Overlapped overlapped = OverlappedData.GetOverlappedFromNative(pOVERLAP).m_overlapped;
                _IOCompletionCallback iocbHelper = overlapped.iocbHelper;
                if (((iocbHelper == null) || (iocbHelper._executionContext == null)) || iocbHelper._executionContext.IsDefaultFTContext(true))
                {
                    overlapped.UserCallback(errorCode, numBytes, pOVERLAP);
                }
                else
                {
                    iocbHelper._errorCode = errorCode;
                    iocbHelper._numBytes = numBytes;
                    iocbHelper._pOVERLAP = pOVERLAP;
                    using (ExecutionContext context = iocbHelper._executionContext.CreateCopy())
                    {
                        ExecutionContext.Run(context, _ccb, iocbHelper, true);
                    }
                }
                OverlappedData.CheckVMForIOPacket(out pOVERLAP, out errorCode, out numBytes);
            }
            while (pOVERLAP != null);
        }
    }
}

