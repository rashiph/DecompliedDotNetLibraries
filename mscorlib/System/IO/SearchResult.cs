namespace System.IO
{
    using Microsoft.Win32;
    using System;
    using System.Security;

    internal sealed class SearchResult
    {
        [SecurityCritical]
        private Win32Native.WIN32_FIND_DATA findData;
        private string fullPath;
        private string userPath;

        [SecurityCritical]
        internal SearchResult(string fullPath, string userPath, Win32Native.WIN32_FIND_DATA findData)
        {
            this.fullPath = fullPath;
            this.userPath = userPath;
            this.findData = findData;
        }

        internal Win32Native.WIN32_FIND_DATA FindData
        {
            [SecurityCritical]
            get
            {
                return this.findData;
            }
        }

        internal string FullPath
        {
            get
            {
                return this.fullPath;
            }
        }

        internal string UserPath
        {
            get
            {
                return this.userPath;
            }
        }
    }
}

