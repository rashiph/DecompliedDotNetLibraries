namespace System.Net
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class FileWebResponse : WebResponse, ISerializable, ICloseEx
    {
        private const string DefaultFileContentType = "application/octet-stream";
        private const int DefaultFileStreamBufferSize = 0x2000;
        private bool m_closed;
        private long m_contentLength;
        private FileAccess m_fileAccess;
        private WebHeaderCollection m_headers;
        private Stream m_stream;
        private Uri m_uri;

        [Obsolete("Serialization is obsoleted for this type. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected FileWebResponse(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
            this.m_headers = (WebHeaderCollection) serializationInfo.GetValue("headers", typeof(WebHeaderCollection));
            this.m_uri = (Uri) serializationInfo.GetValue("uri", typeof(Uri));
            this.m_contentLength = serializationInfo.GetInt64("contentLength");
            this.m_fileAccess = (FileAccess) serializationInfo.GetInt32("fileAccess");
        }

        internal FileWebResponse(FileWebRequest request, Uri uri, FileAccess access, bool asyncHint)
        {
            try
            {
                this.m_fileAccess = access;
                if (access == FileAccess.Write)
                {
                    this.m_stream = Stream.Null;
                }
                else
                {
                    this.m_stream = new FileWebStream(request, uri.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read, 0x2000, asyncHint);
                    this.m_contentLength = this.m_stream.Length;
                }
                this.m_headers = new WebHeaderCollection(WebHeaderCollectionType.FileWebResponse);
                this.m_headers.AddInternal("Content-Length", this.m_contentLength.ToString(NumberFormatInfo.InvariantInfo));
                this.m_headers.AddInternal("Content-Type", "application/octet-stream");
                this.m_uri = uri;
            }
            catch (Exception exception)
            {
                Exception exception2 = new WebException(exception.Message, exception, WebExceptionStatus.ConnectFailure, null);
                throw exception2;
            }
        }

        private void CheckDisposed()
        {
            if (this.m_closed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
        }

        public override void Close()
        {
            ((ICloseEx) this).CloseEx(CloseExState.Normal);
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        protected override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            serializationInfo.AddValue("headers", this.m_headers, typeof(WebHeaderCollection));
            serializationInfo.AddValue("uri", this.m_uri, typeof(Uri));
            serializationInfo.AddValue("contentLength", this.m_contentLength);
            serializationInfo.AddValue("fileAccess", this.m_fileAccess);
            base.GetObjectData(serializationInfo, streamingContext);
        }

        public override Stream GetResponseStream()
        {
            this.CheckDisposed();
            return this.m_stream;
        }

        void ICloseEx.CloseEx(CloseExState closeState)
        {
            if (!this.m_closed)
            {
                this.m_closed = true;
                Stream stream = this.m_stream;
                if (stream != null)
                {
                    if (stream is ICloseEx)
                    {
                        ((ICloseEx) stream).CloseEx(closeState);
                    }
                    else
                    {
                        stream.Close();
                    }
                    this.m_stream = null;
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter, SerializationFormatter=true)]
        void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            this.GetObjectData(serializationInfo, streamingContext);
        }

        public override long ContentLength
        {
            get
            {
                this.CheckDisposed();
                return this.m_contentLength;
            }
        }

        public override string ContentType
        {
            get
            {
                this.CheckDisposed();
                return "application/octet-stream";
            }
        }

        public override WebHeaderCollection Headers
        {
            get
            {
                this.CheckDisposed();
                return this.m_headers;
            }
        }

        public override Uri ResponseUri
        {
            get
            {
                this.CheckDisposed();
                return this.m_uri;
            }
        }

        public override bool SupportsHeaders
        {
            get
            {
                return true;
            }
        }
    }
}

