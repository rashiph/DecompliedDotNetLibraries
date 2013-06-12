namespace Microsoft.Win32
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Cache;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    internal class WinInetCache : RequestCache
    {
        private static int _MaximumResponseHeadersLength;
        private bool async;
        internal const string c_SPARSE_ENTRY_HACK = "~SPARSE_ENTRY:";
        internal static readonly TimeSpan s_MaxTimeSpanForInt32 = TimeSpan.FromSeconds(2147483647.0);
        private static readonly DateTime s_MinDateTimeUtcForFileTimeUtc = DateTime.FromFileTimeUtc(0L);

        internal WinInetCache(bool isPrivateCache, bool canWrite, bool async) : base(isPrivateCache, canWrite)
        {
            _MaximumResponseHeadersLength = 0x7fffffff;
            this.async = async;
        }

        private string CombineMetaInfo(StringCollection entryMetadata, StringCollection systemMetadata)
        {
            int num;
            if (((entryMetadata == null) || (entryMetadata.Count == 0)) && ((systemMetadata == null) || (systemMetadata.Count == 0)))
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder(100);
            if ((entryMetadata != null) && (entryMetadata.Count != 0))
            {
                for (num = 0; num < entryMetadata.Count; num++)
                {
                    if ((entryMetadata[num] != null) && (entryMetadata[num].Length != 0))
                    {
                        builder.Append(entryMetadata[num]).Append("\r\n");
                    }
                }
            }
            if ((systemMetadata != null) && (systemMetadata.Count != 0))
            {
                builder.Append("\r\n");
                for (num = 0; num < systemMetadata.Count; num++)
                {
                    if ((systemMetadata[num] != null) && (systemMetadata[num].Length != 0))
                    {
                        builder.Append(systemMetadata[num]).Append("\r\n");
                    }
                }
            }
            return builder.ToString();
        }

        private Stream GetWriteStream(string key, long contentLength, DateTime expiresUtc, DateTime lastModifiedUtc, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata, bool isThrow)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.RequestCache, "WinInetCache.Store()", "Key = " + key);
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (!base.CanWrite)
            {
                if (Logging.On)
                {
                    Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_operation_failed_with_error", new object[] { "WinInetCache.Store()", SR.GetString("net_cache_access_denied", new object[] { "Write" }) }));
                }
                if (Logging.On)
                {
                    Logging.Exit(Logging.RequestCache, "WinInetCache.Store");
                }
                if (isThrow)
                {
                    throw new InvalidOperationException(SR.GetString("net_cache_access_denied", new object[] { "Write" }));
                }
                return null;
            }
            _WinInetCache.Entry entry = new _WinInetCache.Entry(key, _MaximumResponseHeadersLength) {
                Key = key,
                OptionalLength = (contentLength < 0L) ? 0 : ((contentLength > 0x7fffffffL) ? 0x7fffffff : ((int) contentLength))
            };
            entry.Info.ExpireTime = _WinInetCache.FILETIME.Zero;
            if ((expiresUtc != DateTime.MinValue) && (expiresUtc > s_MinDateTimeUtcForFileTimeUtc))
            {
                entry.Info.ExpireTime = new _WinInetCache.FILETIME(expiresUtc.ToFileTimeUtc());
            }
            entry.Info.LastModifiedTime = _WinInetCache.FILETIME.Zero;
            if ((lastModifiedUtc != DateTime.MinValue) && (lastModifiedUtc > s_MinDateTimeUtcForFileTimeUtc))
            {
                entry.Info.LastModifiedTime = new _WinInetCache.FILETIME(lastModifiedUtc.ToFileTimeUtc());
            }
            entry.Info.EntryType = _WinInetCache.EntryType.NormalEntry;
            if (maxStale > TimeSpan.Zero)
            {
                if (maxStale >= s_MaxTimeSpanForInt32)
                {
                    maxStale = s_MaxTimeSpanForInt32;
                }
                entry.Info.U.ExemptDelta = (int) maxStale.TotalSeconds;
                entry.Info.EntryType = _WinInetCache.EntryType.StickyEntry;
            }
            entry.MetaInfo = this.CombineMetaInfo(entryMetadata, systemMetadata);
            entry.FileExt = "cache";
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_expected_length", new object[] { entry.OptionalLength }));
                Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_last_modified", new object[] { entry.Info.LastModifiedTime.IsNull ? "0" : DateTime.FromFileTimeUtc(entry.Info.LastModifiedTime.ToLong()).ToString("r") }));
                Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_expires", new object[] { entry.Info.ExpireTime.IsNull ? "0" : DateTime.FromFileTimeUtc(entry.Info.ExpireTime.ToLong()).ToString("r") }));
                Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_max_stale", new object[] { (maxStale > TimeSpan.Zero) ? ((int) maxStale.TotalSeconds).ToString() : "n/a" }));
                if (Logging.IsVerbose(Logging.RequestCache))
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_dumping_metadata"));
                    if (entry.MetaInfo.Length == 0)
                    {
                        Logging.PrintInfo(Logging.RequestCache, "<null>");
                    }
                    else
                    {
                        if (entryMetadata != null)
                        {
                            foreach (string str in entryMetadata)
                            {
                                Logging.PrintInfo(Logging.RequestCache, str.TrimEnd(RequestCache.LineSplits));
                            }
                        }
                        Logging.PrintInfo(Logging.RequestCache, "------");
                        if (systemMetadata != null)
                        {
                            foreach (string str2 in systemMetadata)
                            {
                                Logging.PrintInfo(Logging.RequestCache, str2.TrimEnd(RequestCache.LineSplits));
                            }
                        }
                    }
                }
            }
            _WinInetCache.CreateFileName(entry);
            Stream @null = Stream.Null;
            if (entry.Error != _WinInetCache.Status.Success)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_create_failed", new object[] { new Win32Exception((int) entry.Error).Message }));
                    Logging.Exit(Logging.RequestCache, "WinInetCache.Store");
                }
                if (isThrow)
                {
                    Win32Exception innerException = new Win32Exception((int) entry.Error);
                    throw new IOException(SR.GetString("net_cache_retrieve_failure", new object[] { innerException.Message }), innerException);
                }
                return null;
            }
            try
            {
                @null = new WriteStream(entry, isThrow, contentLength, this.async);
            }
            catch (Exception exception2)
            {
                if (((exception2 is ThreadAbortException) || (exception2 is StackOverflowException)) || (exception2 is OutOfMemoryException))
                {
                    throw;
                }
                if (Logging.On)
                {
                    Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_exception", new object[] { "WinInetCache.Store()", exception2 }));
                    Logging.Exit(Logging.RequestCache, "WinInetCache.Store");
                }
                if (isThrow)
                {
                    throw;
                }
                return null;
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.RequestCache, "WinInetCache.Store", "Filename = " + entry.Filename);
            }
            return @null;
        }

        private unsafe Stream Lookup(string key, out RequestCacheEntry cacheEntry, bool isThrow)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.RequestCache, "WinInetCache.Retrieve", "key = " + key);
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            Stream @null = Stream.Null;
            SafeUnlockUrlCacheEntryFile handle = null;
            _WinInetCache.Entry entry = new _WinInetCache.Entry(key, _MaximumResponseHeadersLength);
            try
            {
                handle = _WinInetCache.LookupFile(entry);
                if (entry.Error == _WinInetCache.Status.Success)
                {
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_filename", new object[] { "WinInetCache.Retrieve()", entry.Filename, entry.Error }));
                    }
                    cacheEntry = new RequestCacheEntry(entry, base.IsPrivateCache);
                    if ((entry.MetaInfo != null) && (entry.MetaInfo.Length != 0))
                    {
                        int startIndex = 0;
                        int length = entry.MetaInfo.Length;
                        StringCollection strings = new StringCollection();
                        fixed (char* str2 = ((char*) entry.MetaInfo))
                        {
                            char* chPtr = str2;
                            for (int i = 0; i < length; i++)
                            {
                                if ((((i == startIndex) && ((i + 2) < length)) && (chPtr[i] == '~')) && (((chPtr[i + 1] == 'U') || (chPtr[i + 1] == 'u')) && (chPtr[i + 2] == ':')))
                                {
                                    while ((i < length) && (chPtr[++i] != '\n'))
                                    {
                                    }
                                    startIndex = i + 1;
                                }
                                else if (((i + 1) == length) || (chPtr[i] == '\n'))
                                {
                                    string str = entry.MetaInfo.Substring(startIndex, ((chPtr[i - 1] == '\r') ? (i - 1) : (i + 1)) - startIndex);
                                    if ((str.Length == 0) && (cacheEntry.EntryMetadata == null))
                                    {
                                        cacheEntry.EntryMetadata = strings;
                                        strings = new StringCollection();
                                    }
                                    else if ((cacheEntry.EntryMetadata != null) && str.StartsWith("~SPARSE_ENTRY:", StringComparison.Ordinal))
                                    {
                                        cacheEntry.IsPartialEntry = true;
                                    }
                                    else
                                    {
                                        strings.Add(str);
                                    }
                                    startIndex = i + 1;
                                }
                            }
                        }
                        if (cacheEntry.EntryMetadata == null)
                        {
                            cacheEntry.EntryMetadata = strings;
                        }
                        else
                        {
                            cacheEntry.SystemMetadata = strings;
                        }
                    }
                    @null = new ReadStream(entry, handle, this.async);
                }
                else
                {
                    if (handle != null)
                    {
                        handle.Close();
                    }
                    cacheEntry = new RequestCacheEntry();
                    cacheEntry.IsPrivateEntry = base.IsPrivateCache;
                    if (entry.Error != _WinInetCache.Status.FileNotFound)
                    {
                        if (Logging.On)
                        {
                            Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_lookup_failed", new object[] { "WinInetCache.Retrieve()", new Win32Exception((int) entry.Error).Message }));
                        }
                        if (Logging.On)
                        {
                            Logging.Exit(Logging.RequestCache, "WinInetCache.Retrieve()");
                        }
                        if (isThrow)
                        {
                            Win32Exception innerException = new Win32Exception((int) entry.Error);
                            throw new IOException(SR.GetString("net_cache_retrieve_failure", new object[] { innerException.Message }), innerException);
                        }
                        return null;
                    }
                }
            }
            catch (Exception exception2)
            {
                if (((exception2 is ThreadAbortException) || (exception2 is StackOverflowException)) || (exception2 is OutOfMemoryException))
                {
                    throw;
                }
                if (Logging.On)
                {
                    Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_exception", new object[] { "WinInetCache.Retrieve()", exception2.ToString() }));
                }
                if (Logging.On)
                {
                    Logging.Exit(Logging.RequestCache, "WinInetCache.Retrieve()");
                }
                if (handle != null)
                {
                    handle.Close();
                }
                @null.Close();
                @null = Stream.Null;
                cacheEntry = new RequestCacheEntry();
                cacheEntry.IsPrivateEntry = base.IsPrivateCache;
                if (isThrow)
                {
                    throw;
                }
                return null;
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.RequestCache, "WinInetCache.Retrieve()", "Status = " + entry.Error.ToString());
            }
            return @null;
        }

        internal override void Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (!base.CanWrite)
            {
                if (Logging.On)
                {
                    Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_operation_failed_with_error", new object[] { "WinInetCache.Remove()", SR.GetString("net_cache_access_denied", new object[] { "Write" }) }));
                }
            }
            else
            {
                _WinInetCache.Entry entry = new _WinInetCache.Entry(key, _MaximumResponseHeadersLength);
                if ((_WinInetCache.Remove(entry) != _WinInetCache.Status.Success) && (entry.Error != _WinInetCache.Status.FileNotFound))
                {
                    Win32Exception innerException = new Win32Exception((int) entry.Error);
                    if (Logging.On)
                    {
                        Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_cannot_remove", new object[] { "WinInetCache.Remove()", key, innerException.Message }));
                    }
                    throw new IOException(SR.GetString("net_cache_retrieve_failure", new object[] { innerException.Message }), innerException);
                }
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_key_status", new object[] { "WinInetCache.Remove(), ", key, entry.Error.ToString() }));
                }
            }
        }

        internal override Stream Retrieve(string key, out RequestCacheEntry cacheEntry)
        {
            return this.Lookup(key, out cacheEntry, true);
        }

        internal override Stream Store(string key, long contentLength, DateTime expiresUtc, DateTime lastModifiedUtc, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata)
        {
            return this.GetWriteStream(key, contentLength, expiresUtc, lastModifiedUtc, maxStale, entryMetadata, systemMetadata, true);
        }

        internal override bool TryRemove(string key)
        {
            return this.TryRemove(key, false);
        }

        internal bool TryRemove(string key, bool forceRemove)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (!base.CanWrite)
            {
                if (Logging.On)
                {
                    Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_operation_failed_with_error", new object[] { "WinInetCache.TryRemove()", SR.GetString("net_cache_access_denied", new object[] { "Write" }) }));
                }
                return false;
            }
            _WinInetCache.Entry entry = new _WinInetCache.Entry(key, _MaximumResponseHeadersLength);
            if ((_WinInetCache.Remove(entry) == _WinInetCache.Status.Success) || (entry.Error == _WinInetCache.Status.FileNotFound))
            {
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_key_status", new object[] { "WinInetCache.TryRemove()", key, entry.Error.ToString() }));
                }
                return true;
            }
            if (!forceRemove)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_key_remove_failed_status", new object[] { "WinInetCache.TryRemove()", key, entry.Error.ToString() }));
                }
                return false;
            }
            if (_WinInetCache.LookupInfo(entry) == _WinInetCache.Status.Success)
            {
                while (entry.Info.UseCount != 0)
                {
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_key_status", new object[] { "WinInetCache.TryRemove()", key, entry.Error.ToString() }));
                    }
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_usecount_file", new object[] { "WinInetCache.TryRemove()", entry.Info.UseCount, entry.Filename }));
                    }
                    if (!UnsafeNclNativeMethods.UnsafeWinInetCache.UnlockUrlCacheEntryFileW(key, 0))
                    {
                        break;
                    }
                    _WinInetCache.Status info = _WinInetCache.LookupInfo(entry);
                }
            }
            _WinInetCache.Remove(entry);
            if ((entry.Error != _WinInetCache.Status.Success) && (_WinInetCache.LookupInfo(entry) == _WinInetCache.Status.FileNotFound))
            {
                entry.Error = _WinInetCache.Status.Success;
            }
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_key_status", new object[] { "WinInetCache.TryRemove()", key, entry.Error.ToString() }));
            }
            return (entry.Error == _WinInetCache.Status.Success);
        }

        internal override bool TryRetrieve(string key, out RequestCacheEntry cacheEntry, out Stream readStream)
        {
            readStream = this.Lookup(key, out cacheEntry, false);
            if (readStream == null)
            {
                return false;
            }
            return true;
        }

        internal override bool TryStore(string key, long contentLength, DateTime expiresUtc, DateTime lastModifiedUtc, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata, out Stream writeStream)
        {
            writeStream = this.GetWriteStream(key, contentLength, expiresUtc, lastModifiedUtc, maxStale, entryMetadata, systemMetadata, false);
            if (writeStream == null)
            {
                return false;
            }
            return true;
        }

        internal override bool TryUpdate(string key, DateTime expiresUtc, DateTime lastModifiedUtc, DateTime lastSynchronizedUtc, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata)
        {
            return this.UpdateInfo(key, expiresUtc, lastModifiedUtc, lastSynchronizedUtc, maxStale, entryMetadata, systemMetadata, false);
        }

        internal override void UnlockEntry(Stream stream)
        {
            ReadStream stream2 = stream as ReadStream;
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_stream", new object[] { "WinInetCache.UnlockEntry", (stream == null) ? "<null>" : stream.GetType().FullName }));
            }
            if (stream2 != null)
            {
                stream2.UnlockEntry();
            }
        }

        internal override void Update(string key, DateTime expiresUtc, DateTime lastModifiedUtc, DateTime lastSynchronizedUtc, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata)
        {
            this.UpdateInfo(key, expiresUtc, lastModifiedUtc, lastSynchronizedUtc, maxStale, entryMetadata, systemMetadata, true);
        }

        private bool UpdateInfo(string key, DateTime expiresUtc, DateTime lastModifiedUtc, DateTime lastSynchronizedUtc, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata, bool isThrow)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (Logging.On)
            {
                Logging.Enter(Logging.RequestCache, "WinInetCache.Update", "Key = " + key);
            }
            if (!base.CanWrite)
            {
                if (Logging.On)
                {
                    Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_operation_failed_with_error", new object[] { "WinInetCache.Update()", SR.GetString("net_cache_access_denied", new object[] { "Write" }) }));
                }
                if (Logging.On)
                {
                    Logging.Exit(Logging.RequestCache, "WinInetCache.Update()");
                }
                if (isThrow)
                {
                    throw new InvalidOperationException(SR.GetString("net_cache_access_denied", new object[] { "Write" }));
                }
                return false;
            }
            _WinInetCache.Entry newEntry = new _WinInetCache.Entry(key, _MaximumResponseHeadersLength);
            _WinInetCache.Entry_FC none = _WinInetCache.Entry_FC.None;
            if ((expiresUtc != DateTime.MinValue) && (expiresUtc > s_MinDateTimeUtcForFileTimeUtc))
            {
                none |= _WinInetCache.Entry_FC.Exptime;
                newEntry.Info.ExpireTime = new _WinInetCache.FILETIME(expiresUtc.ToFileTimeUtc());
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_set_expires", new object[] { expiresUtc.ToString("r") }));
                }
            }
            if ((lastModifiedUtc != DateTime.MinValue) && (lastModifiedUtc > s_MinDateTimeUtcForFileTimeUtc))
            {
                none |= _WinInetCache.Entry_FC.Modtime;
                newEntry.Info.LastModifiedTime = new _WinInetCache.FILETIME(lastModifiedUtc.ToFileTimeUtc());
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_set_last_modified", new object[] { lastModifiedUtc.ToString("r") }));
                }
            }
            if ((lastSynchronizedUtc != DateTime.MinValue) && (lastSynchronizedUtc > s_MinDateTimeUtcForFileTimeUtc))
            {
                none |= _WinInetCache.Entry_FC.Synctime;
                newEntry.Info.LastSyncTime = new _WinInetCache.FILETIME(lastSynchronizedUtc.ToFileTimeUtc());
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_set_last_synchronized", new object[] { lastSynchronizedUtc.ToString("r") }));
                }
            }
            if (maxStale != TimeSpan.MinValue)
            {
                none |= _WinInetCache.Entry_FC.ExemptDelta | _WinInetCache.Entry_FC.Attribute;
                newEntry.Info.EntryType = _WinInetCache.EntryType.NormalEntry;
                if (maxStale >= TimeSpan.Zero)
                {
                    if (maxStale >= s_MaxTimeSpanForInt32)
                    {
                        maxStale = s_MaxTimeSpanForInt32;
                    }
                    newEntry.Info.EntryType = _WinInetCache.EntryType.StickyEntry;
                    newEntry.Info.U.ExemptDelta = (int) maxStale.TotalSeconds;
                    if (Logging.On)
                    {
                        object[] args = new object[] { ((int) maxStale.TotalSeconds).ToString() };
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_enable_max_stale", args));
                    }
                }
                else
                {
                    newEntry.Info.U.ExemptDelta = 0;
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_disable_max_stale"));
                    }
                }
            }
            newEntry.MetaInfo = this.CombineMetaInfo(entryMetadata, systemMetadata);
            if (newEntry.MetaInfo.Length != 0)
            {
                none |= _WinInetCache.Entry_FC.Headerinfo;
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_dumping"));
                    if (Logging.IsVerbose(Logging.RequestCache))
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_dumping"));
                        if (entryMetadata != null)
                        {
                            foreach (string str in entryMetadata)
                            {
                                Logging.PrintInfo(Logging.RequestCache, str.TrimEnd(RequestCache.LineSplits));
                            }
                        }
                        Logging.PrintInfo(Logging.RequestCache, "------");
                        if (systemMetadata != null)
                        {
                            foreach (string str2 in systemMetadata)
                            {
                                Logging.PrintInfo(Logging.RequestCache, str2.TrimEnd(RequestCache.LineSplits));
                            }
                        }
                    }
                }
            }
            _WinInetCache.Update(newEntry, none);
            if (newEntry.Error != _WinInetCache.Status.Success)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_update_failed", new object[] { "WinInetCache.Update()", newEntry.Key, new Win32Exception((int) newEntry.Error).Message }));
                    Logging.Exit(Logging.RequestCache, "WinInetCache.Update()");
                }
                if (isThrow)
                {
                    Win32Exception innerException = new Win32Exception((int) newEntry.Error);
                    throw new IOException(SR.GetString("net_cache_retrieve_failure", new object[] { innerException.Message }), innerException);
                }
                return false;
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.RequestCache, "WinInetCache.Update()", "Status = " + newEntry.Error.ToString());
            }
            return true;
        }

        private class ReadStream : FileStream, ICloseEx, IRequestLifetimeTracker
        {
            private bool m_Aborted;
            private int m_CallNesting;
            private int m_Disposed;
            private ManualResetEvent m_Event;
            private SafeUnlockUrlCacheEntryFile m_Handle;
            private string m_Key;
            private int m_ReadTimeout;
            private RequestLifetimeSetter m_RequestLifetimeSetter;
            private int m_WriteTimeout;

            [FileIOPermission(SecurityAction.Assert, Unrestricted=true)]
            internal ReadStream(_WinInetCache.Entry entry, SafeUnlockUrlCacheEntryFile handle, bool async) : base(entry.Filename, FileMode.Open, FileAccess.Read, ComNetOS.IsWinNt ? (FileShare.Delete | FileShare.Read) : FileShare.Read, 0x1000, async)
            {
                this.m_Key = entry.Key;
                this.m_Handle = handle;
                this.m_ReadTimeout = this.m_WriteTimeout = -1;
            }

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                IAsyncResult result;
                lock (this.m_Handle)
                {
                    if (this.m_CallNesting != 0)
                    {
                        throw new NotSupportedException(SR.GetString("net_no_concurrent_io_allowed"));
                    }
                    if (this.m_Aborted)
                    {
                        throw ExceptionHelper.RequestAbortedException;
                    }
                    if (this.m_Event != null)
                    {
                        throw new ObjectDisposedException(base.GetType().FullName);
                    }
                    this.m_CallNesting = 1;
                    try
                    {
                        result = base.BeginRead(buffer, offset, count, callback, state);
                    }
                    catch
                    {
                        this.m_CallNesting = 0;
                        throw;
                    }
                }
                return result;
            }

            public void CloseEx(CloseExState closeState)
            {
                if ((closeState & CloseExState.Abort) != CloseExState.Normal)
                {
                    this.m_Aborted = true;
                }
                try
                {
                    this.Close();
                }
                catch
                {
                    if ((closeState & CloseExState.Silent) == CloseExState.Normal)
                    {
                        throw;
                    }
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (Interlocked.Exchange(ref this.m_Disposed, 1) == 0)
                {
                    if (!disposing)
                    {
                        base.Dispose(false);
                    }
                    else if (this.m_Key != null)
                    {
                        try
                        {
                            lock (this.m_Handle)
                            {
                                if (this.m_CallNesting == 0)
                                {
                                    base.Dispose(true);
                                }
                                else
                                {
                                    this.m_Event = new ManualResetEvent(false);
                                }
                            }
                            RequestLifetimeSetter.Report(this.m_RequestLifetimeSetter);
                            if (this.m_Event != null)
                            {
                                using (this.m_Event)
                                {
                                    this.m_Event.WaitOne();
                                    lock (this.m_Handle)
                                    {
                                    }
                                }
                                base.Dispose(true);
                            }
                        }
                        finally
                        {
                            if (Logging.On)
                            {
                                Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_key", new object[] { "WinInetReadStream.Close()", this.m_Key }));
                            }
                            this.m_Handle.Close();
                        }
                    }
                }
            }

            public override int EndRead(IAsyncResult asyncResult)
            {
                int num;
                lock (this.m_Handle)
                {
                    try
                    {
                        num = base.EndRead(asyncResult);
                    }
                    finally
                    {
                        this.m_CallNesting = 0;
                        if (this.m_Event != null)
                        {
                            try
                            {
                                this.m_Event.Set();
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                return num;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                int num;
                lock (this.m_Handle)
                {
                    try
                    {
                        if (this.m_CallNesting != 0)
                        {
                            throw new NotSupportedException(SR.GetString("net_no_concurrent_io_allowed"));
                        }
                        if (this.m_Aborted)
                        {
                            throw ExceptionHelper.RequestAbortedException;
                        }
                        if (this.m_Event != null)
                        {
                            throw new ObjectDisposedException(base.GetType().FullName);
                        }
                        this.m_CallNesting = 1;
                        num = base.Read(buffer, offset, count);
                    }
                    finally
                    {
                        this.m_CallNesting = 0;
                        if (this.m_Event != null)
                        {
                            this.m_Event.Set();
                        }
                    }
                }
                return num;
            }

            void IRequestLifetimeTracker.TrackRequestLifetime(long requestStartTimestamp)
            {
                this.m_RequestLifetimeSetter = new RequestLifetimeSetter(requestStartTimestamp);
            }

            internal void UnlockEntry()
            {
                this.m_Handle.Close();
            }

            public override bool CanTimeout
            {
                get
                {
                    return true;
                }
            }

            public override int ReadTimeout
            {
                get
                {
                    return this.m_ReadTimeout;
                }
                set
                {
                    this.m_ReadTimeout = value;
                }
            }

            public override int WriteTimeout
            {
                get
                {
                    return this.m_WriteTimeout;
                }
                set
                {
                    this.m_WriteTimeout = value;
                }
            }
        }

        private class WriteStream : FileStream, ICloseEx
        {
            private bool m_Aborted;
            private int m_CallNesting;
            private int m_Disposed;
            private _WinInetCache.Entry m_Entry;
            private ManualResetEvent m_Event;
            private bool m_IsThrow;
            private bool m_OneWriteSucceeded;
            private int m_ReadTimeout;
            private long m_StreamSize;
            private int m_WriteTimeout;

            [FileIOPermission(SecurityAction.Assert, Unrestricted=true)]
            internal WriteStream(_WinInetCache.Entry entry, bool isThrow, long streamSize, bool async) : base(entry.Filename, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 0x1000, async)
            {
                this.m_Entry = entry;
                this.m_IsThrow = isThrow;
                this.m_StreamSize = streamSize;
                this.m_OneWriteSucceeded = streamSize == 0L;
                this.m_ReadTimeout = this.m_WriteTimeout = -1;
            }

            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                IAsyncResult result;
                lock (this.m_Entry)
                {
                    if (this.m_CallNesting != 0)
                    {
                        throw new NotSupportedException(SR.GetString("net_no_concurrent_io_allowed"));
                    }
                    if (this.m_Aborted)
                    {
                        throw ExceptionHelper.RequestAbortedException;
                    }
                    if (this.m_Event != null)
                    {
                        throw new ObjectDisposedException(base.GetType().FullName);
                    }
                    this.m_CallNesting = 1;
                    try
                    {
                        if (this.m_StreamSize > 0L)
                        {
                            this.m_StreamSize -= count;
                        }
                        result = base.BeginWrite(buffer, offset, count, callback, state);
                    }
                    catch
                    {
                        this.m_Aborted = true;
                        this.m_CallNesting = 0;
                        throw;
                    }
                }
                return result;
            }

            public void CloseEx(CloseExState closeState)
            {
                if ((closeState & CloseExState.Abort) != CloseExState.Normal)
                {
                    this.m_Aborted = true;
                }
                try
                {
                    this.Close();
                }
                catch
                {
                    if ((closeState & CloseExState.Silent) == CloseExState.Normal)
                    {
                        throw;
                    }
                }
            }

            protected override void Dispose(bool disposing)
            {
                if ((Interlocked.Exchange(ref this.m_Disposed, 1) == 0) && (this.m_Entry != null))
                {
                    System.Net.TriState unspecified;
                    lock (this.m_Entry)
                    {
                        if (this.m_CallNesting == 0)
                        {
                            base.Dispose(disposing);
                        }
                        else
                        {
                            this.m_Event = new ManualResetEvent(false);
                        }
                    }
                    if (disposing && (this.m_Event != null))
                    {
                        using (this.m_Event)
                        {
                            this.m_Event.WaitOne();
                            lock (this.m_Entry)
                            {
                            }
                        }
                        base.Dispose(disposing);
                    }
                    if (this.m_StreamSize < 0L)
                    {
                        if (this.m_Aborted)
                        {
                            if (this.m_OneWriteSucceeded)
                            {
                                unspecified = System.Net.TriState.Unspecified;
                            }
                            else
                            {
                                unspecified = System.Net.TriState.False;
                            }
                        }
                        else
                        {
                            unspecified = System.Net.TriState.True;
                        }
                    }
                    else if (!this.m_OneWriteSucceeded)
                    {
                        unspecified = System.Net.TriState.False;
                    }
                    else if (this.m_StreamSize > 0L)
                    {
                        unspecified = System.Net.TriState.Unspecified;
                    }
                    else
                    {
                        unspecified = System.Net.TriState.True;
                    }
                    if (unspecified == System.Net.TriState.False)
                    {
                        try
                        {
                            if (Logging.On)
                            {
                                Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_no_commit", new object[] { "WinInetWriteStream.Close()" }));
                            }
                            System.IO.File.Delete(this.m_Entry.Filename);
                        }
                        catch (Exception exception)
                        {
                            if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                            {
                                throw;
                            }
                            if (Logging.On)
                            {
                                Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_error_deleting_filename", new object[] { "WinInetWriteStream.Close()", this.m_Entry.Filename }));
                            }
                        }
                        finally
                        {
                            _WinInetCache.Status status = _WinInetCache.Remove(this.m_Entry);
                            if (((status != _WinInetCache.Status.Success) && (status != _WinInetCache.Status.FileNotFound)) && Logging.On)
                            {
                                Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_delete_failed", new object[] { "WinInetWriteStream.Close()", this.m_Entry.Key, new Win32Exception((int) this.m_Entry.Error).Message }));
                            }
                            this.m_Entry = null;
                        }
                    }
                    else
                    {
                        this.m_Entry.OriginalUrl = null;
                        if (unspecified == System.Net.TriState.Unspecified)
                        {
                            if (((this.m_Entry.MetaInfo == null) || (this.m_Entry.MetaInfo.Length == 0)) || ((this.m_Entry.MetaInfo != "\r\n") && (this.m_Entry.MetaInfo.IndexOf("\r\n\r\n", StringComparison.Ordinal) == -1)))
                            {
                                this.m_Entry.MetaInfo = "\r\n~SPARSE_ENTRY:\r\n";
                            }
                            else
                            {
                                this.m_Entry.MetaInfo = this.m_Entry.MetaInfo + "~SPARSE_ENTRY:\r\n";
                            }
                        }
                        if (_WinInetCache.Commit(this.m_Entry) != _WinInetCache.Status.Success)
                        {
                            if (Logging.On)
                            {
                                Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_commit_failed", new object[] { "WinInetWriteStream.Close()", this.m_Entry.Key, new Win32Exception((int) this.m_Entry.Error).Message }));
                            }
                            try
                            {
                                System.IO.File.Delete(this.m_Entry.Filename);
                            }
                            catch (Exception exception2)
                            {
                                if (((exception2 is ThreadAbortException) || (exception2 is StackOverflowException)) || (exception2 is OutOfMemoryException))
                                {
                                    throw;
                                }
                                if (Logging.On)
                                {
                                    Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_error_deleting_filename", new object[] { "WinInetWriteStream.Close()", this.m_Entry.Filename }));
                                }
                            }
                            if (this.m_IsThrow)
                            {
                                Win32Exception innerException = new Win32Exception((int) this.m_Entry.Error);
                                throw new IOException(SR.GetString("net_cache_retrieve_failure", new object[] { innerException.Message }), innerException);
                            }
                        }
                        else
                        {
                            if (Logging.On)
                            {
                                if ((this.m_StreamSize > 0L) || ((this.m_StreamSize < 0L) && this.m_Aborted))
                                {
                                    Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_committed_as_partial", new object[] { "WinInetWriteStream.Close()", this.m_Entry.Key, (this.m_StreamSize > 0L) ? this.m_StreamSize.ToString(CultureInfo.CurrentCulture) : SR.GetString("net_log_unknown") }));
                                }
                                Logging.PrintInfo(Logging.RequestCache, "WinInetWriteStream.Close(), Key = " + this.m_Entry.Key + ", Commit Status = " + this.m_Entry.Error.ToString());
                            }
                            if ((this.m_Entry.Info.EntryType & _WinInetCache.EntryType.StickyEntry) == _WinInetCache.EntryType.StickyEntry)
                            {
                                if (_WinInetCache.Update(this.m_Entry, _WinInetCache.Entry_FC.ExemptDelta) != _WinInetCache.Status.Success)
                                {
                                    if (Logging.On)
                                    {
                                        Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_update_failed", new object[] { "WinInetWriteStream.Close(), Key = " + this.m_Entry.Key, new Win32Exception((int) this.m_Entry.Error).Message }));
                                    }
                                    if (this.m_IsThrow)
                                    {
                                        Win32Exception exception4 = new Win32Exception((int) this.m_Entry.Error);
                                        throw new IOException(SR.GetString("net_cache_retrieve_failure", new object[] { exception4.Message }), exception4);
                                    }
                                    return;
                                }
                                if (Logging.On)
                                {
                                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_max_stale_and_update_status", new object[] { "WinInetWriteFile.Close()", this.m_Entry.Info.U.ExemptDelta, this.m_Entry.Error.ToString() }));
                                }
                            }
                            base.Dispose(disposing);
                        }
                    }
                }
            }

            public override void EndWrite(IAsyncResult asyncResult)
            {
                lock (this.m_Entry)
                {
                    try
                    {
                        base.EndWrite(asyncResult);
                        if (!this.m_OneWriteSucceeded)
                        {
                            this.m_OneWriteSucceeded = true;
                        }
                    }
                    catch
                    {
                        this.m_Aborted = true;
                        throw;
                    }
                    finally
                    {
                        this.m_CallNesting = 0;
                        if (this.m_Event != null)
                        {
                            try
                            {
                                this.m_Event.Set();
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                lock (this.m_Entry)
                {
                    if (this.m_Aborted)
                    {
                        throw ExceptionHelper.RequestAbortedException;
                    }
                    if (this.m_Event != null)
                    {
                        throw new ObjectDisposedException(base.GetType().FullName);
                    }
                    this.m_CallNesting = 1;
                    try
                    {
                        base.Write(buffer, offset, count);
                        if (this.m_StreamSize > 0L)
                        {
                            this.m_StreamSize -= count;
                        }
                        if (!this.m_OneWriteSucceeded && (count != 0))
                        {
                            this.m_OneWriteSucceeded = true;
                        }
                    }
                    catch
                    {
                        this.m_Aborted = true;
                        throw;
                    }
                    finally
                    {
                        this.m_CallNesting = 0;
                        if (this.m_Event != null)
                        {
                            this.m_Event.Set();
                        }
                    }
                }
            }

            public override bool CanTimeout
            {
                get
                {
                    return true;
                }
            }

            public override int ReadTimeout
            {
                get
                {
                    return this.m_ReadTimeout;
                }
                set
                {
                    this.m_ReadTimeout = value;
                }
            }

            public override int WriteTimeout
            {
                get
                {
                    return this.m_WriteTimeout;
                }
                set
                {
                    this.m_WriteTimeout = value;
                }
            }
        }
    }
}

