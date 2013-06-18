namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.DirectoryServices;
    using System.IO;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Text;
    using System.Xml;

    public class DsmlSoapHttpConnection : DsmlSoapConnection
    {
        private string debugResponse;
        private System.DirectoryServices.Protocols.AuthType dsmlAuthType;
        private HttpWebRequest dsmlHttpConnection;
        private string dsmlSessionID;
        private string dsmlSoapAction;
        private Hashtable httpConnectionTable;

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true), WebPermission(SecurityAction.Assert, Unrestricted=true)]
        public DsmlSoapHttpConnection(DsmlDirectoryIdentifier identifier)
        {
            this.dsmlSoapAction = "\"#batchRequest\"";
            this.dsmlAuthType = System.DirectoryServices.Protocols.AuthType.Negotiate;
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }
            base.directoryIdentifier = identifier;
            this.dsmlHttpConnection = (HttpWebRequest) WebRequest.Create(((DsmlDirectoryIdentifier) base.directoryIdentifier).ServerUri);
            Hashtable table = new Hashtable();
            this.httpConnectionTable = Hashtable.Synchronized(table);
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public DsmlSoapHttpConnection(Uri uri) : this(new DsmlDirectoryIdentifier(uri))
        {
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true), SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode), EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
        public DsmlSoapHttpConnection(DsmlDirectoryIdentifier identifier, NetworkCredential credential) : this(identifier)
        {
            base.directoryCredential = (credential != null) ? new NetworkCredential(credential.UserName, credential.Password, credential.Domain) : null;
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public DsmlSoapHttpConnection(DsmlDirectoryIdentifier identifier, NetworkCredential credential, System.DirectoryServices.Protocols.AuthType authType) : this(identifier, credential)
        {
            this.AuthType = authType;
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public void Abort(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            if (!(asyncResult is DsmlAsyncResult))
            {
                throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("NotReturnedAsyncResult", new object[] { "asyncResult" }));
            }
            if (!this.httpConnectionTable.Contains(asyncResult))
            {
                throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("InvalidAsyncResult"));
            }
            HttpWebRequest request = (HttpWebRequest) this.httpConnectionTable[asyncResult];
            this.httpConnectionTable.Remove(asyncResult);
            request.Abort();
            DsmlAsyncResult result = (DsmlAsyncResult) asyncResult;
            result.resultObject.abortCalled = true;
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), NetworkInformationPermission(SecurityAction.Assert, Unrestricted=true), WebPermission(SecurityAction.Assert, Unrestricted=true)]
        public IAsyncResult BeginSendRequest(DsmlRequestDocument request, AsyncCallback callback, object state)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            HttpWebRequest dsmlConnection = (HttpWebRequest) WebRequest.Create(((DsmlDirectoryIdentifier) base.directoryIdentifier).ServerUri);
            this.PrepareHttpWebRequest(dsmlConnection);
            StringBuilder buffer = new StringBuilder(0x400);
            this.BeginSOAPRequest(ref buffer);
            buffer.Append(request.ToXml().InnerXml);
            this.EndSOAPRequest(ref buffer);
            RequestState state2 = new RequestState {
                request = dsmlConnection,
                requestString = buffer.ToString()
            };
            DsmlAsyncResult key = new DsmlAsyncResult(callback, state) {
                resultObject = state2
            };
            if (request.Count > 0)
            {
                key.hasValidRequest = true;
            }
            state2.dsmlAsync = key;
            this.httpConnectionTable.Add(key, dsmlConnection);
            dsmlConnection.BeginGetRequestStream(new AsyncCallback(DsmlSoapHttpConnection.RequestStreamCallback), state2);
            return key;
        }

        [NetworkInformationPermission(SecurityAction.Assert, Unrestricted=true), WebPermission(SecurityAction.Assert, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public override void BeginSession()
        {
            if (this.dsmlSessionID != null)
            {
                throw new InvalidOperationException(System.DirectoryServices.Protocols.Res.GetString("SessionInUse"));
            }
            try
            {
                this.PrepareHttpWebRequest(this.dsmlHttpConnection);
                StreamWriter webRequestStreamWriter = this.GetWebRequestStreamWriter();
                try
                {
                    webRequestStreamWriter.Write("<se:Envelope xmlns:se=\"http://schemas.xmlsoap.org/soap/envelope/\">");
                    webRequestStreamWriter.Write("<se:Header>");
                    webRequestStreamWriter.Write("<ad:BeginSession xmlns:ad=\"urn:schema-microsoft-com:activedirectory:dsmlv2\" se:mustUnderstand=\"1\"/>");
                    if (base.soapHeaders != null)
                    {
                        webRequestStreamWriter.Write(base.soapHeaders.OuterXml);
                    }
                    webRequestStreamWriter.Write("</se:Header>");
                    webRequestStreamWriter.Write("<se:Body xmlns=\"urn:oasis:names:tc:DSML:2:0:core\">");
                    webRequestStreamWriter.Write(new DsmlRequestDocument().ToXml().InnerXml);
                    webRequestStreamWriter.Write("</se:Body>");
                    webRequestStreamWriter.Write("</se:Envelope>");
                    webRequestStreamWriter.Flush();
                }
                finally
                {
                    webRequestStreamWriter.BaseStream.Close();
                    webRequestStreamWriter.Close();
                }
                HttpWebResponse resp = (HttpWebResponse) this.dsmlHttpConnection.GetResponse();
                try
                {
                    this.dsmlSessionID = this.ExtractSessionID(resp);
                }
                finally
                {
                    resp.Close();
                }
            }
            finally
            {
                this.dsmlHttpConnection = (HttpWebRequest) WebRequest.Create(((DsmlDirectoryIdentifier) base.directoryIdentifier).ServerUri);
            }
        }

        private void BeginSOAPRequest(ref StringBuilder buffer)
        {
            buffer.Append("<se:Envelope xmlns:se=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            if ((this.dsmlSessionID != null) || (base.soapHeaders != null))
            {
                buffer.Append("<se:Header>");
                if (this.dsmlSessionID != null)
                {
                    buffer.Append("<ad:Session xmlns:ad=\"urn:schema-microsoft-com:activedirectory:dsmlv2\" ad:SessionID=\"");
                    buffer.Append(this.dsmlSessionID);
                    buffer.Append("\" se:mustUnderstand=\"1\"/>");
                }
                if (base.soapHeaders != null)
                {
                    buffer.Append(base.soapHeaders.OuterXml);
                }
                buffer.Append("</se:Header>");
            }
            buffer.Append("<se:Body xmlns=\"urn:oasis:names:tc:DSML:2:0:core\">");
        }

        public DsmlResponseDocument EndSendRequest(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            if (!(asyncResult is DsmlAsyncResult))
            {
                throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("NotReturnedAsyncResult", new object[] { "asyncResult" }));
            }
            if (!this.httpConnectionTable.Contains(asyncResult))
            {
                throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("InvalidAsyncResult"));
            }
            this.httpConnectionTable.Remove(asyncResult);
            DsmlAsyncResult result = (DsmlAsyncResult) asyncResult;
            asyncResult.AsyncWaitHandle.WaitOne();
            if (result.resultObject.exception != null)
            {
                throw result.resultObject.exception;
            }
            DsmlResponseDocument document = new DsmlResponseDocument(result.resultObject.responseString, "se:Envelope/se:Body/dsml:batchResponse");
            this.debugResponse = document.ResponseString;
            if (result.hasValidRequest && (document.Count == 0))
            {
                throw new DsmlInvalidDocumentException(System.DirectoryServices.Protocols.Res.GetString("MissingResponse"));
            }
            return document;
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), NetworkInformationPermission(SecurityAction.Assert, Unrestricted=true), WebPermission(SecurityAction.Assert, Unrestricted=true)]
        public override void EndSession()
        {
            if (this.dsmlSessionID == null)
            {
                throw new InvalidOperationException(System.DirectoryServices.Protocols.Res.GetString("NoCurrentSession"));
            }
            try
            {
                try
                {
                    this.PrepareHttpWebRequest(this.dsmlHttpConnection);
                    StreamWriter webRequestStreamWriter = this.GetWebRequestStreamWriter();
                    try
                    {
                        webRequestStreamWriter.Write("<se:Envelope xmlns:se=\"http://schemas.xmlsoap.org/soap/envelope/\">");
                        webRequestStreamWriter.Write("<se:Header>");
                        webRequestStreamWriter.Write("<ad:EndSession xmlns:ad=\"urn:schema-microsoft-com:activedirectory:dsmlv2\" ad:SessionID=\"");
                        webRequestStreamWriter.Write(this.dsmlSessionID);
                        webRequestStreamWriter.Write("\" se:mustUnderstand=\"1\"/>");
                        if (base.soapHeaders != null)
                        {
                            webRequestStreamWriter.Write(base.soapHeaders.OuterXml);
                        }
                        webRequestStreamWriter.Write("</se:Header>");
                        webRequestStreamWriter.Write("<se:Body xmlns=\"urn:oasis:names:tc:DSML:2:0:core\">");
                        webRequestStreamWriter.Write(new DsmlRequestDocument().ToXml().InnerXml);
                        webRequestStreamWriter.Write("</se:Body>");
                        webRequestStreamWriter.Write("</se:Envelope>");
                        webRequestStreamWriter.Flush();
                    }
                    finally
                    {
                        webRequestStreamWriter.BaseStream.Close();
                        webRequestStreamWriter.Close();
                    }
                    ((HttpWebResponse) this.dsmlHttpConnection.GetResponse()).Close();
                }
                catch (WebException exception)
                {
                    if ((((exception.Status != WebExceptionStatus.ConnectFailure) && (exception.Status != WebExceptionStatus.NameResolutionFailure)) && ((exception.Status != WebExceptionStatus.ProxyNameResolutionFailure) && (exception.Status != WebExceptionStatus.SendFailure))) && (exception.Status != WebExceptionStatus.TrustFailure))
                    {
                        this.dsmlSessionID = null;
                    }
                    throw;
                }
                this.dsmlSessionID = null;
            }
            finally
            {
                this.dsmlHttpConnection = (HttpWebRequest) WebRequest.Create(((DsmlDirectoryIdentifier) base.directoryIdentifier).ServerUri);
            }
        }

        private void EndSOAPRequest(ref StringBuilder buffer)
        {
            buffer.Append("</se:Body>");
            buffer.Append("</se:Envelope>");
        }

        private string ExtractSessionID(HttpWebResponse resp)
        {
            string str;
            StreamReader txtReader = new StreamReader(resp.GetResponseStream());
            try
            {
                XmlDocument document = new XmlDocument();
                try
                {
                    document.Load(txtReader);
                }
                catch (XmlException)
                {
                    throw new DsmlInvalidDocumentException();
                }
                XmlNamespaceManager dsmlNamespaceManager = NamespaceUtils.GetDsmlNamespaceManager();
                XmlAttribute attribute = (XmlAttribute) document.SelectSingleNode("se:Envelope/se:Header/ad:Session/@ad:SessionID", dsmlNamespaceManager);
                if (attribute == null)
                {
                    attribute = (XmlAttribute) document.SelectSingleNode("se:Envelope/se:Header/ad:Session/@SessionID", dsmlNamespaceManager);
                    if (attribute == null)
                    {
                        throw new DsmlInvalidDocumentException(System.DirectoryServices.Protocols.Res.GetString("NoSessionIDReturned"));
                    }
                }
                str = attribute.Value;
            }
            finally
            {
                txtReader.Close();
            }
            return str;
        }

        private StreamWriter GetWebRequestStreamWriter()
        {
            return new StreamWriter(this.dsmlHttpConnection.GetRequestStream());
        }

        [EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
        private void PrepareHttpWebRequest(HttpWebRequest dsmlConnection)
        {
            if (base.directoryCredential == null)
            {
                dsmlConnection.Credentials = CredentialCache.DefaultCredentials;
            }
            else
            {
                string authType = "negotiate";
                if (this.dsmlAuthType == System.DirectoryServices.Protocols.AuthType.Ntlm)
                {
                    authType = "NTLM";
                }
                else if (this.dsmlAuthType == System.DirectoryServices.Protocols.AuthType.Basic)
                {
                    authType = "basic";
                }
                else if (this.dsmlAuthType == System.DirectoryServices.Protocols.AuthType.Anonymous)
                {
                    authType = "anonymous";
                }
                else if (this.dsmlAuthType == System.DirectoryServices.Protocols.AuthType.Digest)
                {
                    authType = "digest";
                }
                CredentialCache cache = new CredentialCache();
                cache.Add(dsmlConnection.RequestUri, authType, base.directoryCredential);
                dsmlConnection.Credentials = cache;
            }
            foreach (X509Certificate certificate in base.ClientCertificates)
            {
                dsmlConnection.ClientCertificates.Add(certificate);
            }
            if (this.connectionTimeOut.Ticks != 0L)
            {
                dsmlConnection.Timeout = (int) (this.connectionTimeOut.Ticks / 0x2710L);
            }
            if (this.dsmlSoapAction != null)
            {
                dsmlConnection.Headers.Set("SOAPAction", this.dsmlSoapAction);
            }
            dsmlConnection.Method = "POST";
        }

        private static void ReadCallback(IAsyncResult asyncResult)
        {
            RequestState asyncState = (RequestState) asyncResult.AsyncState;
            int num = 0;
            string str = null;
            try
            {
                num = asyncState.responseStream.EndRead(asyncResult);
                if (num > 0)
                {
                    str = asyncState.encoder.GetString(asyncState.bufferRead);
                    int count = Math.Min(str.Length, num);
                    asyncState.responseString.Append(str, 0, count);
                    asyncState.responseStream.BeginRead(asyncState.bufferRead, 0, 0x400, new AsyncCallback(DsmlSoapHttpConnection.ReadCallback), asyncState);
                }
                else
                {
                    asyncState.responseStream.Close();
                    WakeupRoutine(asyncState);
                }
            }
            catch (Exception exception)
            {
                asyncState.responseStream.Close();
                asyncState.exception = exception;
                WakeupRoutine(asyncState);
            }
        }

        private static void RequestStreamCallback(IAsyncResult asyncResult)
        {
            RequestState asyncState = (RequestState) asyncResult.AsyncState;
            HttpWebRequest request = asyncState.request;
            try
            {
                asyncState.requestStream = request.EndGetRequestStream(asyncResult);
                byte[] bytes = asyncState.encoder.GetBytes(asyncState.requestString);
                asyncState.requestStream.BeginWrite(bytes, 0, bytes.Length, new AsyncCallback(DsmlSoapHttpConnection.WriteCallback), asyncState);
            }
            catch (Exception exception)
            {
                if (asyncState.requestStream != null)
                {
                    asyncState.requestStream.Close();
                }
                asyncState.exception = exception;
                WakeupRoutine(asyncState);
            }
        }

        private static void ResponseCallback(IAsyncResult asyncResult)
        {
            RequestState asyncState = (RequestState) asyncResult.AsyncState;
            try
            {
                asyncState.responseStream = asyncState.request.EndGetResponse(asyncResult).GetResponseStream();
                asyncState.responseStream.BeginRead(asyncState.bufferRead, 0, 0x400, new AsyncCallback(DsmlSoapHttpConnection.ReadCallback), asyncState);
            }
            catch (Exception exception)
            {
                if (asyncState.responseStream != null)
                {
                    asyncState.responseStream.Close();
                }
                asyncState.exception = exception;
                WakeupRoutine(asyncState);
            }
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public override DirectoryResponse SendRequest(DirectoryRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            DsmlRequestDocument document = new DsmlRequestDocument();
            document.Add(request);
            DsmlResponseDocument document2 = this.SendRequestHelper(document.ToXml().InnerXml);
            if (document2.Count == 0)
            {
                throw new DsmlInvalidDocumentException(System.DirectoryServices.Protocols.Res.GetString("MissingResponse"));
            }
            DirectoryResponse response = document2[0];
            if (response is DsmlErrorResponse)
            {
                ErrorResponseException exception = new ErrorResponseException((DsmlErrorResponse) response);
                throw exception;
            }
            ResultCode resultCode = response.ResultCode;
            if (((resultCode != ResultCode.Success) && (resultCode != ResultCode.CompareFalse)) && (((resultCode != ResultCode.CompareTrue) && (resultCode != ResultCode.Referral)) && (resultCode != ResultCode.ReferralV2)))
            {
                throw new DirectoryOperationException(response, OperationErrorMappings.MapResultCode((int) resultCode));
            }
            return response;
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public DsmlResponseDocument SendRequest(DsmlRequestDocument request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            DsmlResponseDocument document = this.SendRequestHelper(request.ToXml().InnerXml);
            if ((request.Count > 0) && (document.Count == 0))
            {
                throw new DsmlInvalidDocumentException(System.DirectoryServices.Protocols.Res.GetString("MissingResponse"));
            }
            return document;
        }

        [NetworkInformationPermission(SecurityAction.Assert, Unrestricted=true), WebPermission(SecurityAction.Assert, Unrestricted=true)]
        private DsmlResponseDocument SendRequestHelper(string reqstring)
        {
            DsmlResponseDocument document2;
            StringBuilder buffer = new StringBuilder(0x400);
            try
            {
                DsmlResponseDocument document;
                this.PrepareHttpWebRequest(this.dsmlHttpConnection);
                StreamWriter webRequestStreamWriter = this.GetWebRequestStreamWriter();
                try
                {
                    this.BeginSOAPRequest(ref buffer);
                    buffer.Append(reqstring);
                    this.EndSOAPRequest(ref buffer);
                    webRequestStreamWriter.Write(buffer.ToString());
                    webRequestStreamWriter.Flush();
                }
                finally
                {
                    webRequestStreamWriter.BaseStream.Close();
                    webRequestStreamWriter.Close();
                }
                HttpWebResponse resp = (HttpWebResponse) this.dsmlHttpConnection.GetResponse();
                try
                {
                    document = new DsmlResponseDocument(resp, "se:Envelope/se:Body/dsml:batchResponse");
                    this.debugResponse = document.ResponseString;
                }
                finally
                {
                    resp.Close();
                }
                document2 = document;
            }
            finally
            {
                this.dsmlHttpConnection = (HttpWebRequest) WebRequest.Create(((DsmlDirectoryIdentifier) base.directoryIdentifier).ServerUri);
            }
            return document2;
        }

        private static void WakeupRoutine(RequestState rs)
        {
            rs.dsmlAsync.manualResetEvent.Set();
            rs.dsmlAsync.completed = true;
            if ((rs.dsmlAsync.callback != null) && !rs.abortCalled)
            {
                rs.dsmlAsync.callback(rs.dsmlAsync);
            }
        }

        private static void WriteCallback(IAsyncResult asyncResult)
        {
            RequestState asyncState = (RequestState) asyncResult.AsyncState;
            try
            {
                asyncState.requestStream.EndWrite(asyncResult);
                asyncState.request.BeginGetResponse(new AsyncCallback(DsmlSoapHttpConnection.ResponseCallback), asyncState);
            }
            catch (Exception exception)
            {
                asyncState.exception = exception;
                WakeupRoutine(asyncState);
            }
            finally
            {
                asyncState.requestStream.Close();
            }
        }

        public System.DirectoryServices.Protocols.AuthType AuthType
        {
            get
            {
                return this.dsmlAuthType;
            }
            set
            {
                if ((value < System.DirectoryServices.Protocols.AuthType.Anonymous) || (value > System.DirectoryServices.Protocols.AuthType.Kerberos))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.DirectoryServices.Protocols.AuthType));
                }
                if ((((value != System.DirectoryServices.Protocols.AuthType.Anonymous) && (value != System.DirectoryServices.Protocols.AuthType.Ntlm)) && ((value != System.DirectoryServices.Protocols.AuthType.Basic) && (value != System.DirectoryServices.Protocols.AuthType.Negotiate))) && (value != System.DirectoryServices.Protocols.AuthType.Digest))
                {
                    throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("WrongAuthType", new object[] { value }), "value");
                }
                this.dsmlAuthType = value;
            }
        }

        private string ResponseString
        {
            get
            {
                return this.debugResponse;
            }
        }

        public override string SessionId
        {
            get
            {
                return this.dsmlSessionID;
            }
        }

        public string SoapActionHeader
        {
            get
            {
                return this.dsmlSoapAction;
            }
            set
            {
                this.dsmlSoapAction = value;
            }
        }

        public override TimeSpan Timeout
        {
            get
            {
                return base.connectionTimeOut;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("NoNegativeTime"), "value");
                }
                if (value.TotalMilliseconds > 2147483647.0)
                {
                    throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("TimespanExceedMax"), "value");
                }
                base.connectionTimeOut = value;
            }
        }
    }
}

