namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Text;

    internal static class HttpChannelUtilities
    {
        internal const string HttpStatusCodeExceptionKey = "System.ServiceModel.Channels.HttpInput.HttpStatusCode";
        internal const string HttpStatusCodeKey = "HttpStatusCode";
        internal const string HttpStatusDescriptionExceptionKey = "System.ServiceModel.Channels.HttpInput.HttpStatusDescription";
        internal const string MIMEVersionHeader = "MIME-Version";
        internal const int ResponseStreamExcerptSize = 0x400;

        public static void AbortRequest(HttpWebRequest request)
        {
            request.Abort();
        }

        public static void AddReplySecurityProperty(HttpChannelFactory factory, HttpWebRequest webRequest, HttpWebResponse webResponse, Message replyMessage)
        {
            SecurityMessageProperty property = factory.CreateReplySecurityProperty(webRequest, webResponse);
            if (property != null)
            {
                replyMessage.Properties.Security = property;
            }
        }

        public static Exception ConvertWebException(WebException webException, HttpWebRequest request, HttpAbortReason abortReason)
        {
            switch (webException.Status)
            {
                case WebExceptionStatus.NameResolutionFailure:
                case WebExceptionStatus.ConnectFailure:
                case WebExceptionStatus.ProxyNameResolutionFailure:
                    return new EndpointNotFoundException(System.ServiceModel.SR.GetString("EndpointNotFound", new object[] { request.RequestUri.AbsoluteUri }), webException);

                case WebExceptionStatus.ReceiveFailure:
                    return new CommunicationException(System.ServiceModel.SR.GetString("HttpReceiveFailure", new object[] { request.RequestUri }), webException);

                case WebExceptionStatus.SendFailure:
                    return new CommunicationException(System.ServiceModel.SR.GetString("HttpSendFailure", new object[] { request.RequestUri }), webException);

                case WebExceptionStatus.RequestCanceled:
                    return CreateRequestCanceledException(webException, request, abortReason);

                case WebExceptionStatus.TrustFailure:
                    return new SecurityNegotiationException(System.ServiceModel.SR.GetString("TrustFailure", new object[] { request.RequestUri.Authority }), webException);

                case WebExceptionStatus.SecureChannelFailure:
                    return new SecurityNegotiationException(System.ServiceModel.SR.GetString("SecureChannelFailure", new object[] { request.RequestUri.Authority }), webException);

                case WebExceptionStatus.Timeout:
                    return new TimeoutException(CreateRequestTimedOutMessage(request), webException);
            }
            return null;
        }

        public static Exception CreateCommunicationException(HttpListenerException listenerException)
        {
            switch (listenerException.NativeErrorCode)
            {
                case 0x40:
                    return new CommunicationException(System.ServiceModel.SR.GetString("HttpNetnameDeleted", new object[] { listenerException.Message }), listenerException);

                case 0x494:
                    return new CommunicationException(System.ServiceModel.SR.GetString("HttpNoTrackingService", new object[] { listenerException.Message }), listenerException);

                case 0x5aa:
                case 8:
                case 14:
                    return new InsufficientMemoryException(System.ServiceModel.SR.GetString("InsufficentMemory"), listenerException);

                case 6:
                    return new CommunicationObjectAbortedException(System.ServiceModel.SR.GetString("HttpResponseAborted"), listenerException);
            }
            return new CommunicationException(listenerException.Message, listenerException);
        }

        public static Exception CreateNullReferenceResponseException(NullReferenceException nullReferenceException)
        {
            return TraceResponseException(new ProtocolException(System.ServiceModel.SR.GetString("NullReferenceOnHttpResponse"), nullReferenceException));
        }

        public static Exception CreateRequestCanceledException(Exception webException, HttpWebRequest request, HttpAbortReason abortReason)
        {
            switch (abortReason)
            {
                case HttpAbortReason.Aborted:
                    return new CommunicationObjectAbortedException(System.ServiceModel.SR.GetString("HttpRequestAborted", new object[] { request.RequestUri }), webException);

                case HttpAbortReason.TimedOut:
                    return new TimeoutException(CreateRequestTimedOutMessage(request), webException);
            }
            return new CommunicationException(System.ServiceModel.SR.GetString("HttpTransferError", new object[] { webException.Message }), webException);
        }

        public static Exception CreateRequestIOException(IOException ioException, HttpWebRequest request)
        {
            return CreateRequestIOException(ioException, request, null);
        }

        public static Exception CreateRequestIOException(IOException ioException, HttpWebRequest request, Exception originalException)
        {
            Exception exception = (originalException == null) ? ioException : originalException;
            if (ioException.InnerException is SocketException)
            {
                return SocketConnection.ConvertTransferException((SocketException) ioException.InnerException, TimeSpan.FromMilliseconds((double) request.Timeout), exception);
            }
            return new CommunicationException(System.ServiceModel.SR.GetString("HttpTransferError", new object[] { exception.Message }), exception);
        }

        private static string CreateRequestTimedOutMessage(HttpWebRequest request)
        {
            return System.ServiceModel.SR.GetString("HttpRequestTimedOut", new object[] { request.RequestUri, TimeSpan.FromMilliseconds((double) request.Timeout) });
        }

        public static Exception CreateRequestWebException(WebException webException, HttpWebRequest request, HttpAbortReason abortReason)
        {
            Exception exception = ConvertWebException(webException, request, abortReason);
            if (webException.Response != null)
            {
                webException.Response.Close();
            }
            if (exception != null)
            {
                return exception;
            }
            if (webException.InnerException is IOException)
            {
                return CreateRequestIOException((IOException) webException.InnerException, request, webException);
            }
            if (webException.InnerException is SocketException)
            {
                return SocketConnectionInitiator.ConvertConnectException((SocketException) webException.InnerException, request.RequestUri, TimeSpan.MaxValue, webException);
            }
            return new EndpointNotFoundException(System.ServiceModel.SR.GetString("EndpointNotFound", new object[] { request.RequestUri.AbsoluteUri }), webException);
        }

        public static Exception CreateResponseIOException(IOException ioException, TimeSpan receiveTimeout)
        {
            if (ioException.InnerException is SocketException)
            {
                return SocketConnection.ConvertTransferException((SocketException) ioException.InnerException, receiveTimeout, ioException);
            }
            return new CommunicationException(System.ServiceModel.SR.GetString("HttpTransferError", new object[] { ioException.Message }), ioException);
        }

        public static Exception CreateResponseWebException(WebException webException, HttpWebResponse response)
        {
            switch (webException.Status)
            {
                case WebExceptionStatus.RequestCanceled:
                    return TraceResponseException(new CommunicationObjectAbortedException(System.ServiceModel.SR.GetString("HttpRequestAborted", new object[] { response.ResponseUri }), webException));

                case WebExceptionStatus.ConnectionClosed:
                    return TraceResponseException(new CommunicationException(webException.Message, webException));

                case WebExceptionStatus.Timeout:
                    return TraceResponseException(new TimeoutException(System.ServiceModel.SR.GetString("HttpResponseTimedOut", new object[] { response.ResponseUri, TimeSpan.FromMilliseconds((double) response.GetResponseStream().ReadTimeout) }), webException));
            }
            return CreateUnexpectedResponseException(webException, response);
        }

        private static Exception CreateUnexpectedResponseException(WebException responseException, HttpWebResponse response)
        {
            string statusDescription = response.StatusDescription;
            if (string.IsNullOrEmpty(statusDescription))
            {
                statusDescription = response.StatusCode.ToString();
            }
            return TraceResponseException(new ProtocolException(System.ServiceModel.SR.GetString("UnexpectedHttpResponseCode", new object[] { (int) response.StatusCode, statusDescription }), responseException));
        }

        public static NetworkCredential GetCredential(AuthenticationSchemes authenticationScheme, SecurityTokenProviderContainer credentialProvider, TimeSpan timeout, out TokenImpersonationLevel impersonationLevel, out AuthenticationLevel authenticationLevel)
        {
            impersonationLevel = TokenImpersonationLevel.None;
            authenticationLevel = AuthenticationLevel.None;
            NetworkCredential credential = null;
            if (authenticationScheme != AuthenticationSchemes.Anonymous)
            {
                credential = GetCredentialCore(authenticationScheme, credentialProvider, timeout, out impersonationLevel, out authenticationLevel);
            }
            return credential;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static NetworkCredential GetCredentialCore(AuthenticationSchemes authenticationScheme, SecurityTokenProviderContainer credentialProvider, TimeSpan timeout, out TokenImpersonationLevel impersonationLevel, out AuthenticationLevel authenticationLevel)
        {
            impersonationLevel = TokenImpersonationLevel.None;
            authenticationLevel = AuthenticationLevel.None;
            NetworkCredential userNameCredential = null;
            switch (authenticationScheme)
            {
                case AuthenticationSchemes.Digest:
                    userNameCredential = TransportSecurityHelpers.GetSspiCredential(credentialProvider, timeout, out impersonationLevel, out authenticationLevel);
                    ValidateDigestCredential(ref userNameCredential, impersonationLevel);
                    return userNameCredential;

                case AuthenticationSchemes.Negotiate:
                    return TransportSecurityHelpers.GetSspiCredential(credentialProvider, timeout, out impersonationLevel, out authenticationLevel);

                case AuthenticationSchemes.Ntlm:
                    userNameCredential = TransportSecurityHelpers.GetSspiCredential(credentialProvider, timeout, out impersonationLevel, out authenticationLevel);
                    if (authenticationLevel == AuthenticationLevel.MutualAuthRequired)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CredentialDisallowsNtlm")));
                    }
                    return userNameCredential;

                case AuthenticationSchemes.Basic:
                    userNameCredential = TransportSecurityHelpers.GetUserNameCredential(credentialProvider, timeout);
                    impersonationLevel = TokenImpersonationLevel.Delegation;
                    return userNameCredential;
            }
            throw Fx.AssertAndThrow("GetCredential: Invalid authentication scheme");
        }

        private static string GetResponseStreamString(HttpWebResponse webResponse, out int bytesRead)
        {
            Stream responseStream = webResponse.GetResponseStream();
            long contentLength = webResponse.ContentLength;
            if ((contentLength < 0L) || (contentLength > 0x400L))
            {
                contentLength = 0x400L;
            }
            byte[] buffer = DiagnosticUtility.Utility.AllocateByteArray((int) contentLength);
            bytesRead = responseStream.Read(buffer, 0, (int) contentLength);
            responseStream.Close();
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        public static HttpWebResponse ProcessGetResponseWebException(WebException webException, HttpWebRequest request, HttpAbortReason abortReason)
        {
            HttpWebResponse response = null;
            if ((webException.Status == WebExceptionStatus.Success) || (webException.Status == WebExceptionStatus.ProtocolError))
            {
                response = (HttpWebResponse) webException.Response;
            }
            if (response == null)
            {
                Exception exception = ConvertWebException(webException, request, abortReason);
                if (exception != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(webException.Message, webException));
            }
            bool flag = false;
            try
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new EndpointNotFoundException(System.ServiceModel.SR.GetString("EndpointNotFound", new object[] { request.RequestUri.AbsoluteUri }), webException));
                }
                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ServerTooBusyException(System.ServiceModel.SR.GetString("HttpServerTooBusy", new object[] { request.RequestUri.AbsoluteUri }), webException));
                }
                if (response.StatusCode == HttpStatusCode.UnsupportedMediaType)
                {
                    string statusDescription = response.StatusDescription;
                    if (!string.IsNullOrEmpty(statusDescription) && (string.Compare(statusDescription, "Missing Content Type", StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("MissingContentType", new object[] { request.RequestUri }), webException));
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("FramingContentTypeMismatch", new object[] { request.ContentType, request.RequestUri }), webException));
                }
                if (response.StatusCode == HttpStatusCode.GatewayTimeout)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(webException.Message, webException));
                }
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    string str2 = null;
                    if (response.ContentLength == "<h1>Bad Request (Invalid Hostname)</h1>".Length)
                    {
                        str2 = "<h1>Bad Request (Invalid Hostname)</h1>";
                    }
                    else if (response.ContentLength == "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\"\"http://www.w3.org/TR/html4/strict.dtd\">\r\n<HTML><HEAD><TITLE>Bad Request</TITLE>\r\n<META HTTP-EQUIV=\"Content-Type\" Content=\"text/html; charset=us-ascii\"></HEAD>\r\n<BODY><h2>Bad Request - Invalid Hostname</h2>\r\n<hr><p>HTTP Error 400. The request hostname is invalid.</p>\r\n</BODY></HTML>\r\n".Length)
                    {
                        str2 = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\"\"http://www.w3.org/TR/html4/strict.dtd\">\r\n<HTML><HEAD><TITLE>Bad Request</TITLE>\r\n<META HTTP-EQUIV=\"Content-Type\" Content=\"text/html; charset=us-ascii\"></HEAD>\r\n<BODY><h2>Bad Request - Invalid Hostname</h2>\r\n<hr><p>HTTP Error 400. The request hostname is invalid.</p>\r\n</BODY></HTML>\r\n";
                    }
                    if (str2 != null)
                    {
                        Stream responseStream = response.GetResponseStream();
                        byte[] buffer = new byte[str2.Length];
                        if ((responseStream.Read(buffer, 0, buffer.Length) == str2.Length) && (str2 == Encoding.ASCII.GetString(buffer)))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new EndpointNotFoundException(System.ServiceModel.SR.GetString("EndpointNotFound", new object[] { request.RequestUri.AbsoluteUri }), webException));
                        }
                    }
                }
                flag = true;
            }
            finally
            {
                if (!flag)
                {
                    response.Close();
                }
            }
            return response;
        }

        public static void SetRequestTimeout(HttpWebRequest request, TimeSpan timeout)
        {
            int num = TimeoutHelper.ToMilliseconds(timeout);
            if (num == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("HttpRequestTimedOut", new object[] { request.RequestUri, timeout })));
            }
            request.Timeout = num;
            request.ReadWriteTimeout = num;
        }

        private static Exception TraceResponseException(Exception exception)
        {
            if (DiagnosticUtility.ShouldTraceError)
            {
                TraceUtility.TraceEvent(TraceEventType.Error, 0x4000c, System.ServiceModel.SR.GetString("TraceCodeHttpChannelUnexpectedResponse"), null, exception);
            }
            return exception;
        }

        private static void ValidateAuthentication(HttpWebRequest request, HttpWebResponse response, WebException responseException, HttpChannelFactory factory)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                string message = System.ServiceModel.SR.GetString("HttpAuthorizationFailed", new object[] { factory.AuthenticationScheme, response.Headers[HttpResponseHeader.WwwAuthenticate] });
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(TraceResponseException(new MessageSecurityException(message, responseException)));
            }
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                string str2 = System.ServiceModel.SR.GetString("HttpAuthorizationForbidden", new object[] { factory.AuthenticationScheme });
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(TraceResponseException(new MessageSecurityException(str2, responseException)));
            }
            if ((request.AuthenticationLevel == AuthenticationLevel.MutualAuthRequired) && !response.IsMutuallyAuthenticated)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(TraceResponseException(new SecurityNegotiationException(System.ServiceModel.SR.GetString("HttpMutualAuthNotSatisfied"), responseException)));
            }
        }

        public static void ValidateDigestCredential(ref NetworkCredential credential, TokenImpersonationLevel impersonationLevel)
        {
            if (!System.ServiceModel.Security.SecurityUtils.IsDefaultNetworkCredential(credential) && !TokenImpersonationLevelHelper.IsGreaterOrEqual(impersonationLevel, TokenImpersonationLevel.Impersonation))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("DigestExplicitCredsImpersonationLevel", new object[] { impersonationLevel })));
            }
        }

        private static bool ValidateEmptyContent(HttpWebResponse response)
        {
            bool flag = true;
            if (response.ContentLength > 0L)
            {
                return false;
            }
            if (response.ContentLength == -1L)
            {
                Stream responseStream = response.GetResponseStream();
                byte[] buffer = new byte[1];
                flag = responseStream.Read(buffer, 0, 1) != 1;
            }
            return flag;
        }

        public static HttpInput ValidateRequestReplyResponse(HttpWebRequest request, HttpWebResponse response, HttpChannelFactory factory, WebException responseException, ChannelBinding channelBinding)
        {
            ValidateAuthentication(request, response, responseException, factory);
            HttpInput input = null;
            if (((HttpStatusCode.OK > response.StatusCode) || (response.StatusCode >= HttpStatusCode.MultipleChoices)) && (response.StatusCode != HttpStatusCode.InternalServerError))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnexpectedResponseException(responseException, response));
            }
            if ((response.StatusCode == HttpStatusCode.InternalServerError) && (string.Compare(response.StatusDescription, "System.ServiceModel.ServiceActivationException", StringComparison.OrdinalIgnoreCase) == 0))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ServiceActivationException(System.ServiceModel.SR.GetString("Hosting_ServiceActivationFailed", new object[] { request.RequestUri })));
            }
            if (string.IsNullOrEmpty(response.ContentType))
            {
                if (!ValidateEmptyContent(response))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(TraceResponseException(new ProtocolException(System.ServiceModel.SR.GetString("HttpContentTypeHeaderRequired"), responseException)));
                }
            }
            else if (response.ContentLength != 0L)
            {
                MessageEncoder encoder = factory.MessageEncoderFactory.Encoder;
                if (!encoder.IsContentTypeSupported(response.ContentType))
                {
                    int num;
                    string responseStreamString = GetResponseStreamString(response, out num);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(TraceResponseException(new ProtocolException(System.ServiceModel.SR.GetString("ResponseContentTypeMismatch", new object[] { response.ContentType, encoder.ContentType, num, responseStreamString }), responseException)));
                }
                input = HttpInput.CreateHttpInput(response, factory, channelBinding);
                input.WebException = responseException;
            }
            if ((input == null) && (factory.MessageEncoderFactory.MessageVersion == MessageVersion.None))
            {
                input = HttpInput.CreateHttpInput(response, factory, channelBinding);
                input.WebException = responseException;
            }
            return input;
        }

        internal static class StatusDescriptionStrings
        {
            internal const string HttpContentTypeMismatch = "Cannot process the message because the content type '{0}' was not the expected type '{1}'.";
            internal const string HttpContentTypeMissing = "Missing Content Type";
            internal const string HttpStatusServiceActivationException = "System.ServiceModel.ServiceActivationException";
        }
    }
}

