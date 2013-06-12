namespace System.Net
{
    using System;
    using System.IO;

    internal class CoreResponseData
    {
        public Stream m_ConnectStream;
        public long m_ContentLength;
        public bool m_IsVersionHttp11;
        public WebHeaderCollection m_ResponseHeaders;
        public HttpStatusCode m_StatusCode;
        public string m_StatusDescription;

        internal CoreResponseData Clone()
        {
            return new CoreResponseData { m_StatusCode = this.m_StatusCode, m_StatusDescription = this.m_StatusDescription, m_IsVersionHttp11 = this.m_IsVersionHttp11, m_ContentLength = this.m_ContentLength, m_ResponseHeaders = this.m_ResponseHeaders, m_ConnectStream = this.m_ConnectStream };
        }
    }
}

