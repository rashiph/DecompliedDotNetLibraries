namespace System.Runtime.Remoting.Channels.Http
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Cache;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Messaging;
    using System.Security.Cryptography.X509Certificates;

    internal class HttpClientTransportSink : BaseChannelSinkWithProperties, IClientChannelSink, IChannelSinkBase
    {
        private bool _bAllowAutoRedirect;
        private bool _bSecurityPreAuthenticate;
        private bool _bUnsafeAuthenticatedConnectionSharing;
        private X509CertificateCollection _certificates;
        private HttpClientChannel _channel;
        private string _channelURI;
        private string _connectionGroupName;
        private ICredentials _credentials;
        private string _proxyName;
        private IWebProxy _proxyObject;
        private int _proxyPort = -1;
        private string _securityDomain;
        private string _securityPassword;
        private string _securityUserName;
        private int _timeout = -1;
        private bool _useChunked;
        private bool _useKeepAlive = true;
        private const string AllowAutoRedirectKey = "allowautoredirect";
        private const string ClientCertificatesKey = "clientcertificates";
        private const string ConnectionGroupNameKey = "connectiongroupname";
        private const string CredentialsKey = "credentials";
        private const string DomainKey = "domain";
        private const string PasswordKey = "password";
        private const string PreAuthenticateKey = "preauthenticate";
        private const string ProxyNameKey = "proxyname";
        private const string ProxyPortKey = "proxyport";
        private const string s_defaultVerb = "POST";
        private static ICollection s_keySet = null;
        private static RequestCachePolicy s_requestCachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
        private static string s_userAgent = string.Concat(new object[] { "Mozilla/4.0+(compatible; MSIE 6.0; Windows ", Environment.OSVersion.Version, "; MS .NET Remoting; MS .NET CLR ", Environment.Version.ToString(), " )" });
        private const string TimeoutKey = "timeout";
        private const string UnsafeAuthenticatedConnectionSharingKey = "unsafeauthenticatedconnectionsharing";
        private const string UserNameKey = "username";

        internal HttpClientTransportSink(HttpClientChannel channel, string channelURI)
        {
            this._channel = channel;
            this._channelURI = channelURI;
            if (this._channelURI.EndsWith("/", StringComparison.Ordinal))
            {
                this._channelURI = this._channelURI.Substring(0, this._channelURI.Length - 1);
            }
        }

        public void AsyncProcessRequest(IClientChannelSinkStack sinkStack, IMessage msg, ITransportHeaders headers, Stream stream)
        {
            new AsyncHttpClientRequestState(this, sinkStack, msg, headers, stream, 1).StartRequest();
        }

        public void AsyncProcessResponse(IClientResponseChannelSinkStack sinkStack, object state, ITransportHeaders headers, Stream stream)
        {
        }

        private static ITransportHeaders CollectResponseHeaders(HttpWebResponse response)
        {
            TransportHeaders headers = new TransportHeaders();
            foreach (object obj2 in response.Headers)
            {
                string str = obj2.ToString();
                headers[str] = response.Headers[str];
            }
            return headers;
        }

        public Stream GetRequestStream(IMessage msg, ITransportHeaders headers)
        {
            return null;
        }

        private HttpWebRequest ProcessAndSend(IMessage msg, ITransportHeaders headers, Stream inputStream)
        {
            long position = 0L;
            bool canSeek = false;
            if (inputStream != null)
            {
                canSeek = inputStream.CanSeek;
                if (canSeek)
                {
                    position = inputStream.Position;
                }
            }
            HttpWebRequest request = null;
            Stream target = null;
            try
            {
                request = this.SetupWebRequest(msg, headers);
                if (inputStream != null)
                {
                    if (!this._useChunked)
                    {
                        request.ContentLength = (int) inputStream.Length;
                    }
                    target = request.GetRequestStream();
                    StreamHelper.CopyStream(inputStream, target);
                }
            }
            catch
            {
                if (canSeek)
                {
                    request = this.SetupWebRequest(msg, headers);
                    if (inputStream != null)
                    {
                        inputStream.Position = position;
                        if (!this._useChunked)
                        {
                            request.ContentLength = (int) inputStream.Length;
                        }
                        target = request.GetRequestStream();
                        StreamHelper.CopyStream(inputStream, target);
                    }
                }
            }
            if (inputStream != null)
            {
                inputStream.Close();
            }
            if (target != null)
            {
                target.Close();
            }
            return request;
        }

        public void ProcessMessage(IMessage msg, ITransportHeaders requestHeaders, Stream requestStream, out ITransportHeaders responseHeaders, out Stream responseStream)
        {
            HttpWebRequest request = this.ProcessAndSend(msg, requestHeaders, requestStream);
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse) request.GetResponse();
            }
            catch (WebException exception)
            {
                ProcessResponseException(exception, out response);
            }
            this.ReceiveAndProcess(response, out responseHeaders, out responseStream);
        }

        private static void ProcessResponseException(WebException webException, out HttpWebResponse response)
        {
            if (webException.Status == WebExceptionStatus.Timeout)
            {
                throw new RemotingTimeoutException(CoreChannel.GetResourceString("Remoting_Channels_RequestTimedOut"), webException);
            }
            response = webException.Response as HttpWebResponse;
            if (response == null)
            {
                throw webException;
            }
            int statusCode = (int) response.StatusCode;
            if ((statusCode < 500) || (statusCode > 0x257))
            {
                throw webException;
            }
        }

        private void ReceiveAndProcess(HttpWebResponse response, out ITransportHeaders returnHeaders, out Stream returnStream)
        {
            int num;
            if (response == null)
            {
                num = 0x1000;
            }
            else
            {
                int contentLength = (int) response.ContentLength;
                switch (contentLength)
                {
                    case -1:
                    case 0:
                        num = 0x1000;
                        goto Label_0034;
                }
                if (contentLength <= 0x3e80)
                {
                    num = contentLength;
                }
                else
                {
                    num = 0x3e80;
                }
            }
        Label_0034:
            returnStream = new BufferedStream(response.GetResponseStream(), num);
            returnHeaders = CollectResponseHeaders(response);
        }

        private HttpWebRequest SetupWebRequest(IMessage msg, ITransportHeaders headers)
        {
            string str2;
            IMethodCallMessage message = msg as IMethodCallMessage;
            string url = (string) headers["__RequestUri"];
            if (url == null)
            {
                if (message != null)
                {
                    url = message.Uri;
                }
                else
                {
                    url = (string) msg.Properties["__Uri"];
                }
            }
            if (HttpChannelHelper.StartsWithHttp(url) != -1)
            {
                str2 = url;
            }
            else
            {
                if (!url.StartsWith("/", StringComparison.Ordinal))
                {
                    url = "/" + url;
                }
                str2 = this._channelURI + url;
            }
            string str3 = (string) headers["__RequestVerb"];
            if (str3 == null)
            {
                str3 = "POST";
            }
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(str2);
            request.AllowAutoRedirect = this._bAllowAutoRedirect;
            request.Method = str3;
            request.SendChunked = this._useChunked;
            request.KeepAlive = this._useKeepAlive;
            request.Pipelined = false;
            request.UserAgent = s_userAgent;
            request.Timeout = this._timeout;
            request.CachePolicy = s_requestCachePolicy;
            IWebProxy proxyObject = this._proxyObject;
            if (proxyObject == null)
            {
                proxyObject = this._channel.ProxyObject;
            }
            if (proxyObject != null)
            {
                request.Proxy = proxyObject;
            }
            if (this._credentials != null)
            {
                request.Credentials = this._credentials;
                request.PreAuthenticate = this._bSecurityPreAuthenticate;
                request.UnsafeAuthenticatedConnectionSharing = this._bUnsafeAuthenticatedConnectionSharing;
                if (this._connectionGroupName != null)
                {
                    request.ConnectionGroupName = this._connectionGroupName;
                }
            }
            else if (this._securityUserName != null)
            {
                if (this._securityDomain == null)
                {
                    request.Credentials = new NetworkCredential(this._securityUserName, this._securityPassword);
                }
                else
                {
                    request.Credentials = new NetworkCredential(this._securityUserName, this._securityPassword, this._securityDomain);
                }
                request.PreAuthenticate = this._bSecurityPreAuthenticate;
                request.UnsafeAuthenticatedConnectionSharing = this._bUnsafeAuthenticatedConnectionSharing;
                if (this._connectionGroupName != null)
                {
                    request.ConnectionGroupName = this._connectionGroupName;
                }
            }
            else if (this._channel.UseDefaultCredentials)
            {
                if (this._channel.UseAuthenticatedConnectionSharing)
                {
                    request.ConnectionGroupName = CoreChannel.GetCurrentSidString();
                    request.UnsafeAuthenticatedConnectionSharing = true;
                }
                request.Credentials = CredentialCache.DefaultCredentials;
                request.PreAuthenticate = this._bSecurityPreAuthenticate;
            }
            if (this._certificates != null)
            {
                foreach (X509Certificate certificate in this._certificates)
                {
                    request.ClientCertificates.Add(certificate);
                }
                request.PreAuthenticate = this._bSecurityPreAuthenticate;
            }
            foreach (DictionaryEntry entry in headers)
            {
                string key = entry.Key as string;
                if ((key != null) && !key.StartsWith("__", StringComparison.Ordinal))
                {
                    if (key.Equals("Content-Type"))
                    {
                        request.ContentType = entry.Value.ToString();
                    }
                    else
                    {
                        request.Headers[key] = entry.Value.ToString();
                    }
                }
            }
            return request;
        }

        private void UpdateProxy()
        {
            if ((this._proxyName != null) && (this._proxyPort > 0))
            {
                WebProxy proxy = new WebProxy(this._proxyName, this._proxyPort) {
                    BypassProxyOnLocal = true
                };
                this._proxyObject = proxy;
            }
        }

        public override object this[object key]
        {
            get
            {
                string str = key as string;
                if (str != null)
                {
                    switch (str.ToLower(CultureInfo.InvariantCulture))
                    {
                        case "username":
                            return this._securityUserName;

                        case "password":
                            return null;

                        case "domain":
                            return this._securityDomain;

                        case "preauthenticate":
                            return this._bSecurityPreAuthenticate;

                        case "credentials":
                            return this._credentials;

                        case "clientcertificates":
                            return null;

                        case "proxyname":
                            return this._proxyName;

                        case "proxyport":
                            return this._proxyPort;

                        case "timeout":
                            return this._timeout;

                        case "allowautoredirect":
                            return this._bAllowAutoRedirect;

                        case "unsafeauthenticatedconnectionsharing":
                            return this._bUnsafeAuthenticatedConnectionSharing;

                        case "connectiongroupname":
                            return this._connectionGroupName;
                    }
                }
                return null;
            }
            set
            {
                string str = key as string;
                if (str != null)
                {
                    switch (str.ToLower(CultureInfo.InvariantCulture))
                    {
                        case "username":
                            this._securityUserName = (string) value;
                            return;

                        case "password":
                            this._securityPassword = (string) value;
                            return;

                        case "domain":
                            this._securityDomain = (string) value;
                            return;

                        case "preauthenticate":
                            this._bSecurityPreAuthenticate = Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                            return;

                        case "credentials":
                            this._credentials = (ICredentials) value;
                            return;

                        case "clientcertificates":
                            this._certificates = (X509CertificateCollection) value;
                            return;

                        case "proxyname":
                            this._proxyName = (string) value;
                            this.UpdateProxy();
                            return;

                        case "proxyport":
                            this._proxyPort = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                            this.UpdateProxy();
                            return;

                        case "timeout":
                        {
                            if (!(value is TimeSpan))
                            {
                                this._timeout = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                                return;
                            }
                            TimeSpan span = (TimeSpan) value;
                            this._timeout = (int) span.TotalMilliseconds;
                            return;
                        }
                        case "allowautoredirect":
                            this._bAllowAutoRedirect = Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                            return;

                        case "unsafeauthenticatedconnectionsharing":
                            this._bUnsafeAuthenticatedConnectionSharing = Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                            return;

                        case "connectiongroupname":
                            this._connectionGroupName = (string) value;
                            return;
                    }
                }
            }
        }

        public override ICollection Keys
        {
            get
            {
                if (s_keySet == null)
                {
                    ArrayList list = new ArrayList(6);
                    list.Add("username");
                    list.Add("password");
                    list.Add("domain");
                    list.Add("preauthenticate");
                    list.Add("credentials");
                    list.Add("clientcertificates");
                    list.Add("proxyname");
                    list.Add("proxyport");
                    list.Add("timeout");
                    list.Add("allowautoredirect");
                    list.Add("unsafeauthenticatedconnectionsharing");
                    list.Add("connectiongroupname");
                    s_keySet = list;
                }
                return s_keySet;
            }
        }

        public IClientChannelSink NextChannelSink
        {
            get
            {
                return null;
            }
        }

        internal static string UserAgent
        {
            get
            {
                return s_userAgent;
            }
        }

        private class AsyncHttpClientRequestState
        {
            private long _initialStreamPosition;
            private IMessage _msg;
            private ITransportHeaders _requestHeaders;
            private int _retryCount;
            private HttpClientTransportSink _transportSink;
            internal Stream ActualResponseStream;
            internal Stream RequestStream;
            private static AsyncCallback s_processAsyncCopyRequestStreamCompletion = new AsyncCallback(HttpClientTransportSink.AsyncHttpClientRequestState.ProcessAsyncCopyResponseStreamCompletion);
            private static AsyncCallback s_processAsyncCopyRequestStreamCompletionCallback = new AsyncCallback(HttpClientTransportSink.AsyncHttpClientRequestState.ProcessAsyncCopyRequestStreamCompletion);
            private static AsyncCallback s_processGetRequestStreamCompletionCallback = new AsyncCallback(HttpClientTransportSink.AsyncHttpClientRequestState.ProcessGetRequestStreamCompletion);
            private static AsyncCallback s_processGetResponseCompletionCallback = new AsyncCallback(HttpClientTransportSink.AsyncHttpClientRequestState.ProcessGetResponseCompletion);
            internal IClientChannelSinkStack SinkStack;
            internal HttpWebRequest WebRequest;
            internal HttpWebResponse WebResponse;

            internal AsyncHttpClientRequestState(HttpClientTransportSink transportSink, IClientChannelSinkStack sinkStack, IMessage msg, ITransportHeaders headers, Stream stream, int retryCount)
            {
                this._transportSink = transportSink;
                this.SinkStack = sinkStack;
                this._msg = msg;
                this._requestHeaders = headers;
                this.RequestStream = stream;
                this._retryCount = retryCount;
                if (this.RequestStream.CanSeek)
                {
                    this._initialStreamPosition = this.RequestStream.Position;
                }
            }

            private static void ProcessAsyncCopyRequestStreamCompletion(IAsyncResult iar)
            {
                HttpClientTransportSink.AsyncHttpClientRequestState asyncState = (HttpClientTransportSink.AsyncHttpClientRequestState) iar.AsyncState;
                try
                {
                    StreamHelper.EndAsyncCopyStream(iar);
                    asyncState.WebRequest.BeginGetResponse(s_processGetResponseCompletionCallback, asyncState);
                }
                catch (Exception exception)
                {
                    asyncState.RetryOrDispatchException(exception);
                }
            }

            private static void ProcessAsyncCopyResponseStreamCompletion(IAsyncResult iar)
            {
                HttpClientTransportSink.AsyncHttpClientRequestState asyncState = (HttpClientTransportSink.AsyncHttpClientRequestState) iar.AsyncState;
                try
                {
                    StreamHelper.EndAsyncCopyStream(iar);
                    HttpWebResponse webResponse = asyncState.WebResponse;
                    Stream actualResponseStream = asyncState.ActualResponseStream;
                    ITransportHeaders headers = HttpClientTransportSink.CollectResponseHeaders(webResponse);
                    asyncState.SinkStack.AsyncProcessResponse(headers, actualResponseStream);
                }
                catch (Exception exception)
                {
                    asyncState.SinkStack.DispatchException(exception);
                }
            }

            private static void ProcessGetRequestStreamCompletion(IAsyncResult iar)
            {
                HttpClientTransportSink.AsyncHttpClientRequestState asyncState = (HttpClientTransportSink.AsyncHttpClientRequestState) iar.AsyncState;
                try
                {
                    HttpWebRequest webRequest = asyncState.WebRequest;
                    Stream requestStream = asyncState.RequestStream;
                    Stream target = webRequest.EndGetRequestStream(iar);
                    StreamHelper.BeginAsyncCopyStream(requestStream, target, false, true, false, true, s_processAsyncCopyRequestStreamCompletionCallback, asyncState);
                }
                catch (Exception exception)
                {
                    asyncState.RetryOrDispatchException(exception);
                }
            }

            private static void ProcessGetResponseCompletion(IAsyncResult iar)
            {
                HttpClientTransportSink.AsyncHttpClientRequestState asyncState = (HttpClientTransportSink.AsyncHttpClientRequestState) iar.AsyncState;
                try
                {
                    asyncState.RequestStream.Close();
                    HttpWebResponse response = null;
                    HttpWebRequest webRequest = asyncState.WebRequest;
                    try
                    {
                        response = (HttpWebResponse) webRequest.EndGetResponse(iar);
                    }
                    catch (WebException exception)
                    {
                        HttpClientTransportSink.ProcessResponseException(exception, out response);
                    }
                    asyncState.WebResponse = response;
                    ChunkedMemoryStream target = new ChunkedMemoryStream(CoreChannel.BufferPool);
                    asyncState.ActualResponseStream = target;
                    StreamHelper.BeginAsyncCopyStream(response.GetResponseStream(), target, true, false, true, false, s_processAsyncCopyRequestStreamCompletion, asyncState);
                }
                catch (Exception exception2)
                {
                    asyncState.SinkStack.DispatchException(exception2);
                }
            }

            internal void RetryOrDispatchException(Exception e)
            {
                bool flag = false;
                try
                {
                    if (this._retryCount > 0)
                    {
                        this._retryCount--;
                        if (this.RequestStream.CanSeek)
                        {
                            this.RequestStream.Position = this._initialStreamPosition;
                            this.StartRequest();
                            flag = true;
                        }
                    }
                }
                catch
                {
                }
                if (!flag)
                {
                    this.RequestStream.Close();
                    this.SinkStack.DispatchException(e);
                }
            }

            internal void StartRequest()
            {
                this.WebRequest = this._transportSink.SetupWebRequest(this._msg, this._requestHeaders);
                if (!this._transportSink._useChunked)
                {
                    try
                    {
                        this.WebRequest.ContentLength = (int) this.RequestStream.Length;
                    }
                    catch
                    {
                    }
                }
                this.WebRequest.BeginGetRequestStream(s_processGetRequestStreamCompletionCallback, this);
            }
        }
    }
}

