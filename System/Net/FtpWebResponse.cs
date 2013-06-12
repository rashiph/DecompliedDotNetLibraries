namespace System.Net
{
    using System;
    using System.IO;

    public class FtpWebResponse : WebResponse, IDisposable
    {
        private string m_BannerMessage;
        private long m_ContentLength;
        private string m_ExitMessage;
        private WebHeaderCollection m_FtpRequestHeaders;
        private HttpWebResponse m_HttpWebResponse;
        private DateTime m_LastModified;
        internal Stream m_ResponseStream;
        private Uri m_ResponseUri;
        private FtpStatusCode m_StatusCode;
        private string m_StatusLine;
        private string m_WelcomeMessage;

        internal FtpWebResponse(HttpWebResponse httpWebResponse)
        {
            this.m_HttpWebResponse = httpWebResponse;
            base.InternalSetFromCache = this.m_HttpWebResponse.IsFromCache;
            base.InternalSetIsCacheFresh = this.m_HttpWebResponse.IsCacheFresh;
        }

        internal FtpWebResponse(Stream responseStream, long contentLength, Uri responseUri, FtpStatusCode statusCode, string statusLine, DateTime lastModified, string bannerMessage, string welcomeMessage, string exitMessage)
        {
            this.m_ResponseStream = responseStream;
            if ((responseStream == null) && (contentLength < 0L))
            {
                contentLength = 0L;
            }
            this.m_ContentLength = contentLength;
            this.m_ResponseUri = responseUri;
            this.m_StatusCode = statusCode;
            this.m_StatusLine = statusLine;
            this.m_LastModified = lastModified;
            this.m_BannerMessage = bannerMessage;
            this.m_WelcomeMessage = welcomeMessage;
            this.m_ExitMessage = exitMessage;
        }

        public override void Close()
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "Close", "");
            }
            if (this.HttpProxyMode)
            {
                this.m_HttpWebResponse.Close();
            }
            else
            {
                Stream responseStream = this.m_ResponseStream;
                if (responseStream != null)
                {
                    responseStream.Close();
                }
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "Close", "");
            }
        }

        public override Stream GetResponseStream()
        {
            if (this.HttpProxyMode)
            {
                return this.m_HttpWebResponse.GetResponseStream();
            }
            if (this.m_ResponseStream != null)
            {
                return this.m_ResponseStream;
            }
            return (this.m_ResponseStream = new EmptyStream());
        }

        internal void SetContentLength(long value)
        {
            if (!this.HttpProxyMode)
            {
                this.m_ContentLength = value;
            }
        }

        internal void SetResponseStream(Stream stream)
        {
            if (((stream != null) && (stream != Stream.Null)) && !(stream is EmptyStream))
            {
                this.m_ResponseStream = stream;
            }
        }

        internal void UpdateStatus(FtpStatusCode statusCode, string statusLine, string exitMessage)
        {
            this.m_StatusCode = statusCode;
            this.m_StatusLine = statusLine;
            this.m_ExitMessage = exitMessage;
        }

        public string BannerMessage
        {
            get
            {
                return this.m_BannerMessage;
            }
        }

        public override long ContentLength
        {
            get
            {
                if (this.HttpProxyMode)
                {
                    return this.m_HttpWebResponse.ContentLength;
                }
                return this.m_ContentLength;
            }
        }

        public string ExitMessage
        {
            get
            {
                return this.m_ExitMessage;
            }
        }

        public override WebHeaderCollection Headers
        {
            get
            {
                if (this.HttpProxyMode)
                {
                    return this.m_HttpWebResponse.Headers;
                }
                if (this.m_FtpRequestHeaders == null)
                {
                    lock (this)
                    {
                        if (this.m_FtpRequestHeaders == null)
                        {
                            this.m_FtpRequestHeaders = new WebHeaderCollection(WebHeaderCollectionType.FtpWebResponse);
                        }
                    }
                }
                return this.m_FtpRequestHeaders;
            }
        }

        private bool HttpProxyMode
        {
            get
            {
                return (this.m_HttpWebResponse != null);
            }
        }

        public DateTime LastModified
        {
            get
            {
                if (this.HttpProxyMode)
                {
                    return this.m_HttpWebResponse.LastModified;
                }
                return this.m_LastModified;
            }
        }

        public override Uri ResponseUri
        {
            get
            {
                if (this.HttpProxyMode)
                {
                    return this.m_HttpWebResponse.ResponseUri;
                }
                return this.m_ResponseUri;
            }
        }

        public FtpStatusCode StatusCode
        {
            get
            {
                if (this.HttpProxyMode)
                {
                    return (FtpStatusCode) this.m_HttpWebResponse.StatusCode;
                }
                return this.m_StatusCode;
            }
        }

        public string StatusDescription
        {
            get
            {
                if (this.HttpProxyMode)
                {
                    return this.m_HttpWebResponse.StatusDescription;
                }
                return this.m_StatusLine;
            }
        }

        public override bool SupportsHeaders
        {
            get
            {
                return true;
            }
        }

        public string WelcomeMessage
        {
            get
            {
                return this.m_WelcomeMessage;
            }
        }

        internal class EmptyStream : MemoryStream
        {
            internal EmptyStream() : base(new byte[0], false)
            {
            }
        }
    }
}

