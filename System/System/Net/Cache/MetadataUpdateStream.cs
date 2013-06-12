namespace System.Net.Cache
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Net;
    using System.Threading;

    internal class MetadataUpdateStream : BaseWrapperStream, ICloseEx
    {
        private int _Disposed;
        private RequestCache m_Cache;
        private bool m_CacheDestroy;
        private StringCollection m_EntryMetadata;
        private DateTime m_Expires;
        private bool m_IsStrictCacheErrors;
        private string m_Key;
        private DateTime m_LastModified;
        private DateTime m_LastSynchronized;
        private TimeSpan m_MaxStale;
        private StringCollection m_SystemMetadata;

        private MetadataUpdateStream(Stream parentStream, RequestCache cache, string key, bool isStrictCacheErrors) : base(parentStream)
        {
            this.m_Cache = cache;
            this.m_Key = key;
            this.m_CacheDestroy = true;
            this.m_IsStrictCacheErrors = isStrictCacheErrors;
        }

        internal MetadataUpdateStream(Stream parentStream, RequestCache cache, string key, DateTime expiresGMT, DateTime lastModifiedGMT, DateTime lastSynchronizedGMT, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata, bool isStrictCacheErrors) : base(parentStream)
        {
            this.m_Cache = cache;
            this.m_Key = key;
            this.m_Expires = expiresGMT;
            this.m_LastModified = lastModifiedGMT;
            this.m_LastSynchronized = lastSynchronizedGMT;
            this.m_MaxStale = maxStale;
            this.m_EntryMetadata = entryMetadata;
            this.m_SystemMetadata = systemMetadata;
            this.m_IsStrictCacheErrors = isStrictCacheErrors;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return base.WrappedStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return base.WrappedStream.BeginWrite(buffer, offset, count, callback, state);
        }

        protected sealed override void Dispose(bool disposing)
        {
            this.Dispose(disposing, CloseExState.Normal);
        }

        protected virtual void Dispose(bool disposing, CloseExState closeState)
        {
            try
            {
                if ((Interlocked.Increment(ref this._Disposed) == 1) && disposing)
                {
                    ICloseEx wrappedStream = base.WrappedStream as ICloseEx;
                    if (wrappedStream != null)
                    {
                        wrappedStream.CloseEx(closeState);
                    }
                    else
                    {
                        base.WrappedStream.Close();
                    }
                    if (this.m_CacheDestroy)
                    {
                        if (this.m_IsStrictCacheErrors)
                        {
                            this.m_Cache.Remove(this.m_Key);
                        }
                        else
                        {
                            this.m_Cache.TryRemove(this.m_Key);
                        }
                    }
                    else if (this.m_IsStrictCacheErrors)
                    {
                        this.m_Cache.Update(this.m_Key, this.m_Expires, this.m_LastModified, this.m_LastSynchronized, this.m_MaxStale, this.m_EntryMetadata, this.m_SystemMetadata);
                    }
                    else
                    {
                        this.m_Cache.TryUpdate(this.m_Key, this.m_Expires, this.m_LastModified, this.m_LastSynchronized, this.m_MaxStale, this.m_EntryMetadata, this.m_SystemMetadata);
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return base.WrappedStream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            base.WrappedStream.EndWrite(asyncResult);
        }

        public override void Flush()
        {
            base.WrappedStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return base.WrappedStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return base.WrappedStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            base.WrappedStream.SetLength(value);
        }

        void ICloseEx.CloseEx(CloseExState closeState)
        {
            this.Dispose(true, closeState);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            base.WrappedStream.Write(buffer, offset, count);
        }

        public override bool CanRead
        {
            get
            {
                return base.WrappedStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return base.WrappedStream.CanSeek;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                return base.WrappedStream.CanTimeout;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return base.WrappedStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return base.WrappedStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return base.WrappedStream.Position;
            }
            set
            {
                base.WrappedStream.Position = value;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                return base.WrappedStream.ReadTimeout;
            }
            set
            {
                base.WrappedStream.ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                return base.WrappedStream.WriteTimeout;
            }
            set
            {
                base.WrappedStream.WriteTimeout = value;
            }
        }
    }
}

