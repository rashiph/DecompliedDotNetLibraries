namespace System.Net.Cache
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;

    internal class SingleItemRequestCache : WinInetCache
    {
        private FrozenCacheEntry _Entry;
        private bool _UseWinInet;

        internal SingleItemRequestCache(bool useWinInet) : base(true, true, false)
        {
            this._UseWinInet = useWinInet;
        }

        private void Commit(string key, RequestCacheEntry tempEntry, byte[] allBytes)
        {
            FrozenCacheEntry entry = new FrozenCacheEntry(key, tempEntry, allBytes);
            this._Entry = entry;
        }

        internal override void Remove(string key)
        {
            if (!this.TryRemove(key))
            {
                FileNotFoundException innerException = new FileNotFoundException(null, key);
                throw new IOException(SR.GetString("net_cache_retrieve_failure", new object[] { innerException.Message }), innerException);
            }
        }

        internal override Stream Retrieve(string key, out RequestCacheEntry cacheEntry)
        {
            Stream stream;
            if (!this.TryRetrieve(key, out cacheEntry, out stream))
            {
                FileNotFoundException innerException = new FileNotFoundException(null, key);
                throw new IOException(SR.GetString("net_cache_retrieve_failure", new object[] { innerException.Message }), innerException);
            }
            return stream;
        }

        internal override Stream Store(string key, long contentLength, DateTime expiresUtc, DateTime lastModifiedUtc, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata)
        {
            Stream stream;
            if (!this.TryStore(key, contentLength, expiresUtc, lastModifiedUtc, maxStale, entryMetadata, systemMetadata, out stream))
            {
                FileNotFoundException innerException = new FileNotFoundException(null, key);
                throw new IOException(SR.GetString("net_cache_retrieve_failure", new object[] { innerException.Message }), innerException);
            }
            return stream;
        }

        internal override bool TryRemove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (this._UseWinInet)
            {
                base.TryRemove(key);
            }
            FrozenCacheEntry entry = this._Entry;
            if ((entry != null) && (entry.Key == key))
            {
                this._Entry = null;
            }
            return true;
        }

        internal override bool TryRetrieve(string key, out RequestCacheEntry cacheEntry, out Stream readStream)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            FrozenCacheEntry clonedObject = this._Entry;
            cacheEntry = null;
            readStream = null;
            if ((clonedObject == null) || (clonedObject.Key != key))
            {
                Stream stream;
                RequestCacheEntry entry2;
                if (!this._UseWinInet || !base.TryRetrieve(key, out entry2, out stream))
                {
                    return false;
                }
                clonedObject = new FrozenCacheEntry(key, entry2, stream);
                stream.Close();
                this._Entry = clonedObject;
            }
            cacheEntry = FrozenCacheEntry.Create(clonedObject);
            readStream = new ReadOnlyStream(clonedObject.StreamBytes);
            return true;
        }

        internal override bool TryStore(string key, long contentLength, DateTime expiresUtc, DateTime lastModifiedUtc, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata, out Stream writeStream)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            RequestCacheEntry cacheEntry = new RequestCacheEntry {
                IsPrivateEntry = base.IsPrivateCache,
                StreamSize = contentLength,
                ExpiresUtc = expiresUtc,
                LastModifiedUtc = lastModifiedUtc,
                LastAccessedUtc = DateTime.UtcNow,
                LastSynchronizedUtc = DateTime.UtcNow,
                MaxStale = maxStale,
                HitCount = 0,
                UsageCount = 0,
                IsPartialEntry = false,
                EntryMetadata = entryMetadata,
                SystemMetadata = systemMetadata
            };
            writeStream = null;
            Stream stream = null;
            if (this._UseWinInet)
            {
                base.TryStore(key, contentLength, expiresUtc, lastModifiedUtc, maxStale, entryMetadata, systemMetadata, out stream);
            }
            writeStream = new WriteOnlyStream(key, this, cacheEntry, stream);
            return true;
        }

        internal override bool TryUpdate(string key, DateTime expiresUtc, DateTime lastModifiedUtc, DateTime lastSynchronizedUtc, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            FrozenCacheEntry entry = FrozenCacheEntry.Create(this._Entry);
            if ((entry != null) && (entry.Key == key))
            {
                entry.ExpiresUtc = expiresUtc;
                entry.LastModifiedUtc = lastModifiedUtc;
                entry.LastSynchronizedUtc = lastSynchronizedUtc;
                entry.MaxStale = maxStale;
                entry.EntryMetadata = entryMetadata;
                entry.SystemMetadata = systemMetadata;
                this._Entry = entry;
            }
            return true;
        }

        internal override void UnlockEntry(Stream stream)
        {
        }

        internal override void Update(string key, DateTime expiresUtc, DateTime lastModifiedUtc, DateTime lastSynchronizedUtc, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata)
        {
            if (!this.TryUpdate(key, expiresUtc, lastModifiedUtc, lastSynchronizedUtc, maxStale, entryMetadata, systemMetadata))
            {
                FileNotFoundException innerException = new FileNotFoundException(null, key);
                throw new IOException(SR.GetString("net_cache_retrieve_failure", new object[] { innerException.Message }), innerException);
            }
        }

        private sealed class FrozenCacheEntry : RequestCacheEntry
        {
            private string _Key;
            private byte[] _StreamBytes;

            public FrozenCacheEntry(string key, RequestCacheEntry entry, Stream stream) : this(key, entry, GetBytes(stream))
            {
            }

            public FrozenCacheEntry(string key, RequestCacheEntry entry, byte[] streamBytes)
            {
                this._Key = key;
                this._StreamBytes = streamBytes;
                base.IsPrivateEntry = entry.IsPrivateEntry;
                base.StreamSize = entry.StreamSize;
                base.ExpiresUtc = entry.ExpiresUtc;
                base.HitCount = entry.HitCount;
                base.LastAccessedUtc = entry.LastAccessedUtc;
                entry.LastModifiedUtc = entry.LastModifiedUtc;
                base.LastSynchronizedUtc = entry.LastSynchronizedUtc;
                base.MaxStale = entry.MaxStale;
                base.UsageCount = entry.UsageCount;
                base.IsPartialEntry = entry.IsPartialEntry;
                base.EntryMetadata = entry.EntryMetadata;
                base.SystemMetadata = entry.SystemMetadata;
            }

            public static SingleItemRequestCache.FrozenCacheEntry Create(SingleItemRequestCache.FrozenCacheEntry clonedObject)
            {
                if (clonedObject != null)
                {
                    return (SingleItemRequestCache.FrozenCacheEntry) clonedObject.MemberwiseClone();
                }
                return null;
            }

            private static byte[] GetBytes(Stream stream)
            {
                byte[] buffer;
                bool flag = false;
                if (stream.CanSeek)
                {
                    buffer = new byte[stream.Length];
                }
                else
                {
                    flag = true;
                    buffer = new byte[0x2000];
                }
                int offset = 0;
                while (true)
                {
                    int num2 = stream.Read(buffer, offset, buffer.Length - offset);
                    if (num2 == 0)
                    {
                        if (flag)
                        {
                            byte[] dst = new byte[offset];
                            Buffer.BlockCopy(buffer, 0, dst, 0, offset);
                            buffer = dst;
                        }
                        return buffer;
                    }
                    if (((offset += num2) == buffer.Length) && flag)
                    {
                        byte[] buffer2 = new byte[buffer.Length + 0x2000];
                        Buffer.BlockCopy(buffer, 0, buffer2, 0, offset);
                        buffer = buffer2;
                    }
                }
            }

            public string Key
            {
                get
                {
                    return this._Key;
                }
            }

            public byte[] StreamBytes
            {
                get
                {
                    return this._StreamBytes;
                }
            }
        }

        internal class ReadOnlyStream : Stream, IRequestLifetimeTracker
        {
            private byte[] _Bytes;
            private bool _Disposed;
            private int _Offset;
            private int _ReadTimeout;
            private int _WriteTimeout;
            private RequestLifetimeSetter m_RequestLifetimeSetter;

            internal ReadOnlyStream(byte[] bytes)
            {
                this._Bytes = bytes;
                this._Offset = 0;
                this._Disposed = false;
                this._ReadTimeout = this._WriteTimeout = -1;
            }

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                int num = this.Read(buffer, offset, count);
                LazyAsyncResult result = new LazyAsyncResult(null, state, callback);
                result.InvokeCallback(num);
                return result;
            }

            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
            {
                throw new NotSupportedException(SR.GetString("net_readonlystream"));
            }

            protected override void Dispose(bool disposing)
            {
                try
                {
                    if (!this._Disposed)
                    {
                        this._Disposed = true;
                        if (disposing)
                        {
                            RequestLifetimeSetter.Report(this.m_RequestLifetimeSetter);
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
                if (asyncResult == null)
                {
                    throw new ArgumentNullException("asyncResult");
                }
                LazyAsyncResult result = (LazyAsyncResult) asyncResult;
                if (result.EndCalled)
                {
                    throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndRead" }));
                }
                result.EndCalled = true;
                return (int) result.InternalWaitForCompletion();
            }

            public override void EndWrite(IAsyncResult asyncResult)
            {
                throw new NotSupportedException(SR.GetString("net_readonlystream"));
            }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (this._Disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if (buffer == null)
                {
                    throw new ArgumentNullException("buffer");
                }
                if ((offset < 0) || (offset > buffer.Length))
                {
                    throw new ArgumentOutOfRangeException("offset");
                }
                if ((count < 0) || (count > (buffer.Length - offset)))
                {
                    throw new ArgumentOutOfRangeException("count");
                }
                if (this._Offset == this._Bytes.Length)
                {
                    return 0;
                }
                int srcOffset = this._Offset;
                count = Math.Min(count, this._Bytes.Length - srcOffset);
                System.Buffer.BlockCopy(this._Bytes, srcOffset, buffer, offset, count);
                srcOffset += count;
                this._Offset = srcOffset;
                return count;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        return (this.Position = offset);

                    case SeekOrigin.Current:
                        return (this.Position += offset);

                    case SeekOrigin.End:
                        return (this.Position = this._Bytes.Length - offset);
                }
                throw new ArgumentException(SR.GetString("net_invalid_enum", new object[] { "SeekOrigin" }), "origin");
            }

            public override void SetLength(long length)
            {
                throw new NotSupportedException(SR.GetString("net_readonlystream"));
            }

            void IRequestLifetimeTracker.TrackRequestLifetime(long requestStartTimestamp)
            {
                this.m_RequestLifetimeSetter = new RequestLifetimeSetter(requestStartTimestamp);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException(SR.GetString("net_readonlystream"));
            }

            internal byte[] Buffer
            {
                get
                {
                    return this._Bytes;
                }
            }

            public override bool CanRead
            {
                get
                {
                    return true;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return true;
                }
            }

            public override bool CanTimeout
            {
                get
                {
                    return true;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return false;
                }
            }

            public override long Length
            {
                get
                {
                    return (long) this._Bytes.Length;
                }
            }

            public override long Position
            {
                get
                {
                    return (long) this._Offset;
                }
                set
                {
                    if ((value < 0L) || (value > this._Bytes.Length))
                    {
                        throw new ArgumentOutOfRangeException("value");
                    }
                    this._Offset = (int) value;
                }
            }

            public override int ReadTimeout
            {
                get
                {
                    return this._ReadTimeout;
                }
                set
                {
                    if ((value <= 0) && (value != -1))
                    {
                        throw new ArgumentOutOfRangeException("value", SR.GetString("net_io_timeout_use_gt_zero"));
                    }
                    this._ReadTimeout = value;
                }
            }

            public override int WriteTimeout
            {
                get
                {
                    return this._WriteTimeout;
                }
                set
                {
                    if ((value <= 0) && (value != -1))
                    {
                        throw new ArgumentOutOfRangeException("value", SR.GetString("net_io_timeout_use_gt_zero"));
                    }
                    this._WriteTimeout = value;
                }
            }
        }

        private class WriteOnlyStream : Stream
        {
            private ArrayList _Buffers;
            private SingleItemRequestCache _Cache;
            private bool _Disposed;
            private string _Key;
            private int _ReadTimeout;
            private Stream _RealStream;
            private RequestCacheEntry _TempEntry;
            private long _TotalSize;
            private int _WriteTimeout;

            public WriteOnlyStream(string key, SingleItemRequestCache cache, RequestCacheEntry cacheEntry, Stream realWriteStream)
            {
                this._Key = key;
                this._Cache = cache;
                this._TempEntry = cacheEntry;
                this._RealStream = realWriteStream;
                this._Buffers = new ArrayList();
            }

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                throw new NotSupportedException(SR.GetString("net_writeonlystream"));
            }

            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                this.Write(buffer, offset, count);
                LazyAsyncResult result = new LazyAsyncResult(null, state, callback);
                result.InvokeCallback(null);
                return result;
            }

            protected override void Dispose(bool disposing)
            {
                this._Disposed = true;
                base.Dispose(disposing);
                if (disposing)
                {
                    if (this._RealStream != null)
                    {
                        try
                        {
                            this._RealStream.Close();
                        }
                        catch
                        {
                        }
                    }
                    byte[] dst = new byte[this._TotalSize];
                    int dstOffset = 0;
                    for (int i = 0; i < this._Buffers.Count; i++)
                    {
                        byte[] src = (byte[]) this._Buffers[i];
                        Buffer.BlockCopy(src, 0, dst, dstOffset, src.Length);
                        dstOffset += src.Length;
                    }
                    this._Cache.Commit(this._Key, this._TempEntry, dst);
                }
            }

            public override int EndRead(IAsyncResult asyncResult)
            {
                throw new NotSupportedException(SR.GetString("net_writeonlystream"));
            }

            public override void EndWrite(IAsyncResult asyncResult)
            {
                if (asyncResult == null)
                {
                    throw new ArgumentNullException("asyncResult");
                }
                LazyAsyncResult result = (LazyAsyncResult) asyncResult;
                if (result.EndCalled)
                {
                    throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndWrite" }));
                }
                result.EndCalled = true;
                result.InternalWaitForCompletion();
            }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException(SR.GetString("net_writeonlystream"));
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException(SR.GetString("net_writeonlystream"));
            }

            public override void SetLength(long length)
            {
                throw new NotSupportedException(SR.GetString("net_writeonlystream"));
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (this._Disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if (buffer == null)
                {
                    throw new ArgumentNullException("buffer");
                }
                if ((offset < 0) || (offset > buffer.Length))
                {
                    throw new ArgumentOutOfRangeException("offset");
                }
                if ((count < 0) || (count > (buffer.Length - offset)))
                {
                    throw new ArgumentOutOfRangeException("count");
                }
                if (this._RealStream != null)
                {
                    try
                    {
                        this._RealStream.Write(buffer, offset, count);
                    }
                    catch
                    {
                        this._RealStream.Close();
                        this._RealStream = null;
                    }
                }
                byte[] dst = new byte[count];
                Buffer.BlockCopy(buffer, offset, dst, 0, count);
                this._Buffers.Add(dst);
                this._TotalSize += count;
            }

            public override bool CanRead
            {
                get
                {
                    return false;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return false;
                }
            }

            public override bool CanTimeout
            {
                get
                {
                    return true;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return true;
                }
            }

            public override long Length
            {
                get
                {
                    throw new NotSupportedException(SR.GetString("net_writeonlystream"));
                }
            }

            public override long Position
            {
                get
                {
                    throw new NotSupportedException(SR.GetString("net_writeonlystream"));
                }
                set
                {
                    throw new NotSupportedException(SR.GetString("net_writeonlystream"));
                }
            }

            public override int ReadTimeout
            {
                get
                {
                    return this._ReadTimeout;
                }
                set
                {
                    if ((value <= 0) && (value != -1))
                    {
                        throw new ArgumentOutOfRangeException("value", SR.GetString("net_io_timeout_use_gt_zero"));
                    }
                    this._ReadTimeout = value;
                }
            }

            public override int WriteTimeout
            {
                get
                {
                    return this._WriteTimeout;
                }
                set
                {
                    if ((value <= 0) && (value != -1))
                    {
                        throw new ArgumentOutOfRangeException("value", SR.GetString("net_io_timeout_use_gt_zero"));
                    }
                    this._WriteTimeout = value;
                }
            }
        }
    }
}

