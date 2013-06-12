namespace System.Net
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Threading;

    [Serializable]
    public class FileWebRequest : WebRequest, ISerializable
    {
        private int m_Aborted;
        private string m_connectionGroupName;
        private long m_contentLength;
        private ICredentials m_credentials;
        private FileAccess m_fileAccess;
        private WebHeaderCollection m_headers;
        private string m_method;
        private bool m_preauthenticate;
        private IWebProxy m_proxy;
        private LazyAsyncResult m_ReadAResult;
        private ManualResetEvent m_readerEvent;
        private bool m_readPending;
        private WebResponse m_response;
        private Stream m_stream;
        private bool m_syncHint;
        private int m_timeout;
        private Uri m_uri;
        private LazyAsyncResult m_WriteAResult;
        private bool m_writePending;
        private bool m_writing;
        private static WaitCallback s_GetRequestStreamCallback = new WaitCallback(FileWebRequest.GetRequestStreamCallback);
        private static WaitCallback s_GetResponseCallback = new WaitCallback(FileWebRequest.GetResponseCallback);
        private static ContextCallback s_WrappedGetRequestStreamCallback = new ContextCallback(FileWebRequest.GetRequestStreamCallback);
        private static ContextCallback s_WrappedResponseCallback = new ContextCallback(FileWebRequest.GetResponseCallback);

        internal FileWebRequest(Uri uri)
        {
            this.m_method = "GET";
            this.m_timeout = 0x186a0;
            if (uri.Scheme != Uri.UriSchemeFile)
            {
                throw new ArgumentOutOfRangeException("uri");
            }
            this.m_uri = uri;
            this.m_fileAccess = FileAccess.Read;
            this.m_headers = new WebHeaderCollection(WebHeaderCollectionType.FileWebRequest);
        }

        [Obsolete("Serialization is obsoleted for this type. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected FileWebRequest(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
            this.m_method = "GET";
            this.m_timeout = 0x186a0;
            this.m_headers = (WebHeaderCollection) serializationInfo.GetValue("headers", typeof(WebHeaderCollection));
            this.m_proxy = (IWebProxy) serializationInfo.GetValue("proxy", typeof(IWebProxy));
            this.m_uri = (Uri) serializationInfo.GetValue("uri", typeof(Uri));
            this.m_connectionGroupName = serializationInfo.GetString("connectionGroupName");
            this.m_method = serializationInfo.GetString("method");
            this.m_contentLength = serializationInfo.GetInt64("contentLength");
            this.m_timeout = serializationInfo.GetInt32("timeout");
            this.m_fileAccess = (FileAccess) serializationInfo.GetInt32("fileAccess");
        }

        public override void Abort()
        {
            if (Logging.On)
            {
                Logging.PrintWarning(Logging.Web, NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled));
            }
            try
            {
                if (Interlocked.Increment(ref this.m_Aborted) == 1)
                {
                    LazyAsyncResult readAResult = this.m_ReadAResult;
                    LazyAsyncResult writeAResult = this.m_WriteAResult;
                    WebException result = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
                    Stream stream = this.m_stream;
                    if ((readAResult != null) && !readAResult.IsCompleted)
                    {
                        readAResult.InvokeCallback(result);
                    }
                    if ((writeAResult != null) && !writeAResult.IsCompleted)
                    {
                        writeAResult.InvokeCallback(result);
                    }
                    if (stream != null)
                    {
                        if (stream is ICloseEx)
                        {
                            ((ICloseEx) stream).CloseEx(CloseExState.Abort);
                        }
                        else
                        {
                            stream.Close();
                        }
                    }
                    if (this.m_response != null)
                    {
                        ((ICloseEx) this.m_response).CloseEx(CloseExState.Abort);
                    }
                }
            }
            catch (Exception exception2)
            {
                if (Logging.On)
                {
                    Logging.Exception(Logging.Web, this, "Abort", exception2);
                }
                throw;
            }
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            try
            {
                if (this.Aborted)
                {
                    throw ExceptionHelper.RequestAbortedException;
                }
                if (!this.CanGetRequestStream())
                {
                    Exception exception = new ProtocolViolationException(SR.GetString("net_nouploadonget"));
                    throw exception;
                }
                if (this.m_response != null)
                {
                    Exception exception2 = new InvalidOperationException(SR.GetString("net_reqsubmitted"));
                    throw exception2;
                }
                lock (this)
                {
                    if (this.m_writePending)
                    {
                        Exception exception3 = new InvalidOperationException(SR.GetString("net_repcall"));
                        throw exception3;
                    }
                    this.m_writePending = true;
                }
                this.m_ReadAResult = new LazyAsyncResult(this, state, callback);
                ThreadPool.QueueUserWorkItem(s_GetRequestStreamCallback, this.m_ReadAResult);
            }
            catch (Exception exception4)
            {
                if (Logging.On)
                {
                    Logging.Exception(Logging.Web, this, "BeginGetRequestStream", exception4);
                }
                throw;
            }
            return this.m_ReadAResult;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            try
            {
                if (this.Aborted)
                {
                    throw ExceptionHelper.RequestAbortedException;
                }
                lock (this)
                {
                    if (this.m_readPending)
                    {
                        Exception exception = new InvalidOperationException(SR.GetString("net_repcall"));
                        throw exception;
                    }
                    this.m_readPending = true;
                }
                this.m_WriteAResult = new LazyAsyncResult(this, state, callback);
                ThreadPool.QueueUserWorkItem(s_GetResponseCallback, this.m_WriteAResult);
            }
            catch (Exception exception2)
            {
                if (Logging.On)
                {
                    Logging.Exception(Logging.Web, this, "BeginGetResponse", exception2);
                }
                throw;
            }
            return this.m_WriteAResult;
        }

        private bool CanGetRequestStream()
        {
            return !KnownHttpVerb.Parse(this.m_method).ContentBodyNotAllowed;
        }

        public override Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            Stream stream;
            try
            {
                LazyAsyncResult result = asyncResult as LazyAsyncResult;
                if ((asyncResult == null) || (result == null))
                {
                    Exception exception = (asyncResult == null) ? new ArgumentNullException("asyncResult") : new ArgumentException(SR.GetString("InvalidAsyncResult"), "asyncResult");
                    throw exception;
                }
                object obj2 = result.InternalWaitForCompletion();
                if (obj2 is Exception)
                {
                    throw ((Exception) obj2);
                }
                stream = (Stream) obj2;
                this.m_writePending = false;
            }
            catch (Exception exception2)
            {
                if (Logging.On)
                {
                    Logging.Exception(Logging.Web, this, "EndGetRequestStream", exception2);
                }
                throw;
            }
            return stream;
        }

        public override WebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            WebResponse response;
            try
            {
                LazyAsyncResult result = asyncResult as LazyAsyncResult;
                if ((asyncResult == null) || (result == null))
                {
                    Exception exception = (asyncResult == null) ? new ArgumentNullException("asyncResult") : new ArgumentException(SR.GetString("InvalidAsyncResult"), "asyncResult");
                    throw exception;
                }
                object obj2 = result.InternalWaitForCompletion();
                if (obj2 is Exception)
                {
                    throw ((Exception) obj2);
                }
                response = (WebResponse) obj2;
                this.m_readPending = false;
            }
            catch (Exception exception2)
            {
                if (Logging.On)
                {
                    Logging.Exception(Logging.Web, this, "EndGetResponse", exception2);
                }
                throw;
            }
            return response;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        protected override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            serializationInfo.AddValue("headers", this.m_headers, typeof(WebHeaderCollection));
            serializationInfo.AddValue("proxy", this.m_proxy, typeof(IWebProxy));
            serializationInfo.AddValue("uri", this.m_uri, typeof(Uri));
            serializationInfo.AddValue("connectionGroupName", this.m_connectionGroupName);
            serializationInfo.AddValue("method", this.m_method);
            serializationInfo.AddValue("contentLength", this.m_contentLength);
            serializationInfo.AddValue("timeout", this.m_timeout);
            serializationInfo.AddValue("fileAccess", this.m_fileAccess);
            serializationInfo.AddValue("preauthenticate", false);
            base.GetObjectData(serializationInfo, streamingContext);
        }

        public override Stream GetRequestStream()
        {
            IAsyncResult result;
            try
            {
                result = this.BeginGetRequestStream(null, null);
                if (((this.Timeout != -1) && !result.IsCompleted) && (!result.AsyncWaitHandle.WaitOne(this.Timeout, false) || !result.IsCompleted))
                {
                    if (this.m_stream != null)
                    {
                        this.m_stream.Close();
                    }
                    Exception exception = new WebException(NetRes.GetWebStatusString(WebExceptionStatus.Timeout), WebExceptionStatus.Timeout);
                    throw exception;
                }
            }
            catch (Exception exception2)
            {
                if (Logging.On)
                {
                    Logging.Exception(Logging.Web, this, "GetRequestStream", exception2);
                }
                throw;
            }
            return this.EndGetRequestStream(result);
        }

        private static void GetRequestStreamCallback(object state)
        {
            LazyAsyncResult result = (LazyAsyncResult) state;
            FileWebRequest asyncObject = (FileWebRequest) result.AsyncObject;
            try
            {
                if (asyncObject.m_stream == null)
                {
                    asyncObject.m_stream = new FileWebStream(asyncObject, asyncObject.m_uri.LocalPath, FileMode.Create, FileAccess.Write, FileShare.Read);
                    asyncObject.m_fileAccess = FileAccess.Write;
                    asyncObject.m_writing = true;
                }
            }
            catch (Exception exception)
            {
                Exception exception2 = new WebException(exception.Message, exception);
                result.InvokeCallback(exception2);
                return;
            }
            result.InvokeCallback(asyncObject.m_stream);
        }

        public override WebResponse GetResponse()
        {
            IAsyncResult result;
            this.m_syncHint = true;
            try
            {
                result = this.BeginGetResponse(null, null);
                if (((this.Timeout != -1) && !result.IsCompleted) && (!result.AsyncWaitHandle.WaitOne(this.Timeout, false) || !result.IsCompleted))
                {
                    if (this.m_response != null)
                    {
                        this.m_response.Close();
                    }
                    Exception exception = new WebException(NetRes.GetWebStatusString(WebExceptionStatus.Timeout), WebExceptionStatus.Timeout);
                    throw exception;
                }
            }
            catch (Exception exception2)
            {
                if (Logging.On)
                {
                    Logging.Exception(Logging.Web, this, "GetResponse", exception2);
                }
                throw;
            }
            return this.EndGetResponse(result);
        }

        private static void GetResponseCallback(object state)
        {
            LazyAsyncResult result = (LazyAsyncResult) state;
            FileWebRequest asyncObject = (FileWebRequest) result.AsyncObject;
            if (asyncObject.m_writePending || asyncObject.m_writing)
            {
                lock (asyncObject)
                {
                    if (asyncObject.m_writePending || asyncObject.m_writing)
                    {
                        asyncObject.m_readerEvent = new ManualResetEvent(false);
                    }
                }
            }
            if (asyncObject.m_readerEvent != null)
            {
                asyncObject.m_readerEvent.WaitOne();
            }
            try
            {
                if (asyncObject.m_response == null)
                {
                    asyncObject.m_response = new FileWebResponse(asyncObject, asyncObject.m_uri, asyncObject.m_fileAccess, !asyncObject.m_syncHint);
                }
            }
            catch (Exception exception)
            {
                Exception exception2 = new WebException(exception.Message, exception);
                result.InvokeCallback(exception2);
                return;
            }
            result.InvokeCallback(asyncObject.m_response);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter, SerializationFormatter=true)]
        void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            this.GetObjectData(serializationInfo, streamingContext);
        }

        internal void UnblockReader()
        {
            lock (this)
            {
                if (this.m_readerEvent != null)
                {
                    this.m_readerEvent.Set();
                }
            }
            this.m_writing = false;
        }

        internal bool Aborted
        {
            get
            {
                return (this.m_Aborted != 0);
            }
        }

        public override string ConnectionGroupName
        {
            get
            {
                return this.m_connectionGroupName;
            }
            set
            {
                this.m_connectionGroupName = value;
            }
        }

        public override long ContentLength
        {
            get
            {
                return this.m_contentLength;
            }
            set
            {
                if (value < 0L)
                {
                    throw new ArgumentException(SR.GetString("net_clsmall"), "value");
                }
                this.m_contentLength = value;
            }
        }

        public override string ContentType
        {
            get
            {
                return this.m_headers["Content-Type"];
            }
            set
            {
                this.m_headers["Content-Type"] = value;
            }
        }

        public override ICredentials Credentials
        {
            get
            {
                return this.m_credentials;
            }
            set
            {
                this.m_credentials = value;
            }
        }

        public override WebHeaderCollection Headers
        {
            get
            {
                return this.m_headers;
            }
        }

        public override string Method
        {
            get
            {
                return this.m_method;
            }
            set
            {
                if (ValidationHelper.IsBlankString(value))
                {
                    throw new ArgumentException(SR.GetString("net_badmethod"), "value");
                }
                this.m_method = value;
            }
        }

        public override bool PreAuthenticate
        {
            get
            {
                return this.m_preauthenticate;
            }
            set
            {
                this.m_preauthenticate = true;
            }
        }

        public override IWebProxy Proxy
        {
            get
            {
                return this.m_proxy;
            }
            set
            {
                this.m_proxy = value;
            }
        }

        public override Uri RequestUri
        {
            get
            {
                return this.m_uri;
            }
        }

        public override int Timeout
        {
            get
            {
                return this.m_timeout;
            }
            set
            {
                if ((value < 0) && (value != -1))
                {
                    throw new ArgumentOutOfRangeException("value", SR.GetString("net_io_timeout_use_ge_zero"));
                }
                this.m_timeout = value;
            }
        }

        public override bool UseDefaultCredentials
        {
            get
            {
                throw ExceptionHelper.PropertyNotSupportedException;
            }
            set
            {
                throw ExceptionHelper.PropertyNotSupportedException;
            }
        }
    }
}

