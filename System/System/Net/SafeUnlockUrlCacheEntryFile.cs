namespace System.Net
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Net.Cache;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class SafeUnlockUrlCacheEntryFile : SafeHandleZeroOrMinusOneIsInvalid
    {
        private string m_KeyString;

        private SafeUnlockUrlCacheEntryFile(string keyString) : base(true)
        {
            this.m_KeyString = keyString;
        }

        internal static unsafe _WinInetCache.Status GetAndLockFile(string key, byte* entryPtr, ref int entryBufSize, out SafeUnlockUrlCacheEntryFile handle)
        {
            if (ValidationHelper.IsBlankString(key))
            {
                throw new ArgumentNullException("key");
            }
            handle = new SafeUnlockUrlCacheEntryFile(key);
            fixed (char* str = ((char*) key))
            {
                char* chPtr = str;
                return MustRunGetAndLockFile(chPtr, entryPtr, ref entryBufSize, handle);
            }
        }

        private static unsafe _WinInetCache.Status MustRunGetAndLockFile(char* key, byte* entryPtr, ref int entryBufSize, SafeUnlockUrlCacheEntryFile handle)
        {
            _WinInetCache.Status success = _WinInetCache.Status.Success;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                if (!UnsafeNclNativeMethods.SafeNetHandles.RetrieveUrlCacheEntryFileW(key, entryPtr, ref entryBufSize, 0))
                {
                    success = (_WinInetCache.Status) Marshal.GetLastWin32Error();
                    handle.SetHandleAsInvalid();
                }
                else
                {
                    handle.SetHandle((IntPtr) 1);
                }
            }
            return success;
        }

        protected override unsafe bool ReleaseHandle()
        {
            fixed (char* str = ((char*) this.m_KeyString))
            {
                char* urlName = str;
                UnsafeNclNativeMethods.SafeNetHandles.UnlockUrlCacheEntryFileW(urlName, 0);
            }
            base.SetHandle(IntPtr.Zero);
            this.m_KeyString = null;
            return true;
        }
    }
}

