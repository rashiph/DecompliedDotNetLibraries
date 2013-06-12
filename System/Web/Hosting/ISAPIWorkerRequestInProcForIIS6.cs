namespace System.Web.Hosting
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Web;
    using System.Web.Util;

    internal class ISAPIWorkerRequestInProcForIIS6 : ISAPIWorkerRequestInProc
    {
        private int _asyncFinalStatus;
        private ISAPIAsyncCompletionCallback _asyncFlushCompletionCallback;
        private static int _asyncIoCount;
        private HttpAsyncResult _asyncResultOfExecuteUrl;
        private bool _cacheInKernelMode;
        private bool _disableKernelCache;
        private IntPtr _entity;
        private ISAPIAsyncCompletionCallback _executeUrlCompletionCallback;
        private bool _headersSentFromExecuteUrl;
        private GCHandle _rootedThis;
        private bool _serverSupportFunctionError;
        protected bool _trySkipIisCustomErrors;
        private const int MIN_ASYNC_SIZE = 0x800;
        private const int TRY_SKIP_IIS_CUSTOM_ERRORS = 0x40;

        internal ISAPIWorkerRequestInProcForIIS6(IntPtr ecb) : base(ecb)
        {
        }

        internal override IAsyncResult BeginExecuteUrl(string url, string method, string childHeaders, bool sendHeaders, bool addUserIndo, IntPtr token, string name, string authType, byte[] entity, AsyncCallback cb, object state)
        {
            if (((base._ecb == IntPtr.Zero) || (this._asyncResultOfExecuteUrl != null)) || (sendHeaders && this.HeadersSent()))
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Cannot_execute_url_in_this_context"));
            }
            if (((entity != null) && (entity.Length > 0)) && (UnsafeNativeMethods.EcbGetExecUrlEntityInfo(entity.Length, entity, out this._entity) != 1))
            {
                throw new HttpException(System.Web.SR.GetString("Failed_to_execute_url"));
            }
            HttpAsyncResult result = new HttpAsyncResult(cb, state);
            this._asyncResultOfExecuteUrl = result;
            this._executeUrlCompletionCallback = new ISAPIAsyncCompletionCallback(this.OnExecuteUrlCompletion);
            this._rootedThis = GCHandle.Alloc(this);
            if (UnsafeNativeMethods.EcbExecuteUrlUnicode(base._ecb, url, method, childHeaders, sendHeaders, addUserIndo, token, name, authType, this._entity, this._executeUrlCompletionCallback) != 1)
            {
                if (this._entity != IntPtr.Zero)
                {
                    UnsafeNativeMethods.EcbFreeExecUrlEntityInfo(this._entity);
                }
                this._rootedThis.Free();
                this._asyncResultOfExecuteUrl = null;
                throw new HttpException(System.Web.SR.GetString("Failed_to_execute_url"));
            }
            if (sendHeaders)
            {
                this._headersSentFromExecuteUrl = true;
            }
            return result;
        }

        internal override void DisableKernelCache()
        {
            this._disableKernelCache = true;
            this._cacheInKernelMode = false;
        }

        internal override void EndExecuteUrl(IAsyncResult result)
        {
            HttpAsyncResult result2 = result as HttpAsyncResult;
            if (result2 != null)
            {
                result2.End();
            }
        }

        internal override void FlushCore(byte[] status, byte[] header, int keepConnected, int totalBodySize, int numBodyFragments, IntPtr[] bodyFragments, int[] bodyFragmentLengths, int doneWithSession, int finalStatus, out bool async)
        {
            async = false;
            if (base._ecb != IntPtr.Zero)
            {
                if (this._headersSentFromExecuteUrl)
                {
                    status = null;
                    header = null;
                }
                if (((doneWithSession != 0) && !HttpRuntime.ShutdownInProgress) && (base._ignoreMinAsyncSize || (totalBodySize >= 0x800)))
                {
                    if (base._requiresAsyncFlushCallback)
                    {
                        this._asyncFlushCompletionCallback = new ISAPIAsyncCompletionCallback(this.OnAsyncFlushCompletion);
                        this._asyncFinalStatus = finalStatus;
                        this._rootedThis = GCHandle.Alloc(this);
                        doneWithSession = 0;
                        async = true;
                        Interlocked.Increment(ref _asyncIoCount);
                    }
                    else
                    {
                        this._asyncFlushCompletionCallback = null;
                        doneWithSession = 0;
                        async = true;
                    }
                }
                int num = this._trySkipIisCustomErrors ? (finalStatus | 0x40) : finalStatus;
                int hr = UnsafeNativeMethods.EcbFlushCore(base._ecb, status, header, keepConnected, totalBodySize, numBodyFragments, bodyFragments, bodyFragmentLengths, doneWithSession, num, this._cacheInKernelMode ? 1 : 0, async ? 1 : 0, this._asyncFlushCompletionCallback);
                if ((!base._requiresAsyncFlushCallback && (hr == 0)) && async)
                {
                    base.UnlockCachedResponseBytesOnceAfterIoComplete();
                    base.CallEndOfRequestCallbackOnceAfterAllIoComplete();
                }
                else if ((hr != 0) && async)
                {
                    async = false;
                    UnsafeNativeMethods.EcbFlushCore(base._ecb, null, null, 0, 0, 0, null, null, 1, this._asyncFinalStatus, 0, 0, null);
                    if (this._asyncFlushCompletionCallback != null)
                    {
                        this._rootedThis.Free();
                        Interlocked.Decrement(ref _asyncIoCount);
                    }
                }
                else if (((hr != 0) && !async) && ((doneWithSession == 0) && !this._serverSupportFunctionError))
                {
                    this._serverSupportFunctionError = true;
                    string name = "Server_Support_Function_Error";
                    switch (hr)
                    {
                        case -2147014843:
                        case -2147014842:
                            name = "Server_Support_Function_Error_Disconnect";
                            PerfCounters.IncrementGlobalCounter(GlobalPerfCounter.REQUESTS_DISCONNECTED);
                            break;
                    }
                    throw new HttpException(System.Web.SR.GetString(name, new object[] { hr.ToString("X8", CultureInfo.InvariantCulture) }), hr);
                }
            }
        }

        protected override void GetAdditionalServerVariables()
        {
            if ((base._ecb != IntPtr.Zero) && (base._additionalServerVars == null))
            {
                base._additionalServerVars = new string[0x17];
                using (ServerVarCharBuffer buffer = new ServerVarCharBuffer())
                {
                    int[] serverVarLengths = new int[0x17];
                    int requiredSize = 0;
                    int errorCode = UnsafeNativeMethods.EcbGetUnicodeServerVariables(base._ecb, buffer.PinnedAddress, buffer.Length, serverVarLengths, serverVarLengths.Length, 12, ref requiredSize);
                    if (requiredSize > buffer.Length)
                    {
                        buffer.Resize(requiredSize);
                        errorCode = UnsafeNativeMethods.EcbGetUnicodeServerVariables(base._ecb, buffer.PinnedAddress, buffer.Length, serverVarLengths, serverVarLengths.Length, 12, ref requiredSize);
                    }
                    if (errorCode != 0)
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                    IntPtr pinnedAddress = buffer.PinnedAddress;
                    for (int i = 0; i < base._additionalServerVars.Length; i++)
                    {
                        base._additionalServerVars[i] = Marshal.PtrToStringUni(pinnedAddress, serverVarLengths[i]);
                        pinnedAddress = new IntPtr(((long) pinnedAddress) + (2L * (1L + serverVarLengths[i])));
                    }
                }
            }
        }

        private void GetBasicServerVariables()
        {
            if ((base._ecb != IntPtr.Zero) && (base._basicServerVars == null))
            {
                base._basicServerVars = new string[12];
                using (ServerVarCharBuffer buffer = new ServerVarCharBuffer())
                {
                    int[] serverVarLengths = new int[12];
                    int requiredSize = 0;
                    int hresult = UnsafeNativeMethods.EcbGetUnicodeServerVariables(base._ecb, buffer.PinnedAddress, buffer.Length, serverVarLengths, serverVarLengths.Length, 0, ref requiredSize);
                    if (requiredSize > buffer.Length)
                    {
                        buffer.Resize(requiredSize);
                        hresult = UnsafeNativeMethods.EcbGetUnicodeServerVariables(base._ecb, buffer.PinnedAddress, buffer.Length, serverVarLengths, serverVarLengths.Length, 0, ref requiredSize);
                    }
                    Misc.ThrowIfFailedHr(hresult);
                    IntPtr pinnedAddress = buffer.PinnedAddress;
                    for (int i = 0; i < base._basicServerVars.Length; i++)
                    {
                        base._basicServerVars[i] = Marshal.PtrToStringUni(pinnedAddress, serverVarLengths[i]);
                        pinnedAddress = new IntPtr(((long) pinnedAddress) + (2L * (1L + serverVarLengths[i])));
                    }
                    base._appPathTranslated = base._basicServerVars[2];
                    base._method = base._basicServerVars[3];
                    base._path = base._basicServerVars[4];
                    base._pathTranslated = base._basicServerVars[5];
                    base._filePath = base._basicServerVars[6];
                }
            }
        }

        public override string GetRawUrl()
        {
            return HttpWorkerRequest.GetRawUrlHelper(this.GetUnicodeServerVariable(7));
        }

        protected override string GetServerVariableCore(string name)
        {
            if (StringUtil.StringStartsWith(name, "HTTP_"))
            {
                return base.GetServerVariableCore(name);
            }
            return this.GetUnicodeServerVariable("UNICODE_" + name);
        }

        private string GetUnicodeServerVariable(int nameIndex)
        {
            using (ServerVarCharBuffer buffer = new ServerVarCharBuffer())
            {
                return this.GetUnicodeServerVariable(nameIndex, buffer);
            }
        }

        private string GetUnicodeServerVariable(string name)
        {
            using (ServerVarCharBuffer buffer = new ServerVarCharBuffer())
            {
                return this.GetUnicodeServerVariable(name, buffer);
            }
        }

        private string GetUnicodeServerVariable(int nameIndex, ServerVarCharBuffer buffer)
        {
            if (base._ecb != IntPtr.Zero)
            {
                int len = UnsafeNativeMethods.EcbGetUnicodeServerVariableByIndex(base._ecb, nameIndex, buffer.PinnedAddress, buffer.Length);
                if (len < 0)
                {
                    buffer.Resize(-len);
                    len = UnsafeNativeMethods.EcbGetUnicodeServerVariableByIndex(base._ecb, nameIndex, buffer.PinnedAddress, buffer.Length);
                }
                if (len > 0)
                {
                    return Marshal.PtrToStringUni(buffer.PinnedAddress, len);
                }
            }
            return null;
        }

        private string GetUnicodeServerVariable(string name, ServerVarCharBuffer buffer)
        {
            if (base._ecb != IntPtr.Zero)
            {
                int len = UnsafeNativeMethods.EcbGetUnicodeServerVariable(base._ecb, name, buffer.PinnedAddress, buffer.Length);
                if (len < 0)
                {
                    buffer.Resize(-len);
                    len = UnsafeNativeMethods.EcbGetUnicodeServerVariable(base._ecb, name, buffer.PinnedAddress, buffer.Length);
                }
                if (len > 0)
                {
                    return Marshal.PtrToStringUni(buffer.PinnedAddress, len);
                }
            }
            return null;
        }

        private void OnAsyncFlushCompletion(IntPtr ecb, int byteCount, int error)
        {
            try
            {
                this._rootedThis.Free();
                UnsafeNativeMethods.EcbFlushCore(ecb, null, null, 0, 0, 0, null, null, 1, this._asyncFinalStatus, 0, 0, null);
                base.UnlockCachedResponseBytesOnceAfterIoComplete();
                UnsafeNativeMethods.RevertToSelf();
                base.CallEndOfRequestCallbackOnceAfterAllIoComplete();
            }
            finally
            {
                Interlocked.Decrement(ref _asyncIoCount);
            }
        }

        private void OnExecuteUrlCompletion(IntPtr ecb, int byteCount, int error)
        {
            if (this._entity != IntPtr.Zero)
            {
                UnsafeNativeMethods.EcbFreeExecUrlEntityInfo(this._entity);
            }
            this._rootedThis.Free();
            HttpAsyncResult result = this._asyncResultOfExecuteUrl;
            this._asyncResultOfExecuteUrl = null;
            result.Complete(false, null, null);
        }

        internal override MemoryBytes PackageFile(string filename, long offset, long size, bool isImpersonating)
        {
            return new MemoryBytes(filename, offset, size);
        }

        internal override void ReadRequestBasics()
        {
            if (base._ecb != IntPtr.Zero)
            {
                this.GetBasicServerVariables();
                int num = base._path.Length - base._filePath.Length;
                if (num > 0)
                {
                    base._pathInfo = base._path.Substring(base._filePath.Length);
                    int length = base._pathTranslated.Length - num;
                    if (length > 0)
                    {
                        base._pathTranslated = base._pathTranslated.Substring(0, length);
                    }
                }
                else
                {
                    base._filePath = base._path;
                    base._pathInfo = string.Empty;
                }
                base._appPath = HostingEnvironment.ApplicationVirtualPath;
                int[] contentInfo = null;
                try
                {
                    contentInfo = RecyclableArrayHelper.GetIntegerArray(4);
                    UnsafeNativeMethods.EcbGetBasicsContentInfo(base._ecb, contentInfo);
                    base._contentType = contentInfo[0];
                    base._contentTotalLength = contentInfo[1];
                    base._contentAvailLength = contentInfo[2];
                    base._queryStringLength = contentInfo[3];
                }
                finally
                {
                    RecyclableArrayHelper.ReuseIntegerArray(contentInfo);
                }
            }
        }

        internal override void SendEmptyResponse()
        {
            UnsafeNativeMethods.UpdateLastActivityTimeForHealthMonitor();
        }

        internal override string SetupKernelCaching(int secondsToLive, string originalCacheUrl, bool enableKernelCacheForVaryByStar)
        {
            if ((base._ecb == IntPtr.Zero) || this._disableKernelCache)
            {
                return null;
            }
            string unicodeServerVariable = this.GetUnicodeServerVariable(7);
            if ((originalCacheUrl != null) && (originalCacheUrl != unicodeServerVariable))
            {
                return null;
            }
            if (string.IsNullOrEmpty(unicodeServerVariable) || (!enableKernelCacheForVaryByStar && (unicodeServerVariable.IndexOf('?') != -1)))
            {
                return null;
            }
            this._cacheInKernelMode = true;
            return unicodeServerVariable;
        }

        internal static void WaitForPendingAsyncIo()
        {
            while (_asyncIoCount != 0)
            {
                Thread.Sleep(250);
            }
        }

        internal override bool SupportsExecuteUrl
        {
            get
            {
                return true;
            }
        }

        internal override bool SupportsLongTransmitFile
        {
            get
            {
                return true;
            }
        }
    }
}

