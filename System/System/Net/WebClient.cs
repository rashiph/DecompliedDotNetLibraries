namespace System.Net
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Net.Cache;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [ComVisible(true)]
    public class WebClient : Component
    {
        private const int DefaultCopyBufferLength = 0x2000;
        private const int DefaultDownloadBufferLength = 0x10000;
        private const string DefaultUploadFileContentType = "application/octet-stream";
        private SendOrPostCallback downloadDataOperationCompleted;
        private SendOrPostCallback downloadFileOperationCompleted;
        private SendOrPostCallback downloadStringOperationCompleted;
        private AsyncOperation m_AsyncOp;
        private Uri m_baseAddress;
        private RequestCachePolicy m_CachePolicy;
        private int m_CallNesting;
        private bool m_Cancelled;
        private long m_ContentLength = -1L;
        private ICredentials m_credentials;
        private System.Text.Encoding m_Encoding = System.Text.Encoding.Default;
        private WebHeaderCollection m_headers;
        private bool m_InitWebClientAsync;
        private string m_Method;
        private ProgressData m_Progress;
        private IWebProxy m_Proxy;
        private bool m_ProxySet;
        private NameValueCollection m_requestParameters;
        private WebRequest m_WebRequest;
        private WebResponse m_WebResponse;
        private SendOrPostCallback openReadOperationCompleted;
        private SendOrPostCallback openWriteOperationCompleted;
        private SendOrPostCallback reportDownloadProgressChanged;
        private SendOrPostCallback reportUploadProgressChanged;
        private SendOrPostCallback uploadDataOperationCompleted;
        private const string UploadFileContentType = "multipart/form-data";
        private SendOrPostCallback uploadFileOperationCompleted;
        private SendOrPostCallback uploadStringOperationCompleted;
        private const string UploadValuesContentType = "application/x-www-form-urlencoded";
        private SendOrPostCallback uploadValuesOperationCompleted;

        public event DownloadDataCompletedEventHandler DownloadDataCompleted;

        public event AsyncCompletedEventHandler DownloadFileCompleted;

        public event DownloadProgressChangedEventHandler DownloadProgressChanged;

        public event DownloadStringCompletedEventHandler DownloadStringCompleted;

        public event OpenReadCompletedEventHandler OpenReadCompleted;

        public event OpenWriteCompletedEventHandler OpenWriteCompleted;

        public event UploadDataCompletedEventHandler UploadDataCompleted;

        public event UploadFileCompletedEventHandler UploadFileCompleted;

        public event UploadProgressChangedEventHandler UploadProgressChanged;

        public event UploadStringCompletedEventHandler UploadStringCompleted;

        public event UploadValuesCompletedEventHandler UploadValuesCompleted;

        private static void AbortRequest(WebRequest request)
        {
            try
            {
                if (request != null)
                {
                    request.Abort();
                }
            }
            catch (Exception exception)
            {
                if (((exception is OutOfMemoryException) || (exception is StackOverflowException)) || (exception is ThreadAbortException))
                {
                    throw;
                }
            }
        }

        private bool AnotherCallInProgress(int callNesting)
        {
            return (callNesting > 1);
        }

        private bool ByteArrayHasPrefix(byte[] prefix, byte[] byteArray)
        {
            if (((prefix == null) || (byteArray == null)) || (prefix.Length > byteArray.Length))
            {
                return false;
            }
            for (int i = 0; i < prefix.Length; i++)
            {
                if (prefix[i] != byteArray[i])
                {
                    return false;
                }
            }
            return true;
        }

        public void CancelAsync()
        {
            WebRequest webRequest = this.m_WebRequest;
            this.m_Cancelled = true;
            AbortRequest(webRequest);
        }

        private void ClearWebClientState()
        {
            if (this.AnotherCallInProgress(Interlocked.Increment(ref this.m_CallNesting)))
            {
                this.CompleteWebClientState();
                throw new NotSupportedException(SR.GetString("net_webclient_no_concurrent_io_allowed"));
            }
            this.m_ContentLength = -1L;
            this.m_WebResponse = null;
            this.m_WebRequest = null;
            this.m_Method = null;
            this.m_Cancelled = false;
            if (this.m_Progress != null)
            {
                this.m_Progress.Reset();
            }
        }

        private void CompleteWebClientState()
        {
            Interlocked.Decrement(ref this.m_CallNesting);
        }

        private void CopyHeadersTo(WebRequest request)
        {
            if ((this.m_headers != null) && (request is HttpWebRequest))
            {
                string str = this.m_headers["Accept"];
                string str2 = this.m_headers["Connection"];
                string str3 = this.m_headers["Content-Type"];
                string str4 = this.m_headers["Expect"];
                string str5 = this.m_headers["Referer"];
                string str6 = this.m_headers["User-Agent"];
                string str7 = this.m_headers["Host"];
                this.m_headers.RemoveInternal("Accept");
                this.m_headers.RemoveInternal("Connection");
                this.m_headers.RemoveInternal("Content-Type");
                this.m_headers.RemoveInternal("Expect");
                this.m_headers.RemoveInternal("Referer");
                this.m_headers.RemoveInternal("User-Agent");
                this.m_headers.RemoveInternal("Host");
                request.Headers = this.m_headers;
                if ((str != null) && (str.Length > 0))
                {
                    ((HttpWebRequest) request).Accept = str;
                }
                if ((str2 != null) && (str2.Length > 0))
                {
                    ((HttpWebRequest) request).Connection = str2;
                }
                if ((str3 != null) && (str3.Length > 0))
                {
                    ((HttpWebRequest) request).ContentType = str3;
                }
                if ((str4 != null) && (str4.Length > 0))
                {
                    ((HttpWebRequest) request).Expect = str4;
                }
                if ((str5 != null) && (str5.Length > 0))
                {
                    ((HttpWebRequest) request).Referer = str5;
                }
                if ((str6 != null) && (str6.Length > 0))
                {
                    ((HttpWebRequest) request).UserAgent = str6;
                }
                if (!string.IsNullOrEmpty(str7))
                {
                    ((HttpWebRequest) request).Host = str7;
                }
            }
        }

        private byte[] DownloadBits(WebRequest request, Stream writeStream, CompletionDelegate completionDelegate, AsyncOperation asyncOp)
        {
            WebResponse response = null;
            DownloadBitsState state = new DownloadBitsState(request, writeStream, completionDelegate, asyncOp, this.m_Progress, this);
            if (state.Async)
            {
                request.BeginGetResponse(new AsyncCallback(WebClient.DownloadBitsResponseCallback), state);
                return null;
            }
            response = this.m_WebResponse = this.GetWebResponse(request);
            int bytesRetrieved = state.SetResponse(response);
            while (!state.RetrieveBytes(ref bytesRetrieved))
            {
            }
            state.Close();
            return state.InnerBuffer;
        }

        private static void DownloadBitsReadCallback(IAsyncResult result)
        {
            DownloadBitsState asyncState = (DownloadBitsState) result.AsyncState;
            DownloadBitsReadCallbackState(asyncState, result);
        }

        private static void DownloadBitsReadCallbackState(DownloadBitsState state, IAsyncResult result)
        {
            Stream readStream = state.ReadStream;
            Exception exception = null;
            bool flag = false;
            try
            {
                int bytesRetrieved = 0;
                if ((readStream != null) && (readStream != Stream.Null))
                {
                    bytesRetrieved = readStream.EndRead(result);
                }
                flag = state.RetrieveBytes(ref bytesRetrieved);
            }
            catch (Exception exception2)
            {
                flag = true;
                if (((exception2 is ThreadAbortException) || (exception2 is StackOverflowException)) || (exception2 is OutOfMemoryException))
                {
                    throw;
                }
                exception = exception2;
                state.InnerBuffer = null;
                if (!(exception2 is WebException) && !(exception2 is SecurityException))
                {
                    exception = new WebException(SR.GetString("net_webclient"), exception2);
                }
                AbortRequest(state.Request);
                if ((state != null) && (state.WriteStream != null))
                {
                    state.WriteStream.Close();
                }
            }
            finally
            {
                if (flag)
                {
                    if (exception == null)
                    {
                        state.Close();
                    }
                    state.CompletionDelegate(state.InnerBuffer, exception, state.AsyncOp);
                }
            }
        }

        private static void DownloadBitsResponseCallback(IAsyncResult result)
        {
            DownloadBitsState asyncState = (DownloadBitsState) result.AsyncState;
            WebRequest request = asyncState.Request;
            Exception exception = null;
            try
            {
                WebResponse webResponse = asyncState.WebClient.GetWebResponse(request, result);
                asyncState.WebClient.m_WebResponse = webResponse;
                asyncState.SetResponse(webResponse);
            }
            catch (Exception exception2)
            {
                if (((exception2 is ThreadAbortException) || (exception2 is StackOverflowException)) || (exception2 is OutOfMemoryException))
                {
                    throw;
                }
                exception = exception2;
                if (!(exception2 is WebException) && !(exception2 is SecurityException))
                {
                    exception = new WebException(SR.GetString("net_webclient"), exception2);
                }
                AbortRequest(request);
                if ((asyncState != null) && (asyncState.WriteStream != null))
                {
                    asyncState.WriteStream.Close();
                }
            }
            finally
            {
                if (exception != null)
                {
                    asyncState.CompletionDelegate(null, exception, asyncState.AsyncOp);
                }
            }
        }

        public byte[] DownloadData(string address)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            return this.DownloadData(this.GetUri(address));
        }

        public byte[] DownloadData(Uri address)
        {
            byte[] buffer2;
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "DownloadData", address);
            }
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            this.ClearWebClientState();
            byte[] retObject = null;
            try
            {
                WebRequest request;
                retObject = this.DownloadDataInternal(address, out request);
                if (Logging.On)
                {
                    Logging.Exit(Logging.Web, this, "DownloadData", retObject);
                }
                buffer2 = retObject;
            }
            finally
            {
                this.CompleteWebClientState();
            }
            return buffer2;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void DownloadDataAsync(Uri address)
        {
            this.DownloadDataAsync(address, null);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void DownloadDataAsync(Uri address, object userToken)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "DownloadDataAsync", address);
            }
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            this.InitWebClientAsync();
            this.ClearWebClientState();
            AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(userToken);
            this.m_AsyncOp = asyncOp;
            try
            {
                WebRequest request = this.m_WebRequest = this.GetWebRequest(this.GetUri(address));
                this.DownloadBits(request, null, new CompletionDelegate(this.DownloadDataAsyncCallback), asyncOp);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (!(exception is WebException) && !(exception is SecurityException))
                {
                    exception = new WebException(SR.GetString("net_webclient"), exception);
                }
                this.DownloadDataAsyncCallback(null, exception, asyncOp);
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "DownloadDataAsync", (string) null);
            }
        }

        private void DownloadDataAsyncCallback(byte[] returnBytes, Exception exception, object state)
        {
            AsyncOperation asyncOp = (AsyncOperation) state;
            DownloadDataCompletedEventArgs eventArgs = new DownloadDataCompletedEventArgs(returnBytes, exception, this.m_Cancelled, asyncOp.UserSuppliedState);
            this.InvokeOperationCompleted(asyncOp, this.downloadDataOperationCompleted, eventArgs);
        }

        private byte[] DownloadDataInternal(Uri address, out WebRequest request)
        {
            byte[] buffer2;
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "DownloadData", address);
            }
            request = null;
            try
            {
                request = this.m_WebRequest = this.GetWebRequest(this.GetUri(address));
                buffer2 = this.DownloadBits(request, null, null, null);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (!(exception is WebException) && !(exception is SecurityException))
                {
                    exception = new WebException(SR.GetString("net_webclient"), exception);
                }
                AbortRequest(request);
                throw exception;
            }
            return buffer2;
        }

        private void DownloadDataOperationCompleted(object arg)
        {
            this.OnDownloadDataCompleted((DownloadDataCompletedEventArgs) arg);
        }

        public void DownloadFile(string address, string fileName)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            this.DownloadFile(this.GetUri(address), fileName);
        }

        public void DownloadFile(Uri address, string fileName)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "DownloadFile", address + ", " + fileName);
            }
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            WebRequest request = null;
            FileStream writeStream = null;
            bool flag = false;
            this.ClearWebClientState();
            try
            {
                writeStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                request = this.m_WebRequest = this.GetWebRequest(this.GetUri(address));
                this.DownloadBits(request, writeStream, null, null);
                flag = true;
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (!(exception is WebException) && !(exception is SecurityException))
                {
                    exception = new WebException(SR.GetString("net_webclient"), exception);
                }
                AbortRequest(request);
                throw exception;
            }
            finally
            {
                if (writeStream != null)
                {
                    writeStream.Close();
                    if (!flag)
                    {
                        System.IO.File.Delete(fileName);
                    }
                    writeStream = null;
                }
                this.CompleteWebClientState();
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "DownloadFile", "");
            }
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void DownloadFileAsync(Uri address, string fileName)
        {
            this.DownloadFileAsync(address, fileName, null);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void DownloadFileAsync(Uri address, string fileName, object userToken)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "DownloadFileAsync", address);
            }
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            FileStream writeStream = null;
            this.InitWebClientAsync();
            this.ClearWebClientState();
            AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(userToken);
            this.m_AsyncOp = asyncOp;
            try
            {
                writeStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                WebRequest request = this.m_WebRequest = this.GetWebRequest(this.GetUri(address));
                this.DownloadBits(request, writeStream, new CompletionDelegate(this.DownloadFileAsyncCallback), asyncOp);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (writeStream != null)
                {
                    writeStream.Close();
                }
                if (!(exception is WebException) && !(exception is SecurityException))
                {
                    exception = new WebException(SR.GetString("net_webclient"), exception);
                }
                this.DownloadFileAsyncCallback(null, exception, asyncOp);
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "DownloadFileAsync", (string) null);
            }
        }

        private void DownloadFileAsyncCallback(byte[] returnBytes, Exception exception, object state)
        {
            AsyncOperation asyncOp = (AsyncOperation) state;
            AsyncCompletedEventArgs eventArgs = new AsyncCompletedEventArgs(exception, this.m_Cancelled, asyncOp.UserSuppliedState);
            this.InvokeOperationCompleted(asyncOp, this.downloadFileOperationCompleted, eventArgs);
        }

        private void DownloadFileOperationCompleted(object arg)
        {
            this.OnDownloadFileCompleted((AsyncCompletedEventArgs) arg);
        }

        public string DownloadString(string address)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            return this.DownloadString(this.GetUri(address));
        }

        public string DownloadString(Uri address)
        {
            string str2;
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "DownloadString", address);
            }
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            this.ClearWebClientState();
            try
            {
                WebRequest request;
                byte[] data = this.DownloadDataInternal(address, out request);
                string stringUsingEncoding = this.GetStringUsingEncoding(request, data);
                if (Logging.On)
                {
                    Logging.Exit(Logging.Web, this, "DownloadString", stringUsingEncoding);
                }
                str2 = stringUsingEncoding;
            }
            finally
            {
                this.CompleteWebClientState();
            }
            return str2;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void DownloadStringAsync(Uri address)
        {
            this.DownloadStringAsync(address, null);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void DownloadStringAsync(Uri address, object userToken)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "DownloadStringAsync", address);
            }
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            this.InitWebClientAsync();
            this.ClearWebClientState();
            AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(userToken);
            this.m_AsyncOp = asyncOp;
            try
            {
                WebRequest request = this.m_WebRequest = this.GetWebRequest(this.GetUri(address));
                this.DownloadBits(request, null, new CompletionDelegate(this.DownloadStringAsyncCallback), asyncOp);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (!(exception is WebException) && !(exception is SecurityException))
                {
                    exception = new WebException(SR.GetString("net_webclient"), exception);
                }
                this.DownloadStringAsyncCallback(null, exception, asyncOp);
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "DownloadStringAsync", "");
            }
        }

        private void DownloadStringAsyncCallback(byte[] returnBytes, Exception exception, object state)
        {
            AsyncOperation asyncOp = (AsyncOperation) state;
            string result = null;
            try
            {
                if (returnBytes != null)
                {
                    result = this.GetStringUsingEncoding(this.m_WebRequest, returnBytes);
                }
            }
            catch (Exception exception2)
            {
                if (((exception2 is ThreadAbortException) || (exception2 is StackOverflowException)) || (exception2 is OutOfMemoryException))
                {
                    throw;
                }
                exception = exception2;
            }
            DownloadStringCompletedEventArgs eventArgs = new DownloadStringCompletedEventArgs(result, exception, this.m_Cancelled, asyncOp.UserSuppliedState);
            this.InvokeOperationCompleted(asyncOp, this.downloadStringOperationCompleted, eventArgs);
        }

        private void DownloadStringOperationCompleted(object arg)
        {
            this.OnDownloadStringCompleted((DownloadStringCompletedEventArgs) arg);
        }

        private string GetStringUsingEncoding(WebRequest request, byte[] data)
        {
            System.Text.Encoding encoding = null;
            string contentType;
            int index = -1;
            try
            {
                contentType = request.ContentType;
            }
            catch (NotImplementedException)
            {
                contentType = null;
            }
            catch (NotSupportedException)
            {
                contentType = null;
            }
            if (contentType != null)
            {
                string[] strArray = contentType.ToLower(CultureInfo.InvariantCulture).Split(new char[] { ';', '=', ' ' });
                bool flag = false;
                foreach (string str2 in strArray)
                {
                    if (str2 == "charset")
                    {
                        flag = true;
                    }
                    else if (flag)
                    {
                        try
                        {
                            encoding = System.Text.Encoding.GetEncoding(str2);
                        }
                        catch (ArgumentException)
                        {
                            break;
                        }
                    }
                }
            }
            if (encoding == null)
            {
                System.Text.Encoding[] encodingArray = new System.Text.Encoding[] { System.Text.Encoding.UTF8, System.Text.Encoding.Unicode, System.Text.Encoding.BigEndianUnicode, System.Text.Encoding.UTF32 };
                for (int i = 0; i < encodingArray.Length; i++)
                {
                    byte[] preamble = encodingArray[i].GetPreamble();
                    if (this.ByteArrayHasPrefix(preamble, data))
                    {
                        encoding = encodingArray[i];
                        index = preamble.Length;
                        break;
                    }
                }
            }
            if (encoding == null)
            {
                encoding = this.Encoding;
            }
            if (index == -1)
            {
                byte[] prefix = encoding.GetPreamble();
                if (this.ByteArrayHasPrefix(prefix, data))
                {
                    index = prefix.Length;
                }
                else
                {
                    index = 0;
                }
            }
            return encoding.GetString(data, index, data.Length - index);
        }

        private Uri GetUri(string path)
        {
            Uri uri;
            if (this.m_baseAddress != null)
            {
                if (!Uri.TryCreate(this.m_baseAddress, path, out uri))
                {
                    return new Uri(Path.GetFullPath(path));
                }
            }
            else if (!Uri.TryCreate(path, UriKind.Absolute, out uri))
            {
                return new Uri(Path.GetFullPath(path));
            }
            return this.GetUri(uri);
        }

        private Uri GetUri(Uri address)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            Uri result = address;
            if ((!address.IsAbsoluteUri && (this.m_baseAddress != null)) && !Uri.TryCreate(this.m_baseAddress, address, out result))
            {
                return address;
            }
            if ((result.Query != null) && !(result.Query == string.Empty))
            {
                return result;
            }
            if (this.m_requestParameters == null)
            {
                return result;
            }
            StringBuilder builder = new StringBuilder();
            string str = string.Empty;
            for (int i = 0; i < this.m_requestParameters.Count; i++)
            {
                builder.Append(str + this.m_requestParameters.AllKeys[i] + "=" + this.m_requestParameters[i]);
                str = "&";
            }
            UriBuilder builder2 = new UriBuilder(result) {
                Query = builder.ToString()
            };
            return builder2.Uri;
        }

        protected virtual WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = WebRequest.Create(address);
            this.CopyHeadersTo(request);
            if (this.Credentials != null)
            {
                request.Credentials = this.Credentials;
            }
            if (this.m_Method != null)
            {
                request.Method = this.m_Method;
            }
            if (this.m_ContentLength != -1L)
            {
                request.ContentLength = this.m_ContentLength;
            }
            if (this.m_ProxySet)
            {
                request.Proxy = this.m_Proxy;
            }
            if (this.m_CachePolicy != null)
            {
                request.CachePolicy = this.m_CachePolicy;
            }
            return request;
        }

        protected virtual WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = request.GetResponse();
            this.m_WebResponse = response;
            return response;
        }

        protected virtual WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse response = request.EndGetResponse(result);
            this.m_WebResponse = response;
            return response;
        }

        private void InitWebClientAsync()
        {
            if (!this.m_InitWebClientAsync)
            {
                this.openReadOperationCompleted = new SendOrPostCallback(this.OpenReadOperationCompleted);
                this.openWriteOperationCompleted = new SendOrPostCallback(this.OpenWriteOperationCompleted);
                this.downloadStringOperationCompleted = new SendOrPostCallback(this.DownloadStringOperationCompleted);
                this.downloadDataOperationCompleted = new SendOrPostCallback(this.DownloadDataOperationCompleted);
                this.downloadFileOperationCompleted = new SendOrPostCallback(this.DownloadFileOperationCompleted);
                this.uploadStringOperationCompleted = new SendOrPostCallback(this.UploadStringOperationCompleted);
                this.uploadDataOperationCompleted = new SendOrPostCallback(this.UploadDataOperationCompleted);
                this.uploadFileOperationCompleted = new SendOrPostCallback(this.UploadFileOperationCompleted);
                this.uploadValuesOperationCompleted = new SendOrPostCallback(this.UploadValuesOperationCompleted);
                this.reportDownloadProgressChanged = new SendOrPostCallback(this.ReportDownloadProgressChanged);
                this.reportUploadProgressChanged = new SendOrPostCallback(this.ReportUploadProgressChanged);
                this.m_Progress = new ProgressData();
                this.m_InitWebClientAsync = true;
            }
        }

        private static char IntToHex(int n)
        {
            if (n <= 9)
            {
                return (char) (n + 0x30);
            }
            return (char) ((n - 10) + 0x61);
        }

        private void InvokeOperationCompleted(AsyncOperation asyncOp, SendOrPostCallback callback, AsyncCompletedEventArgs eventArgs)
        {
            if (Interlocked.CompareExchange<AsyncOperation>(ref this.m_AsyncOp, null, asyncOp) == asyncOp)
            {
                this.CompleteWebClientState();
                asyncOp.PostOperationCompleted(callback, eventArgs);
            }
        }

        private static bool IsSafe(char ch)
        {
            if ((((ch >= 'a') && (ch <= 'z')) || ((ch >= 'A') && (ch <= 'Z'))) || ((ch >= '0') && (ch <= '9')))
            {
                return true;
            }
            switch (ch)
            {
                case '\'':
                case '(':
                case ')':
                case '*':
                case '-':
                case '.':
                case '_':
                case '!':
                    return true;
            }
            return false;
        }

        private string MapToDefaultMethod(Uri address)
        {
            Uri uri;
            if (!address.IsAbsoluteUri && (this.m_baseAddress != null))
            {
                uri = new Uri(this.m_baseAddress, address);
            }
            else
            {
                uri = address;
            }
            if (uri.Scheme.ToLower(CultureInfo.InvariantCulture) == "ftp")
            {
                return "STOR";
            }
            return "POST";
        }

        protected virtual void OnDownloadDataCompleted(DownloadDataCompletedEventArgs e)
        {
            if (this.DownloadDataCompleted != null)
            {
                this.DownloadDataCompleted(this, e);
            }
        }

        protected virtual void OnDownloadFileCompleted(AsyncCompletedEventArgs e)
        {
            if (this.DownloadFileCompleted != null)
            {
                this.DownloadFileCompleted(this, e);
            }
        }

        protected virtual void OnDownloadProgressChanged(DownloadProgressChangedEventArgs e)
        {
            if (this.DownloadProgressChanged != null)
            {
                this.DownloadProgressChanged(this, e);
            }
        }

        protected virtual void OnDownloadStringCompleted(DownloadStringCompletedEventArgs e)
        {
            if (this.DownloadStringCompleted != null)
            {
                this.DownloadStringCompleted(this, e);
            }
        }

        protected virtual void OnOpenReadCompleted(OpenReadCompletedEventArgs e)
        {
            if (this.OpenReadCompleted != null)
            {
                this.OpenReadCompleted(this, e);
            }
        }

        protected virtual void OnOpenWriteCompleted(OpenWriteCompletedEventArgs e)
        {
            if (this.OpenWriteCompleted != null)
            {
                this.OpenWriteCompleted(this, e);
            }
        }

        protected virtual void OnUploadDataCompleted(UploadDataCompletedEventArgs e)
        {
            if (this.UploadDataCompleted != null)
            {
                this.UploadDataCompleted(this, e);
            }
        }

        protected virtual void OnUploadFileCompleted(UploadFileCompletedEventArgs e)
        {
            if (this.UploadFileCompleted != null)
            {
                this.UploadFileCompleted(this, e);
            }
        }

        protected virtual void OnUploadProgressChanged(UploadProgressChangedEventArgs e)
        {
            if (this.UploadProgressChanged != null)
            {
                this.UploadProgressChanged(this, e);
            }
        }

        protected virtual void OnUploadStringCompleted(UploadStringCompletedEventArgs e)
        {
            if (this.UploadStringCompleted != null)
            {
                this.UploadStringCompleted(this, e);
            }
        }

        protected virtual void OnUploadValuesCompleted(UploadValuesCompletedEventArgs e)
        {
            if (this.UploadValuesCompleted != null)
            {
                this.UploadValuesCompleted(this, e);
            }
        }

        private void OpenFileInternal(bool needsHeaderAndBoundary, string fileName, ref FileStream fs, ref byte[] buffer, ref byte[] formHeaderBytes, ref byte[] boundaryBytes)
        {
            fileName = Path.GetFullPath(fileName);
            if (this.m_headers == null)
            {
                this.m_headers = new WebHeaderCollection(WebHeaderCollectionType.WebRequest);
            }
            string str = this.m_headers["Content-Type"];
            if (str != null)
            {
                if (str.ToLower(CultureInfo.InvariantCulture).StartsWith("multipart/"))
                {
                    throw new WebException(SR.GetString("net_webclient_Multipart"));
                }
            }
            else
            {
                str = "application/octet-stream";
            }
            fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            int num = 0x2000;
            this.m_ContentLength = -1L;
            if (this.m_Method.ToUpper(CultureInfo.InvariantCulture) == "POST")
            {
                if (needsHeaderAndBoundary)
                {
                    string str2 = "---------------------" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);
                    this.m_headers["Content-Type"] = "multipart/form-data; boundary=" + str2;
                    string s = "--" + str2 + "\r\nContent-Disposition: form-data; name=\"file\"; filename=\"" + Path.GetFileName(fileName) + "\"\r\nContent-Type: " + str + "\r\n\r\n";
                    formHeaderBytes = System.Text.Encoding.UTF8.GetBytes(s);
                    boundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + str2 + "--\r\n");
                }
                else
                {
                    formHeaderBytes = new byte[0];
                    boundaryBytes = new byte[0];
                }
                if (fs.CanSeek)
                {
                    this.m_ContentLength = (fs.Length + formHeaderBytes.Length) + boundaryBytes.Length;
                    num = (int) Math.Min(0x2000L, fs.Length);
                }
            }
            else
            {
                this.m_headers["Content-Type"] = str;
                formHeaderBytes = null;
                boundaryBytes = null;
                if (fs.CanSeek)
                {
                    this.m_ContentLength = fs.Length;
                    num = (int) Math.Min(0x2000L, fs.Length);
                }
            }
            buffer = new byte[num];
        }

        public Stream OpenRead(string address)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            return this.OpenRead(this.GetUri(address));
        }

        public Stream OpenRead(Uri address)
        {
            Stream stream2;
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "OpenRead", address);
            }
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            WebRequest request = null;
            this.ClearWebClientState();
            try
            {
                request = this.m_WebRequest = this.GetWebRequest(this.GetUri(address));
                Stream responseStream = (this.m_WebResponse = this.GetWebResponse(request)).GetResponseStream();
                if (Logging.On)
                {
                    Logging.Exit(Logging.Web, this, "OpenRead", responseStream);
                }
                stream2 = responseStream;
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (!(exception is WebException) && !(exception is SecurityException))
                {
                    exception = new WebException(SR.GetString("net_webclient"), exception);
                }
                AbortRequest(request);
                throw exception;
            }
            finally
            {
                this.CompleteWebClientState();
            }
            return stream2;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void OpenReadAsync(Uri address)
        {
            this.OpenReadAsync(address, null);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void OpenReadAsync(Uri address, object userToken)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "OpenReadAsync", address);
            }
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            this.InitWebClientAsync();
            this.ClearWebClientState();
            AsyncOperation state = AsyncOperationManager.CreateOperation(userToken);
            this.m_AsyncOp = state;
            try
            {
                (this.m_WebRequest = this.GetWebRequest(this.GetUri(address))).BeginGetResponse(new AsyncCallback(this.OpenReadAsyncCallback), state);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (!(exception is WebException) && !(exception is SecurityException))
                {
                    exception = new WebException(SR.GetString("net_webclient"), exception);
                }
                OpenReadCompletedEventArgs eventArgs = new OpenReadCompletedEventArgs(null, exception, this.m_Cancelled, state.UserSuppliedState);
                this.InvokeOperationCompleted(state, this.openReadOperationCompleted, eventArgs);
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "OpenReadAsync", (string) null);
            }
        }

        private void OpenReadAsyncCallback(IAsyncResult result)
        {
            LazyAsyncResult result2 = (LazyAsyncResult) result;
            AsyncOperation asyncState = (AsyncOperation) result2.AsyncState;
            WebRequest asyncObject = (WebRequest) result2.AsyncObject;
            Stream responseStream = null;
            Exception exception = null;
            try
            {
                responseStream = (this.m_WebResponse = this.GetWebResponse(asyncObject, result)).GetResponseStream();
            }
            catch (Exception exception2)
            {
                if (((exception2 is ThreadAbortException) || (exception2 is StackOverflowException)) || (exception2 is OutOfMemoryException))
                {
                    throw;
                }
                exception = exception2;
                if (!(exception2 is WebException) && !(exception2 is SecurityException))
                {
                    exception = new WebException(SR.GetString("net_webclient"), exception2);
                }
            }
            OpenReadCompletedEventArgs eventArgs = new OpenReadCompletedEventArgs(responseStream, exception, this.m_Cancelled, asyncState.UserSuppliedState);
            this.InvokeOperationCompleted(asyncState, this.openReadOperationCompleted, eventArgs);
        }

        private void OpenReadOperationCompleted(object arg)
        {
            this.OnOpenReadCompleted((OpenReadCompletedEventArgs) arg);
        }

        public Stream OpenWrite(string address)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            return this.OpenWrite(this.GetUri(address), null);
        }

        public Stream OpenWrite(Uri address)
        {
            return this.OpenWrite(address, null);
        }

        public Stream OpenWrite(string address, string method)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            return this.OpenWrite(this.GetUri(address), method);
        }

        public Stream OpenWrite(Uri address, string method)
        {
            Stream stream2;
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "OpenWrite", address + ", " + method);
            }
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (method == null)
            {
                method = this.MapToDefaultMethod(address);
            }
            WebRequest request = null;
            this.ClearWebClientState();
            try
            {
                this.m_Method = method;
                request = this.m_WebRequest = this.GetWebRequest(this.GetUri(address));
                WebClientWriteStream retObject = new WebClientWriteStream(request.GetRequestStream(), request, this);
                if (Logging.On)
                {
                    Logging.Exit(Logging.Web, this, "OpenWrite", retObject);
                }
                stream2 = retObject;
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (!(exception is WebException) && !(exception is SecurityException))
                {
                    exception = new WebException(SR.GetString("net_webclient"), exception);
                }
                AbortRequest(request);
                throw exception;
            }
            finally
            {
                this.CompleteWebClientState();
            }
            return stream2;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void OpenWriteAsync(Uri address)
        {
            this.OpenWriteAsync(address, null, null);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void OpenWriteAsync(Uri address, string method)
        {
            this.OpenWriteAsync(address, method, null);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void OpenWriteAsync(Uri address, string method, object userToken)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "OpenWriteAsync", address + ", " + method);
            }
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (method == null)
            {
                method = this.MapToDefaultMethod(address);
            }
            this.InitWebClientAsync();
            this.ClearWebClientState();
            AsyncOperation state = AsyncOperationManager.CreateOperation(userToken);
            this.m_AsyncOp = state;
            try
            {
                this.m_Method = method;
                (this.m_WebRequest = this.GetWebRequest(this.GetUri(address))).BeginGetRequestStream(new AsyncCallback(this.OpenWriteAsyncCallback), state);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (!(exception is WebException) && !(exception is SecurityException))
                {
                    exception = new WebException(SR.GetString("net_webclient"), exception);
                }
                OpenWriteCompletedEventArgs eventArgs = new OpenWriteCompletedEventArgs(null, exception, this.m_Cancelled, state.UserSuppliedState);
                this.InvokeOperationCompleted(state, this.openWriteOperationCompleted, eventArgs);
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "OpenWriteAsync", (string) null);
            }
        }

        private void OpenWriteAsyncCallback(IAsyncResult result)
        {
            LazyAsyncResult result2 = (LazyAsyncResult) result;
            AsyncOperation asyncState = (AsyncOperation) result2.AsyncState;
            WebRequest asyncObject = (WebRequest) result2.AsyncObject;
            WebClientWriteStream stream = null;
            Exception exception = null;
            try
            {
                stream = new WebClientWriteStream(asyncObject.EndGetRequestStream(result), asyncObject, this);
            }
            catch (Exception exception2)
            {
                if (((exception2 is ThreadAbortException) || (exception2 is StackOverflowException)) || (exception2 is OutOfMemoryException))
                {
                    throw;
                }
                exception = exception2;
                if (!(exception2 is WebException) && !(exception2 is SecurityException))
                {
                    exception = new WebException(SR.GetString("net_webclient"), exception2);
                }
            }
            OpenWriteCompletedEventArgs eventArgs = new OpenWriteCompletedEventArgs(stream, exception, this.m_Cancelled, asyncState.UserSuppliedState);
            this.InvokeOperationCompleted(asyncState, this.openWriteOperationCompleted, eventArgs);
        }

        private void OpenWriteOperationCompleted(object arg)
        {
            this.OnOpenWriteCompleted((OpenWriteCompletedEventArgs) arg);
        }

        private void PostProgressChanged(AsyncOperation asyncOp, ProgressData progress)
        {
            if ((asyncOp != null) && ((progress.BytesSent + progress.BytesReceived) > 0L))
            {
                int num;
                if (progress.HasUploadPhase)
                {
                    if ((progress.TotalBytesToReceive < 0L) && (progress.BytesReceived == 0L))
                    {
                        num = (progress.TotalBytesToSend < 0L) ? 0 : ((progress.TotalBytesToSend == 0L) ? 50 : ((int) ((50L * progress.BytesSent) / progress.TotalBytesToSend)));
                    }
                    else
                    {
                        num = (progress.TotalBytesToSend < 0L) ? 50 : ((progress.TotalBytesToReceive == 0L) ? 100 : ((int) (((50L * progress.BytesReceived) / progress.TotalBytesToReceive) + 50L)));
                    }
                    asyncOp.Post(this.reportUploadProgressChanged, new UploadProgressChangedEventArgs(num, asyncOp.UserSuppliedState, progress.BytesSent, progress.TotalBytesToSend, progress.BytesReceived, progress.TotalBytesToReceive));
                }
                else
                {
                    num = (progress.TotalBytesToReceive < 0L) ? 0 : ((progress.TotalBytesToReceive == 0L) ? 100 : ((int) ((100L * progress.BytesReceived) / progress.TotalBytesToReceive)));
                    asyncOp.Post(this.reportDownloadProgressChanged, new DownloadProgressChangedEventArgs(num, asyncOp.UserSuppliedState, progress.BytesReceived, progress.TotalBytesToReceive));
                }
            }
        }

        private void ReportDownloadProgressChanged(object arg)
        {
            this.OnDownloadProgressChanged((DownloadProgressChangedEventArgs) arg);
        }

        private void ReportUploadProgressChanged(object arg)
        {
            this.OnUploadProgressChanged((UploadProgressChangedEventArgs) arg);
        }

        private void StartDownloadAsync(UploadBitsState state)
        {
            try
            {
                this.DownloadBits(state.Request, null, state.DownloadCompletionDelegate, state.AsyncOp);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (!(exception is WebException) && !(exception is SecurityException))
                {
                    exception = new WebException(SR.GetString("net_webclient"), exception);
                }
                state.DownloadCompletionDelegate(null, exception, state.AsyncOp);
            }
        }

        private void UploadBits(WebRequest request, Stream readStream, byte[] buffer, int chunkSize, byte[] header, byte[] footer, CompletionDelegate uploadCompletionDelegate, CompletionDelegate downloadCompletionDelegate, AsyncOperation asyncOp)
        {
            if (request.RequestUri.Scheme == Uri.UriSchemeFile)
            {
                header = (byte[]) (footer = null);
            }
            UploadBitsState state = new UploadBitsState(request, readStream, buffer, chunkSize, header, footer, uploadCompletionDelegate, downloadCompletionDelegate, asyncOp, this.m_Progress, this);
            if (state.Async)
            {
                request.BeginGetRequestStream(new AsyncCallback(WebClient.UploadBitsRequestCallback), state);
            }
            else
            {
                Stream requestStream = request.GetRequestStream();
                state.SetRequestStream(requestStream);
                while (!state.WriteBytes())
                {
                }
                state.Close();
            }
        }

        private static void UploadBitsRequestCallback(IAsyncResult result)
        {
            UploadBitsState asyncState = (UploadBitsState) result.AsyncState;
            WebRequest request = asyncState.Request;
            Exception exception = null;
            try
            {
                Stream writeStream = request.EndGetRequestStream(result);
                asyncState.SetRequestStream(writeStream);
            }
            catch (Exception exception2)
            {
                if (((exception2 is ThreadAbortException) || (exception2 is StackOverflowException)) || (exception2 is OutOfMemoryException))
                {
                    throw;
                }
                exception = exception2;
                if (!(exception2 is WebException) && !(exception2 is SecurityException))
                {
                    exception = new WebException(SR.GetString("net_webclient"), exception2);
                }
                AbortRequest(request);
                if ((asyncState != null) && (asyncState.ReadStream != null))
                {
                    asyncState.ReadStream.Close();
                }
            }
            finally
            {
                if (exception != null)
                {
                    asyncState.UploadCompletionDelegate(null, exception, asyncState);
                }
            }
        }

        private static void UploadBitsWriteCallback(IAsyncResult result)
        {
            UploadBitsState asyncState = (UploadBitsState) result.AsyncState;
            Stream writeStream = asyncState.WriteStream;
            Exception exception = null;
            bool flag = false;
            try
            {
                writeStream.EndWrite(result);
                flag = asyncState.WriteBytes();
            }
            catch (Exception exception2)
            {
                flag = true;
                if (((exception2 is ThreadAbortException) || (exception2 is StackOverflowException)) || (exception2 is OutOfMemoryException))
                {
                    throw;
                }
                exception = exception2;
                if (!(exception2 is WebException) && !(exception2 is SecurityException))
                {
                    exception = new WebException(SR.GetString("net_webclient"), exception2);
                }
                AbortRequest(asyncState.Request);
                if ((asyncState != null) && (asyncState.ReadStream != null))
                {
                    asyncState.ReadStream.Close();
                }
            }
            finally
            {
                if (flag)
                {
                    if (exception == null)
                    {
                        asyncState.Close();
                    }
                    asyncState.UploadCompletionDelegate(null, exception, asyncState);
                }
            }
        }

        public byte[] UploadData(string address, byte[] data)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            return this.UploadData(this.GetUri(address), null, data);
        }

        public byte[] UploadData(Uri address, byte[] data)
        {
            return this.UploadData(address, null, data);
        }

        public byte[] UploadData(string address, string method, byte[] data)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            return this.UploadData(this.GetUri(address), method, data);
        }

        public byte[] UploadData(Uri address, string method, byte[] data)
        {
            byte[] buffer2;
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "UploadData", address + ", " + method);
            }
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if (method == null)
            {
                method = this.MapToDefaultMethod(address);
            }
            this.ClearWebClientState();
            try
            {
                WebRequest request;
                byte[] retObject = this.UploadDataInternal(address, method, data, out request);
                if (Logging.On)
                {
                    Logging.Exit(Logging.Web, this, "UploadData", retObject);
                }
                buffer2 = retObject;
            }
            finally
            {
                this.CompleteWebClientState();
            }
            return buffer2;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void UploadDataAsync(Uri address, byte[] data)
        {
            this.UploadDataAsync(address, null, data, null);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void UploadDataAsync(Uri address, string method, byte[] data)
        {
            this.UploadDataAsync(address, method, data, null);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void UploadDataAsync(Uri address, string method, byte[] data, object userToken)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "UploadDataAsync", address + ", " + method);
            }
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if (method == null)
            {
                method = this.MapToDefaultMethod(address);
            }
            this.InitWebClientAsync();
            this.ClearWebClientState();
            AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(userToken);
            this.m_AsyncOp = asyncOp;
            int chunkSize = 0;
            try
            {
                this.m_Method = method;
                this.m_ContentLength = data.Length;
                WebRequest request = this.m_WebRequest = this.GetWebRequest(this.GetUri(address));
                if (this.UploadProgressChanged != null)
                {
                    chunkSize = (int) Math.Min(0x2000L, (long) data.Length);
                }
                this.UploadBits(request, null, data, chunkSize, null, null, new CompletionDelegate(this.UploadDataAsyncWriteCallback), new CompletionDelegate(this.UploadDataAsyncReadCallback), asyncOp);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (!(exception is WebException) && !(exception is SecurityException))
                {
                    exception = new WebException(SR.GetString("net_webclient"), exception);
                }
                this.UploadDataAsyncWriteCallback(null, exception, asyncOp);
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "UploadDataAsync", (string) null);
            }
        }

        private void UploadDataAsyncReadCallback(byte[] returnBytes, Exception exception, object state)
        {
            AsyncOperation asyncOp = (AsyncOperation) state;
            UploadDataCompletedEventArgs eventArgs = new UploadDataCompletedEventArgs(returnBytes, exception, this.m_Cancelled, asyncOp.UserSuppliedState);
            this.InvokeOperationCompleted(asyncOp, this.uploadDataOperationCompleted, eventArgs);
        }

        private void UploadDataAsyncWriteCallback(byte[] returnBytes, Exception exception, object state)
        {
            UploadBitsState state2 = (UploadBitsState) state;
            if (exception != null)
            {
                UploadDataCompletedEventArgs eventArgs = new UploadDataCompletedEventArgs(returnBytes, exception, this.m_Cancelled, state2.AsyncOp.UserSuppliedState);
                this.InvokeOperationCompleted(state2.AsyncOp, this.uploadDataOperationCompleted, eventArgs);
            }
            else
            {
                this.StartDownloadAsync(state2);
            }
        }

        private byte[] UploadDataInternal(Uri address, string method, byte[] data, out WebRequest request)
        {
            byte[] buffer2;
            request = null;
            try
            {
                this.m_Method = method;
                this.m_ContentLength = data.Length;
                request = this.m_WebRequest = this.GetWebRequest(this.GetUri(address));
                this.UploadBits(request, null, data, 0, null, null, null, null, null);
                buffer2 = this.DownloadBits(request, null, null, null);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (!(exception is WebException) && !(exception is SecurityException))
                {
                    exception = new WebException(SR.GetString("net_webclient"), exception);
                }
                AbortRequest(request);
                throw exception;
            }
            return buffer2;
        }

        private void UploadDataOperationCompleted(object arg)
        {
            this.OnUploadDataCompleted((UploadDataCompletedEventArgs) arg);
        }

        public byte[] UploadFile(string address, string fileName)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            return this.UploadFile(this.GetUri(address), fileName);
        }

        public byte[] UploadFile(Uri address, string fileName)
        {
            return this.UploadFile(address, null, fileName);
        }

        public byte[] UploadFile(string address, string method, string fileName)
        {
            return this.UploadFile(this.GetUri(address), method, fileName);
        }

        public byte[] UploadFile(Uri address, string method, string fileName)
        {
            byte[] buffer5;
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "UploadFile", address + ", " + method);
            }
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            if (method == null)
            {
                method = this.MapToDefaultMethod(address);
            }
            FileStream fs = null;
            WebRequest request = null;
            this.ClearWebClientState();
            try
            {
                this.m_Method = method;
                byte[] formHeaderBytes = null;
                byte[] boundaryBytes = null;
                byte[] buffer = null;
                Uri uri = this.GetUri(address);
                bool needsHeaderAndBoundary = uri.Scheme != Uri.UriSchemeFile;
                this.OpenFileInternal(needsHeaderAndBoundary, fileName, ref fs, ref buffer, ref formHeaderBytes, ref boundaryBytes);
                request = this.m_WebRequest = this.GetWebRequest(uri);
                this.UploadBits(request, fs, buffer, 0, formHeaderBytes, boundaryBytes, null, null, null);
                byte[] retObject = this.DownloadBits(request, null, null, null);
                if (Logging.On)
                {
                    Logging.Exit(Logging.Web, this, "UploadFile", retObject);
                }
                buffer5 = retObject;
            }
            catch (Exception exception)
            {
                if (fs != null)
                {
                    fs.Close();
                    fs = null;
                }
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (!(exception is WebException) && !(exception is SecurityException))
                {
                    exception = new WebException(SR.GetString("net_webclient"), exception);
                }
                AbortRequest(request);
                throw exception;
            }
            finally
            {
                this.CompleteWebClientState();
            }
            return buffer5;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void UploadFileAsync(Uri address, string fileName)
        {
            this.UploadFileAsync(address, null, fileName, null);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void UploadFileAsync(Uri address, string method, string fileName)
        {
            this.UploadFileAsync(address, method, fileName, null);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void UploadFileAsync(Uri address, string method, string fileName, object userToken)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "UploadFileAsync", address + ", " + method);
            }
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            if (method == null)
            {
                method = this.MapToDefaultMethod(address);
            }
            this.InitWebClientAsync();
            this.ClearWebClientState();
            AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(userToken);
            this.m_AsyncOp = asyncOp;
            FileStream fs = null;
            try
            {
                this.m_Method = method;
                byte[] formHeaderBytes = null;
                byte[] boundaryBytes = null;
                byte[] buffer = null;
                Uri uri = this.GetUri(address);
                bool needsHeaderAndBoundary = uri.Scheme != Uri.UriSchemeFile;
                this.OpenFileInternal(needsHeaderAndBoundary, fileName, ref fs, ref buffer, ref formHeaderBytes, ref boundaryBytes);
                WebRequest request = this.m_WebRequest = this.GetWebRequest(uri);
                this.UploadBits(request, fs, buffer, 0, formHeaderBytes, boundaryBytes, new CompletionDelegate(this.UploadFileAsyncWriteCallback), new CompletionDelegate(this.UploadFileAsyncReadCallback), asyncOp);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (fs != null)
                {
                    fs.Close();
                }
                if (!(exception is WebException) && !(exception is SecurityException))
                {
                    exception = new WebException(SR.GetString("net_webclient"), exception);
                }
                this.UploadFileAsyncWriteCallback(null, exception, asyncOp);
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "UploadFileAsync", (string) null);
            }
        }

        private void UploadFileAsyncReadCallback(byte[] returnBytes, Exception exception, object state)
        {
            AsyncOperation asyncOp = (AsyncOperation) state;
            UploadFileCompletedEventArgs eventArgs = new UploadFileCompletedEventArgs(returnBytes, exception, this.m_Cancelled, asyncOp.UserSuppliedState);
            this.InvokeOperationCompleted(asyncOp, this.uploadFileOperationCompleted, eventArgs);
        }

        private void UploadFileAsyncWriteCallback(byte[] returnBytes, Exception exception, object state)
        {
            UploadBitsState state2 = (UploadBitsState) state;
            if (exception != null)
            {
                UploadFileCompletedEventArgs eventArgs = new UploadFileCompletedEventArgs(returnBytes, exception, this.m_Cancelled, state2.AsyncOp.UserSuppliedState);
                this.InvokeOperationCompleted(state2.AsyncOp, this.uploadFileOperationCompleted, eventArgs);
            }
            else
            {
                this.StartDownloadAsync(state2);
            }
        }

        private void UploadFileOperationCompleted(object arg)
        {
            this.OnUploadFileCompleted((UploadFileCompletedEventArgs) arg);
        }

        public string UploadString(string address, string data)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            return this.UploadString(this.GetUri(address), null, data);
        }

        public string UploadString(Uri address, string data)
        {
            return this.UploadString(address, null, data);
        }

        public string UploadString(string address, string method, string data)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            return this.UploadString(this.GetUri(address), method, data);
        }

        public string UploadString(Uri address, string method, string data)
        {
            string str2;
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "UploadString", address + ", " + method);
            }
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if (method == null)
            {
                method = this.MapToDefaultMethod(address);
            }
            this.ClearWebClientState();
            try
            {
                WebRequest request;
                byte[] bytes = this.Encoding.GetBytes(data);
                byte[] buffer2 = this.UploadDataInternal(address, method, bytes, out request);
                string stringUsingEncoding = this.GetStringUsingEncoding(request, buffer2);
                if (Logging.On)
                {
                    Logging.Exit(Logging.Web, this, "UploadString", stringUsingEncoding);
                }
                str2 = stringUsingEncoding;
            }
            finally
            {
                this.CompleteWebClientState();
            }
            return str2;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void UploadStringAsync(Uri address, string data)
        {
            this.UploadStringAsync(address, null, data, null);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void UploadStringAsync(Uri address, string method, string data)
        {
            this.UploadStringAsync(address, method, data, null);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void UploadStringAsync(Uri address, string method, string data, object userToken)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "UploadStringAsync", address);
            }
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if (method == null)
            {
                method = this.MapToDefaultMethod(address);
            }
            this.InitWebClientAsync();
            this.ClearWebClientState();
            AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(userToken);
            this.m_AsyncOp = asyncOp;
            try
            {
                byte[] bytes = this.Encoding.GetBytes(data);
                this.m_Method = method;
                this.m_ContentLength = bytes.Length;
                WebRequest request = this.m_WebRequest = this.GetWebRequest(this.GetUri(address));
                this.UploadBits(request, null, bytes, 0, null, null, new CompletionDelegate(this.UploadStringAsyncWriteCallback), new CompletionDelegate(this.UploadStringAsyncReadCallback), asyncOp);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (!(exception is WebException) && !(exception is SecurityException))
                {
                    exception = new WebException(SR.GetString("net_webclient"), exception);
                }
                this.UploadStringAsyncWriteCallback(null, exception, asyncOp);
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "UploadStringAsync", (string) null);
            }
        }

        private void UploadStringAsyncReadCallback(byte[] returnBytes, Exception exception, object state)
        {
            AsyncOperation asyncOp = (AsyncOperation) state;
            string result = null;
            try
            {
                if (returnBytes != null)
                {
                    result = this.GetStringUsingEncoding(this.m_WebRequest, returnBytes);
                }
            }
            catch (Exception exception2)
            {
                if (((exception2 is ThreadAbortException) || (exception2 is StackOverflowException)) || (exception2 is OutOfMemoryException))
                {
                    throw;
                }
                exception = exception2;
            }
            UploadStringCompletedEventArgs eventArgs = new UploadStringCompletedEventArgs(result, exception, this.m_Cancelled, asyncOp.UserSuppliedState);
            this.InvokeOperationCompleted(asyncOp, this.uploadStringOperationCompleted, eventArgs);
        }

        private void UploadStringAsyncWriteCallback(byte[] returnBytes, Exception exception, object state)
        {
            UploadBitsState state2 = (UploadBitsState) state;
            if (exception != null)
            {
                UploadStringCompletedEventArgs eventArgs = new UploadStringCompletedEventArgs(null, exception, this.m_Cancelled, state2.AsyncOp.UserSuppliedState);
                this.InvokeOperationCompleted(state2.AsyncOp, this.uploadStringOperationCompleted, eventArgs);
            }
            else
            {
                this.StartDownloadAsync(state2);
            }
        }

        private void UploadStringOperationCompleted(object arg)
        {
            this.OnUploadStringCompleted((UploadStringCompletedEventArgs) arg);
        }

        public byte[] UploadValues(string address, NameValueCollection data)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            return this.UploadValues(this.GetUri(address), null, data);
        }

        public byte[] UploadValues(Uri address, NameValueCollection data)
        {
            return this.UploadValues(address, null, data);
        }

        public byte[] UploadValues(string address, string method, NameValueCollection data)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            return this.UploadValues(this.GetUri(address), method, data);
        }

        public byte[] UploadValues(Uri address, string method, NameValueCollection data)
        {
            byte[] buffer3;
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "UploadValues", address + ", " + method);
            }
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if (method == null)
            {
                method = this.MapToDefaultMethod(address);
            }
            WebRequest request = null;
            this.ClearWebClientState();
            try
            {
                byte[] buffer = this.UploadValuesInternal(data);
                this.m_Method = method;
                request = this.m_WebRequest = this.GetWebRequest(this.GetUri(address));
                this.UploadBits(request, null, buffer, 0, null, null, null, null, null);
                byte[] buffer2 = this.DownloadBits(request, null, null, null);
                if (Logging.On)
                {
                    Logging.Exit(Logging.Web, this, "UploadValues", address + ", " + method);
                }
                buffer3 = buffer2;
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (!(exception is WebException) && !(exception is SecurityException))
                {
                    exception = new WebException(SR.GetString("net_webclient"), exception);
                }
                AbortRequest(request);
                throw exception;
            }
            finally
            {
                this.CompleteWebClientState();
            }
            return buffer3;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void UploadValuesAsync(Uri address, NameValueCollection data)
        {
            this.UploadValuesAsync(address, null, data, null);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void UploadValuesAsync(Uri address, string method, NameValueCollection data)
        {
            this.UploadValuesAsync(address, method, data, null);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void UploadValuesAsync(Uri address, string method, NameValueCollection data, object userToken)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "UploadValuesAsync", address + ", " + method);
            }
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if (method == null)
            {
                method = this.MapToDefaultMethod(address);
            }
            this.InitWebClientAsync();
            this.ClearWebClientState();
            AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(userToken);
            this.m_AsyncOp = asyncOp;
            int chunkSize = 0;
            try
            {
                byte[] buffer = this.UploadValuesInternal(data);
                this.m_Method = method;
                WebRequest request = this.m_WebRequest = this.GetWebRequest(this.GetUri(address));
                if (this.UploadProgressChanged != null)
                {
                    chunkSize = (int) Math.Min(0x2000L, (long) buffer.Length);
                }
                this.UploadBits(request, null, buffer, chunkSize, null, null, new CompletionDelegate(this.UploadValuesAsyncWriteCallback), new CompletionDelegate(this.UploadValuesAsyncReadCallback), asyncOp);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (!(exception is WebException) && !(exception is SecurityException))
                {
                    exception = new WebException(SR.GetString("net_webclient"), exception);
                }
                this.UploadValuesAsyncWriteCallback(null, exception, asyncOp);
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "UploadValuesAsync", (string) null);
            }
        }

        private void UploadValuesAsyncReadCallback(byte[] returnBytes, Exception exception, object state)
        {
            AsyncOperation asyncOp = (AsyncOperation) state;
            UploadValuesCompletedEventArgs eventArgs = new UploadValuesCompletedEventArgs(returnBytes, exception, this.m_Cancelled, asyncOp.UserSuppliedState);
            this.InvokeOperationCompleted(asyncOp, this.uploadValuesOperationCompleted, eventArgs);
        }

        private void UploadValuesAsyncWriteCallback(byte[] returnBytes, Exception exception, object state)
        {
            UploadBitsState state2 = (UploadBitsState) state;
            if (exception != null)
            {
                UploadValuesCompletedEventArgs eventArgs = new UploadValuesCompletedEventArgs(returnBytes, exception, this.m_Cancelled, state2.AsyncOp.UserSuppliedState);
                this.InvokeOperationCompleted(state2.AsyncOp, this.uploadValuesOperationCompleted, eventArgs);
            }
            else
            {
                this.StartDownloadAsync(state2);
            }
        }

        private byte[] UploadValuesInternal(NameValueCollection data)
        {
            if (this.m_headers == null)
            {
                this.m_headers = new WebHeaderCollection(WebHeaderCollectionType.WebRequest);
            }
            string strA = this.m_headers["Content-Type"];
            if ((strA != null) && (string.Compare(strA, "application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) != 0))
            {
                throw new WebException(SR.GetString("net_webclient_ContentType"));
            }
            this.m_headers["Content-Type"] = "application/x-www-form-urlencoded";
            string str2 = string.Empty;
            StringBuilder builder = new StringBuilder();
            foreach (string str3 in data.AllKeys)
            {
                builder.Append(str2);
                builder.Append(UrlEncode(str3));
                builder.Append("=");
                builder.Append(UrlEncode(data[str3]));
                str2 = "&";
            }
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(builder.ToString());
            this.m_ContentLength = bytes.Length;
            return bytes;
        }

        private void UploadValuesOperationCompleted(object arg)
        {
            this.OnUploadValuesCompleted((UploadValuesCompletedEventArgs) arg);
        }

        private static string UrlEncode(string str)
        {
            if (str == null)
            {
                return null;
            }
            return UrlEncode(str, System.Text.Encoding.UTF8);
        }

        private static string UrlEncode(string str, System.Text.Encoding e)
        {
            if (str == null)
            {
                return null;
            }
            return System.Text.Encoding.ASCII.GetString(UrlEncodeToBytes(str, e));
        }

        private static byte[] UrlEncodeBytesToBytesInternal(byte[] bytes, int offset, int count, bool alwaysCreateReturnValue)
        {
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < count; i++)
            {
                char ch = (char) bytes[offset + i];
                if (ch == ' ')
                {
                    num++;
                }
                else if (!IsSafe(ch))
                {
                    num2++;
                }
            }
            if ((!alwaysCreateReturnValue && (num == 0)) && (num2 == 0))
            {
                return bytes;
            }
            byte[] buffer = new byte[count + (num2 * 2)];
            int num4 = 0;
            for (int j = 0; j < count; j++)
            {
                byte num6 = bytes[offset + j];
                char ch2 = (char) num6;
                if (IsSafe(ch2))
                {
                    buffer[num4++] = num6;
                }
                else if (ch2 == ' ')
                {
                    buffer[num4++] = 0x2b;
                }
                else
                {
                    buffer[num4++] = 0x25;
                    buffer[num4++] = (byte) IntToHex((num6 >> 4) & 15);
                    buffer[num4++] = (byte) IntToHex(num6 & 15);
                }
            }
            return buffer;
        }

        private static byte[] UrlEncodeToBytes(string str, System.Text.Encoding e)
        {
            if (str == null)
            {
                return null;
            }
            byte[] bytes = e.GetBytes(str);
            return UrlEncodeBytesToBytesInternal(bytes, 0, bytes.Length, false);
        }

        public string BaseAddress
        {
            get
            {
                if (this.m_baseAddress != null)
                {
                    return this.m_baseAddress.ToString();
                }
                return string.Empty;
            }
            set
            {
                if ((value == null) || (value.Length == 0))
                {
                    this.m_baseAddress = null;
                }
                else
                {
                    try
                    {
                        this.m_baseAddress = new Uri(value);
                    }
                    catch (UriFormatException exception)
                    {
                        throw new ArgumentException(SR.GetString("net_webclient_invalid_baseaddress"), "value", exception);
                    }
                }
            }
        }

        public RequestCachePolicy CachePolicy
        {
            get
            {
                return this.m_CachePolicy;
            }
            set
            {
                this.m_CachePolicy = value;
            }
        }

        public ICredentials Credentials
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

        public System.Text.Encoding Encoding
        {
            get
            {
                return this.m_Encoding;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Encoding");
                }
                this.m_Encoding = value;
            }
        }

        public WebHeaderCollection Headers
        {
            get
            {
                if (this.m_headers == null)
                {
                    this.m_headers = new WebHeaderCollection(WebHeaderCollectionType.WebRequest);
                }
                return this.m_headers;
            }
            set
            {
                this.m_headers = value;
            }
        }

        public bool IsBusy
        {
            get
            {
                return (this.m_AsyncOp != null);
            }
        }

        public IWebProxy Proxy
        {
            get
            {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                if (!this.m_ProxySet)
                {
                    return WebRequest.InternalDefaultWebProxy;
                }
                return this.m_Proxy;
            }
            set
            {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                this.m_Proxy = value;
                this.m_ProxySet = true;
            }
        }

        public NameValueCollection QueryString
        {
            get
            {
                if (this.m_requestParameters == null)
                {
                    this.m_requestParameters = new NameValueCollection();
                }
                return this.m_requestParameters;
            }
            set
            {
                this.m_requestParameters = value;
            }
        }

        public WebHeaderCollection ResponseHeaders
        {
            get
            {
                if (this.m_WebResponse != null)
                {
                    return this.m_WebResponse.Headers;
                }
                return null;
            }
        }

        public bool UseDefaultCredentials
        {
            get
            {
                return (this.m_credentials is SystemNetworkCredential);
            }
            set
            {
                this.m_credentials = value ? CredentialCache.DefaultCredentials : null;
            }
        }

        private class DownloadBitsState
        {
            internal AsyncOperation AsyncOp;
            internal System.Net.CompletionDelegate CompletionDelegate;
            internal long ContentLength;
            internal byte[] InnerBuffer;
            internal long Length;
            internal int Offset;
            internal System.Net.WebClient.ProgressData Progress;
            internal Stream ReadStream;
            internal WebRequest Request;
            internal ScatterGatherBuffers SgBuffers;
            internal System.Net.WebClient WebClient;
            internal Stream WriteStream;

            internal DownloadBitsState(WebRequest request, Stream writeStream, System.Net.CompletionDelegate completionDelegate, AsyncOperation asyncOp, System.Net.WebClient.ProgressData progress, System.Net.WebClient webClient)
            {
                this.WriteStream = writeStream;
                this.Request = request;
                this.AsyncOp = asyncOp;
                this.CompletionDelegate = completionDelegate;
                this.WebClient = webClient;
                this.Progress = progress;
            }

            internal void Close()
            {
                if (this.WriteStream != null)
                {
                    this.WriteStream.Close();
                }
                if (this.ReadStream != null)
                {
                    this.ReadStream.Close();
                }
            }

            internal bool RetrieveBytes(ref int bytesRetrieved)
            {
                if (bytesRetrieved > 0)
                {
                    if (this.WriteStream != null)
                    {
                        this.WriteStream.Write(this.InnerBuffer, 0, bytesRetrieved);
                    }
                    else
                    {
                        this.SgBuffers.Write(this.InnerBuffer, 0, bytesRetrieved);
                    }
                    if (this.Async)
                    {
                        this.Progress.BytesReceived += (long) bytesRetrieved;
                    }
                    if (this.Offset != this.ContentLength)
                    {
                        if (this.Async)
                        {
                            this.WebClient.PostProgressChanged(this.AsyncOp, this.Progress);
                            this.ReadStream.BeginRead(this.InnerBuffer, this.Offset, ((int) this.Length) - this.Offset, new AsyncCallback(System.Net.WebClient.DownloadBitsReadCallback), this);
                        }
                        else
                        {
                            bytesRetrieved = this.ReadStream.Read(this.InnerBuffer, this.Offset, ((int) this.Length) - this.Offset);
                        }
                        return false;
                    }
                }
                if (this.Async)
                {
                    if (this.Progress.TotalBytesToReceive < 0L)
                    {
                        this.Progress.TotalBytesToReceive = this.Progress.BytesReceived;
                    }
                    this.WebClient.PostProgressChanged(this.AsyncOp, this.Progress);
                }
                if (this.ReadStream != null)
                {
                    this.ReadStream.Close();
                }
                if (this.WriteStream != null)
                {
                    this.WriteStream.Close();
                }
                else if (this.WriteStream == null)
                {
                    byte[] dst = new byte[this.SgBuffers.Length];
                    if (this.SgBuffers.Length > 0)
                    {
                        BufferOffsetSize[] buffers = this.SgBuffers.GetBuffers();
                        int dstOffset = 0;
                        for (int i = 0; i < buffers.Length; i++)
                        {
                            BufferOffsetSize size = buffers[i];
                            Buffer.BlockCopy(size.Buffer, 0, dst, dstOffset, size.Size);
                            dstOffset += size.Size;
                        }
                    }
                    this.InnerBuffer = dst;
                }
                return true;
            }

            internal int SetResponse(WebResponse response)
            {
                this.ContentLength = response.ContentLength;
                if ((this.ContentLength == -1L) || (this.ContentLength > 0x10000L))
                {
                    this.Length = 0x10000L;
                }
                else
                {
                    this.Length = this.ContentLength;
                }
                if (this.WriteStream == null)
                {
                    if (this.ContentLength > 0x7fffffffL)
                    {
                        throw new WebException(SR.GetString("net_webstatus_MessageLengthLimitExceeded"), WebExceptionStatus.MessageLengthLimitExceeded);
                    }
                    this.SgBuffers = new ScatterGatherBuffers(this.Length);
                }
                this.InnerBuffer = new byte[(int) this.Length];
                this.ReadStream = response.GetResponseStream();
                if (this.Async && (response.ContentLength >= 0L))
                {
                    this.Progress.TotalBytesToReceive = response.ContentLength;
                }
                if (this.Async)
                {
                    if ((this.ReadStream == null) || (this.ReadStream == Stream.Null))
                    {
                        System.Net.WebClient.DownloadBitsReadCallbackState(this, null);
                    }
                    else
                    {
                        this.ReadStream.BeginRead(this.InnerBuffer, this.Offset, ((int) this.Length) - this.Offset, new AsyncCallback(System.Net.WebClient.DownloadBitsReadCallback), this);
                    }
                    return -1;
                }
                if ((this.ReadStream != null) && (this.ReadStream != Stream.Null))
                {
                    return this.ReadStream.Read(this.InnerBuffer, this.Offset, ((int) this.Length) - this.Offset);
                }
                return 0;
            }

            internal bool Async
            {
                get
                {
                    return (this.AsyncOp != null);
                }
            }
        }

        private class ProgressData
        {
            internal long BytesReceived;
            internal long BytesSent;
            internal bool HasUploadPhase;
            internal long TotalBytesToReceive = -1L;
            internal long TotalBytesToSend = -1L;

            internal void Reset()
            {
                this.BytesSent = 0L;
                this.TotalBytesToSend = -1L;
                this.BytesReceived = 0L;
                this.TotalBytesToReceive = -1L;
                this.HasUploadPhase = false;
            }
        }

        private class UploadBitsState
        {
            internal AsyncOperation AsyncOp;
            internal CompletionDelegate DownloadCompletionDelegate;
            internal byte[] Footer;
            internal byte[] Header;
            internal byte[] InnerBuffer;
            internal long Length;
            private int m_BufferWritePosition;
            private int m_ChunkSize;
            internal int Offset;
            internal System.Net.WebClient.ProgressData Progress;
            internal Stream ReadStream;
            internal WebRequest Request;
            internal CompletionDelegate UploadCompletionDelegate;
            internal System.Net.WebClient WebClient;
            internal Stream WriteStream;

            internal UploadBitsState(WebRequest request, Stream readStream, byte[] buffer, int chunkSize, byte[] header, byte[] footer, CompletionDelegate uploadCompletionDelegate, CompletionDelegate downloadCompletionDelegate, AsyncOperation asyncOp, System.Net.WebClient.ProgressData progress, System.Net.WebClient webClient)
            {
                this.InnerBuffer = buffer;
                this.m_ChunkSize = chunkSize;
                this.m_BufferWritePosition = 0;
                this.Header = header;
                this.Footer = footer;
                this.ReadStream = readStream;
                this.Request = request;
                this.AsyncOp = asyncOp;
                this.UploadCompletionDelegate = uploadCompletionDelegate;
                this.DownloadCompletionDelegate = downloadCompletionDelegate;
                if (this.AsyncOp != null)
                {
                    this.Progress = progress;
                    this.Progress.HasUploadPhase = true;
                    this.Progress.TotalBytesToSend = (request.ContentLength < 0L) ? -1L : request.ContentLength;
                }
                this.WebClient = webClient;
            }

            internal void Close()
            {
                if (this.WriteStream != null)
                {
                    this.WriteStream.Close();
                }
                if (this.ReadStream != null)
                {
                    this.ReadStream.Close();
                }
            }

            internal void SetRequestStream(Stream writeStream)
            {
                this.WriteStream = writeStream;
                byte[] header = null;
                if (this.Header != null)
                {
                    header = this.Header;
                    this.Header = null;
                }
                else
                {
                    header = new byte[0];
                }
                if (this.Async)
                {
                    this.Progress.BytesSent += header.Length;
                    this.WriteStream.BeginWrite(header, 0, header.Length, new AsyncCallback(System.Net.WebClient.UploadBitsWriteCallback), this);
                }
                else
                {
                    this.WriteStream.Write(header, 0, header.Length);
                }
            }

            internal bool WriteBytes()
            {
                byte[] footer = null;
                int count = 0;
                int offset = 0;
                if (this.Async)
                {
                    this.WebClient.PostProgressChanged(this.AsyncOp, this.Progress);
                }
                if (this.FileUpload)
                {
                    int num3 = 0;
                    if (this.InnerBuffer != null)
                    {
                        num3 = this.ReadStream.Read(this.InnerBuffer, 0, this.InnerBuffer.Length);
                        if (num3 <= 0)
                        {
                            this.ReadStream.Close();
                            this.InnerBuffer = null;
                        }
                    }
                    if (this.InnerBuffer == null)
                    {
                        if (this.Footer == null)
                        {
                            return true;
                        }
                        count = this.Footer.Length;
                        footer = this.Footer;
                        this.Footer = null;
                    }
                    else
                    {
                        count = num3;
                        footer = this.InnerBuffer;
                    }
                }
                else
                {
                    if (this.InnerBuffer == null)
                    {
                        return true;
                    }
                    footer = this.InnerBuffer;
                    if (this.m_ChunkSize != 0)
                    {
                        offset = this.m_BufferWritePosition;
                        this.m_BufferWritePosition += this.m_ChunkSize;
                        count = this.m_ChunkSize;
                        if (this.m_BufferWritePosition >= this.InnerBuffer.Length)
                        {
                            count = this.InnerBuffer.Length - offset;
                            this.InnerBuffer = null;
                        }
                    }
                    else
                    {
                        count = this.InnerBuffer.Length;
                        this.InnerBuffer = null;
                    }
                }
                if (this.Async)
                {
                    this.Progress.BytesSent += count;
                    this.WriteStream.BeginWrite(footer, offset, count, new AsyncCallback(System.Net.WebClient.UploadBitsWriteCallback), this);
                }
                else
                {
                    this.WriteStream.Write(footer, 0, count);
                }
                return false;
            }

            internal bool Async
            {
                get
                {
                    return (this.AsyncOp != null);
                }
            }

            internal bool FileUpload
            {
                get
                {
                    return (this.ReadStream != null);
                }
            }
        }

        private class WebClientWriteStream : Stream
        {
            private WebRequest m_request;
            private Stream m_stream;
            private WebClient m_WebClient;

            public WebClientWriteStream(Stream stream, WebRequest request, WebClient webClient)
            {
                this.m_request = request;
                this.m_stream = stream;
                this.m_WebClient = webClient;
            }

            [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
            public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
            {
                return this.m_stream.BeginRead(buffer, offset, size, callback, state);
            }

            [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
            {
                return this.m_stream.BeginWrite(buffer, offset, size, callback, state);
            }

            protected override void Dispose(bool disposing)
            {
                try
                {
                    if (disposing)
                    {
                        this.m_stream.Close();
                        this.m_WebClient.GetWebResponse(this.m_request).Close();
                    }
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }

            public override int EndRead(IAsyncResult result)
            {
                return this.m_stream.EndRead(result);
            }

            public override void EndWrite(IAsyncResult result)
            {
                this.m_stream.EndWrite(result);
            }

            public override void Flush()
            {
                this.m_stream.Flush();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return this.m_stream.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return this.m_stream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                this.m_stream.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                this.m_stream.Write(buffer, offset, count);
            }

            public override bool CanRead
            {
                get
                {
                    return this.m_stream.CanRead;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return this.m_stream.CanSeek;
                }
            }

            public override bool CanTimeout
            {
                get
                {
                    return this.m_stream.CanTimeout;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return this.m_stream.CanWrite;
                }
            }

            public override long Length
            {
                get
                {
                    return this.m_stream.Length;
                }
            }

            public override long Position
            {
                get
                {
                    return this.m_stream.Position;
                }
                set
                {
                    this.m_stream.Position = value;
                }
            }

            public override int ReadTimeout
            {
                get
                {
                    return this.m_stream.ReadTimeout;
                }
                set
                {
                    this.m_stream.ReadTimeout = value;
                }
            }

            public override int WriteTimeout
            {
                get
                {
                    return this.m_stream.WriteTimeout;
                }
                set
                {
                    this.m_stream.WriteTimeout = value;
                }
            }
        }
    }
}

