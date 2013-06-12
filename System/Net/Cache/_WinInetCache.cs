namespace System.Net.Cache
{
    using System;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    internal static class _WinInetCache
    {
        private const int c_CharSz = 2;

        internal static unsafe Status Commit(Entry entry)
        {
            string metaInfo = entry.MetaInfo;
            if (metaInfo == null)
            {
                metaInfo = string.Empty;
            }
            if ((((metaInfo.Length + entry.Key.Length) + entry.Filename.Length) + ((entry.OriginalUrl == null) ? 0 : entry.OriginalUrl.Length)) > (entry.MaxBufferBytes / 2))
            {
                entry.Error = Status.InsufficientBuffer;
                return entry.Error;
            }
            entry.Error = Status.Success;
            fixed (char* str2 = ((char*) metaInfo))
            {
                char* chPtr = str2;
                byte* headerInfo = (metaInfo.Length == 0) ? null : ((byte*) chPtr);
                if (!UnsafeNclNativeMethods.UnsafeWinInetCache.CommitUrlCacheEntryW(entry.Key, entry.Filename, entry.Info.ExpireTime, entry.Info.LastModifiedTime, entry.Info.EntryType, headerInfo, metaInfo.Length, null, entry.OriginalUrl))
                {
                    entry.Error = (Status) Marshal.GetLastWin32Error();
                }
            }
            return entry.Error;
        }

        internal static Status CreateFileName(Entry entry)
        {
            entry.Error = Status.Success;
            StringBuilder fileName = new StringBuilder(260);
            if (UnsafeNclNativeMethods.UnsafeWinInetCache.CreateUrlCacheEntryW(entry.Key, entry.OptionalLength, entry.FileExt, fileName, 0))
            {
                entry.Filename = fileName.ToString();
                return Status.Success;
            }
            entry.Error = (Status) Marshal.GetLastWin32Error();
            return entry.Error;
        }

        private static unsafe Status EntryFixup(Entry entry, EntryBuffer* bufferPtr, byte[] buffer)
        {
            bufferPtr._OffsetExtension = (bufferPtr._OffsetExtension == IntPtr.Zero) ? IntPtr.Zero : ((IntPtr) ((long) ((((void*) bufferPtr._OffsetExtension) - bufferPtr) / 1)));
            bufferPtr._OffsetFileName = (bufferPtr._OffsetFileName == IntPtr.Zero) ? IntPtr.Zero : ((IntPtr) ((long) ((((void*) bufferPtr._OffsetFileName) - bufferPtr) / 1)));
            bufferPtr._OffsetHeaderInfo = (bufferPtr._OffsetHeaderInfo == IntPtr.Zero) ? IntPtr.Zero : ((IntPtr) ((long) ((((void*) bufferPtr._OffsetHeaderInfo) - bufferPtr) / 1)));
            bufferPtr._OffsetSourceUrlName = (bufferPtr._OffsetSourceUrlName == IntPtr.Zero) ? IntPtr.Zero : ((IntPtr) ((long) ((((void*) bufferPtr._OffsetSourceUrlName) - bufferPtr) / 1)));
            entry.Info = bufferPtr[0];
            entry.OriginalUrl = GetEntryBufferString((void*) bufferPtr, (int) bufferPtr._OffsetSourceUrlName);
            entry.Filename = GetEntryBufferString((void*) bufferPtr, (int) bufferPtr._OffsetFileName);
            entry.FileExt = GetEntryBufferString((void*) bufferPtr, (int) bufferPtr._OffsetExtension);
            return GetEntryHeaders(entry, bufferPtr, buffer);
        }

        private static unsafe string GetEntryBufferString(void* bufferPtr, int offset)
        {
            if (offset == 0)
            {
                return null;
            }
            IntPtr ptr = new IntPtr(bufferPtr + offset);
            return Marshal.PtrToStringUni(ptr);
        }

        private static unsafe Status GetEntryHeaders(Entry entry, EntryBuffer* bufferPtr, byte[] buffer)
        {
            entry.Error = Status.Success;
            entry.MetaInfo = null;
            if (((bufferPtr._OffsetHeaderInfo == IntPtr.Zero) || (bufferPtr.HeaderInfoChars == 0)) || ((bufferPtr.EntryType & EntryType.UrlHistory) != 0))
            {
                return Status.Success;
            }
            int num = bufferPtr.HeaderInfoChars + (((int) bufferPtr._OffsetHeaderInfo) / 2);
            if ((num * 2) > entry.MaxBufferBytes)
            {
                num = entry.MaxBufferBytes / 2;
            }
            while (*(((ushort*) (bufferPtr + ((num - 1) * 2)))) == 0)
            {
                num--;
            }
            entry.MetaInfo = Encoding.Unicode.GetString(buffer, (int) bufferPtr._OffsetHeaderInfo, (num - (((int) bufferPtr._OffsetHeaderInfo) / 2)) * 2);
            return entry.Error;
        }

        internal static unsafe SafeUnlockUrlCacheEntryFile LookupFile(Entry entry)
        {
            byte[] buffer = new byte[0x800];
            int length = buffer.Length;
            SafeUnlockUrlCacheEntryFile handle = null;
            try
            {
                try
                {
                Label_0011:
                    fixed (byte* numRef = buffer)
                    {
                        entry.Error = SafeUnlockUrlCacheEntryFile.GetAndLockFile(entry.Key, numRef, ref length, out handle);
                        if (entry.Error == Status.Success)
                        {
                            entry.MaxBufferBytes = length;
                            EntryFixup(entry, (EntryBuffer*) numRef, buffer);
                            return handle;
                        }
                        if ((entry.Error == Status.InsufficientBuffer) && (length <= entry.MaxBufferBytes))
                        {
                            buffer = new byte[length];
                            goto Label_0011;
                        }
                    }
                }
                finally
                {
                    numRef = null;
                }
            }
            catch (Exception exception)
            {
                if (handle != null)
                {
                    handle.Close();
                }
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (entry.Error == Status.Success)
                {
                    entry.Error = Status.InternalError;
                }
            }
            return null;
        }

        internal static unsafe Status LookupInfo(Entry entry)
        {
            byte[] buffer = new byte[0x800];
            int length = buffer.Length;
            byte[] buffer2 = buffer;
            for (int i = 0; i < 0x40; i++)
            {
                try
                {
                    fixed (byte* numRef = buffer2)
                    {
                        if (UnsafeNclNativeMethods.UnsafeWinInetCache.GetUrlCacheEntryInfoW(entry.Key, numRef, ref length))
                        {
                            buffer = buffer2;
                            entry.MaxBufferBytes = length;
                            EntryFixup(entry, (EntryBuffer*) numRef, buffer2);
                            entry.Error = Status.Success;
                            return entry.Error;
                        }
                        entry.Error = (Status) Marshal.GetLastWin32Error();
                        if (((entry.Error == Status.InsufficientBuffer) && (buffer2 == buffer)) && (length <= entry.MaxBufferBytes))
                        {
                            buffer2 = new byte[length];
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                finally
                {
                    numRef = null;
                }
            }
            return entry.Error;
        }

        internal static Status Remove(Entry entry)
        {
            entry.Error = Status.Success;
            if (!UnsafeNclNativeMethods.UnsafeWinInetCache.DeleteUrlCacheEntryW(entry.Key))
            {
                entry.Error = (Status) Marshal.GetLastWin32Error();
            }
            return entry.Error;
        }

        internal static unsafe Status Update(Entry newEntry, Entry_FC attributes)
        {
            byte[] buffer = new byte[EntryBuffer.MarshalSize];
            newEntry.Error = Status.Success;
            fixed (byte* numRef = buffer)
            {
                EntryBuffer* bufferPtr = (EntryBuffer*) numRef;
                bufferPtr[0] = newEntry.Info;
                bufferPtr->StructSize = EntryBuffer.MarshalSize;
                if ((attributes & Entry_FC.Headerinfo) == Entry_FC.None)
                {
                    if (!UnsafeNclNativeMethods.UnsafeWinInetCache.SetUrlCacheEntryInfoW(newEntry.Key, numRef, attributes))
                    {
                        newEntry.Error = (Status) Marshal.GetLastWin32Error();
                    }
                }
                else
                {
                    Entry entry = new Entry(newEntry.Key, newEntry.MaxBufferBytes);
                    SafeUnlockUrlCacheEntryFile file = null;
                    bool flag = false;
                    try
                    {
                        file = LookupFile(entry);
                        if (file == null)
                        {
                            newEntry.Error = entry.Error;
                            return newEntry.Error;
                        }
                        newEntry.Filename = entry.Filename;
                        newEntry.OriginalUrl = entry.OriginalUrl;
                        newEntry.FileExt = entry.FileExt;
                        attributes &= ~Entry_FC.Headerinfo;
                        if ((attributes & Entry_FC.Exptime) == Entry_FC.None)
                        {
                            newEntry.Info.ExpireTime = entry.Info.ExpireTime;
                        }
                        if ((attributes & Entry_FC.Modtime) == Entry_FC.None)
                        {
                            newEntry.Info.LastModifiedTime = entry.Info.LastModifiedTime;
                        }
                        if ((attributes & Entry_FC.Attribute) == Entry_FC.None)
                        {
                            newEntry.Info.EntryType = entry.Info.EntryType;
                            newEntry.Info.U.ExemptDelta = entry.Info.U.ExemptDelta;
                            if ((entry.Info.EntryType & EntryType.StickyEntry) == EntryType.StickyEntry)
                            {
                                attributes |= Entry_FC.ExemptDelta | Entry_FC.Attribute;
                            }
                        }
                        attributes &= ~(Entry_FC.Exptime | Entry_FC.Modtime);
                        flag = (entry.Info.EntryType & EntryType.Edited) != 0;
                        if (!flag)
                        {
                            entry.Info.EntryType |= EntryType.Edited;
                            if (Update(entry, Entry_FC.Attribute) != Status.Success)
                            {
                                newEntry.Error = entry.Error;
                                return newEntry.Error;
                            }
                        }
                    }
                    finally
                    {
                        if (file != null)
                        {
                            file.Close();
                        }
                    }
                    Remove(entry);
                    if (Commit(newEntry) != Status.Success)
                    {
                        if (!flag)
                        {
                            entry.Info.EntryType &= ~EntryType.Edited;
                            Update(entry, Entry_FC.Attribute);
                        }
                        return newEntry.Error;
                    }
                    if (attributes != Entry_FC.None)
                    {
                        Update(newEntry, attributes);
                    }
                }
            }
            return newEntry.Error;
        }

        internal class Entry
        {
            public const int DefaultBufferSize = 0x800;
            public _WinInetCache.Status Error;
            public string FileExt;
            public string Filename;
            public _WinInetCache.EntryBuffer Info;
            public string Key;
            public int MaxBufferBytes;
            public string MetaInfo;
            public int OptionalLength;
            public string OriginalUrl;

            public Entry(string key, int maxHeadersSize)
            {
                this.Key = key;
                this.MaxBufferBytes = maxHeadersSize;
                if ((maxHeadersSize != 0x7fffffff) && ((0x7fffffff - (((key.Length + _WinInetCache.EntryBuffer.MarshalSize) + 0x400) * 2)) > maxHeadersSize))
                {
                    this.MaxBufferBytes += ((key.Length + _WinInetCache.EntryBuffer.MarshalSize) + 0x400) * 2;
                }
                this.Info.EntryType = _WinInetCache.EntryType.NormalEntry;
            }
        }

        [Flags]
        internal enum Entry_FC
        {
            Acctime = 0x100,
            Attribute = 4,
            ExemptDelta = 0x800,
            Exptime = 0x80,
            Headerinfo = 0x400,
            Hitrate = 0x10,
            Modtime = 0x40,
            None = 0,
            Synctime = 0x200
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        internal struct EntryBuffer
        {
            public static int MarshalSize;
            public int StructSize;
            public IntPtr _OffsetSourceUrlName;
            public IntPtr _OffsetFileName;
            public System.Net.Cache._WinInetCache.EntryType EntryType;
            public int UseCount;
            public int HitRate;
            public int SizeLow;
            public int SizeHigh;
            public _WinInetCache.FILETIME LastModifiedTime;
            public _WinInetCache.FILETIME ExpireTime;
            public _WinInetCache.FILETIME LastAccessTime;
            public _WinInetCache.FILETIME LastSyncTime;
            public IntPtr _OffsetHeaderInfo;
            public int HeaderInfoChars;
            public IntPtr _OffsetExtension;
            public Rsv U;
            static EntryBuffer()
            {
                MarshalSize = Marshal.SizeOf(typeof(_WinInetCache.EntryBuffer));
            }
            [StructLayout(LayoutKind.Explicit)]
            public struct Rsv
            {
                [FieldOffset(0)]
                public int ExemptDelta;
                [FieldOffset(0)]
                public int Reserved;
            }
        }

        [Flags]
        internal enum EntryType
        {
            Cookie = 0x100000,
            Edited = 8,
            NormalEntry = 0x41,
            Sparse = 0x10000,
            StickyEntry = 0x44,
            TrackOffline = 0x10,
            TrackOnline = 0x20,
            UrlHistory = 0x200000
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        internal struct FILETIME
        {
            public uint Low;
            public uint High;
            public static readonly _WinInetCache.FILETIME Zero;
            public FILETIME(long time)
            {
                this.Low = (uint) time;
                this.High = (uint) (time >> 0x20);
            }

            public long ToLong()
            {
                return (long) ((this.High << 0x20) | this.Low);
            }

            public bool IsNull
            {
                get
                {
                    return ((this.Low == 0) && (this.High == 0));
                }
            }
            static FILETIME()
            {
                Zero = new _WinInetCache.FILETIME(0L);
            }
        }

        internal enum Status
        {
            CorruptedHeaders = 0x1001001,
            FatalErrors = 0x1001000,
            FileNotFound = 2,
            InsufficientBuffer = 0x7a,
            InternalError = 0x1001002,
            InvalidParameter = 0x57,
            NoMoreItems = 0x103,
            NotEnoughStorage = 8,
            SharingViolation = 0x20,
            Success = 0,
            Warnings = 0x1000000
        }
    }
}

