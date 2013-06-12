namespace System.IO
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    internal class FileSystemEnumerableIterator<TSource> : Iterator<TSource>
    {
        [SecurityCritical]
        private SafeFindHandle _hnd;
        private SearchResultHandler<TSource> _resultHandler;
        private bool empty;
        private string fullPath;
        private bool needsParentPathDiscoveryDemand;
        private string normalizedSearchPath;
        private int oldMode;
        private string searchCriteria;
        private Directory.SearchData searchData;
        private SearchOption searchOption;
        private List<Directory.SearchData> searchStack;
        private const int STATE_FIND_NEXT_FILE = 3;
        private const int STATE_FINISH = 4;
        private const int STATE_INIT = 1;
        private const int STATE_SEARCH_NEXT_DIR = 2;
        private string userPath;

        [SecuritySafeCritical]
        internal FileSystemEnumerableIterator(string path, string originalUserPath, string searchPattern, SearchOption searchOption, SearchResultHandler<TSource> resultHandler)
        {
            this.oldMode = Win32Native.SetErrorMode(1);
            this.searchStack = new List<Directory.SearchData>();
            string str = FileSystemEnumerableIterator<TSource>.NormalizeSearchPattern(searchPattern);
            if (str.Length == 0)
            {
                this.empty = true;
            }
            else
            {
                this._resultHandler = resultHandler;
                this.searchOption = searchOption;
                this.fullPath = Path.GetFullPathInternal(path);
                string fullSearchString = FileSystemEnumerableIterator<TSource>.GetFullSearchString(this.fullPath, str);
                this.normalizedSearchPath = Path.GetDirectoryName(fullSearchString);
                string[] pathList = new string[] { Directory.GetDemandDir(this.fullPath, true), Directory.GetDemandDir(this.normalizedSearchPath, true) };
                new FileIOPermission(FileIOPermissionAccess.PathDiscovery, pathList, false, false).Demand();
                this.searchCriteria = FileSystemEnumerableIterator<TSource>.GetNormalizedSearchCriteria(fullSearchString, this.normalizedSearchPath);
                string directoryName = Path.GetDirectoryName(str);
                string str4 = originalUserPath;
                if ((directoryName != null) && (directoryName.Length != 0))
                {
                    str4 = Path.Combine(str4, directoryName);
                }
                this.userPath = str4;
                this.searchData = new Directory.SearchData(this.normalizedSearchPath, this.userPath, searchOption);
                this.CommonInit();
            }
        }

        [SecuritySafeCritical]
        private FileSystemEnumerableIterator(string fullPath, string normalizedSearchPath, string searchCriteria, string userPath, SearchOption searchOption, SearchResultHandler<TSource> resultHandler)
        {
            this.fullPath = fullPath;
            this.normalizedSearchPath = normalizedSearchPath;
            this.searchCriteria = searchCriteria;
            this._resultHandler = resultHandler;
            this.userPath = userPath;
            this.searchOption = searchOption;
            this.searchStack = new List<Directory.SearchData>();
            if (searchCriteria != null)
            {
                string[] pathList = new string[] { Directory.GetDemandDir(fullPath, true), Directory.GetDemandDir(normalizedSearchPath, true) };
                new FileIOPermission(FileIOPermissionAccess.PathDiscovery, pathList, false, false).Demand();
                this.searchData = new Directory.SearchData(normalizedSearchPath, userPath, searchOption);
                this.CommonInit();
            }
            else
            {
                this.empty = true;
            }
        }

        [SecurityCritical]
        private void AddSearchableDirsToStack(Directory.SearchData localSearchData)
        {
            string fileName = localSearchData.fullPath + "*";
            SafeFindHandle hndFindFile = null;
            Win32Native.WIN32_FIND_DATA win_find_data = new Win32Native.WIN32_FIND_DATA();
            using (hndFindFile = Win32Native.FindFirstFile(fileName, win_find_data))
            {
                if (hndFindFile.IsInvalid)
                {
                    int hr = Marshal.GetLastWin32Error();
                    switch (hr)
                    {
                        case 2:
                        case 0x12:
                        case 3:
                            return;
                    }
                    this.HandleError(hr, localSearchData.fullPath);
                }
                int num2 = 0;
                do
                {
                    if (FileSystemEnumerableHelpers.IsDir(win_find_data))
                    {
                        StringBuilder builder = new StringBuilder(localSearchData.fullPath);
                        builder.Append(win_find_data.cFileName);
                        string fullPath = builder.ToString();
                        builder.Length = 0;
                        builder.Append(localSearchData.userPath);
                        builder.Append(win_find_data.cFileName);
                        SearchOption searchOption = localSearchData.searchOption;
                        Directory.SearchData item = new Directory.SearchData(fullPath, builder.ToString(), searchOption);
                        this.searchStack.Insert(num2++, item);
                    }
                }
                while (Win32Native.FindNextFile(hndFindFile, win_find_data));
            }
        }

        [SecuritySafeCritical]
        protected override Iterator<TSource> Clone()
        {
            return new FileSystemEnumerableIterator<TSource>(this.fullPath, this.normalizedSearchPath, this.searchCriteria, this.userPath, this.searchOption, this._resultHandler);
        }

        [SecurityCritical]
        private void CommonInit()
        {
            string fileName = this.searchData.fullPath + this.searchCriteria;
            Win32Native.WIN32_FIND_DATA data = new Win32Native.WIN32_FIND_DATA();
            this._hnd = Win32Native.FindFirstFile(fileName, data);
            if (this._hnd.IsInvalid)
            {
                int hr = Marshal.GetLastWin32Error();
                if ((hr != 2) && (hr != 0x12))
                {
                    this.HandleError(hr, this.searchData.fullPath);
                }
                else
                {
                    this.empty = this.searchData.searchOption == SearchOption.TopDirectoryOnly;
                }
            }
            if (this.searchData.searchOption == SearchOption.TopDirectoryOnly)
            {
                if (this.empty)
                {
                    this._hnd.Dispose();
                }
                else
                {
                    SearchResult result = this.CreateSearchResult(this.searchData, data);
                    if (this._resultHandler.IsResultIncluded(result))
                    {
                        base.current = this._resultHandler.CreateObject(result);
                    }
                }
            }
            else
            {
                this._hnd.Dispose();
                this.searchStack.Add(this.searchData);
            }
        }

        [SecurityCritical]
        private SearchResult CreateSearchResult(Directory.SearchData localSearchData, Win32Native.WIN32_FIND_DATA findData)
        {
            return new SearchResult(Path.InternalCombine(localSearchData.fullPath, findData.cFileName), Path.InternalCombine(localSearchData.userPath, findData.cFileName), findData);
        }

        [SecuritySafeCritical]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (this._hnd != null)
                {
                    this._hnd.Dispose();
                }
            }
            finally
            {
                Win32Native.SetErrorMode(this.oldMode);
                base.Dispose(disposing);
            }
        }

        [SecurityCritical]
        internal static void DoDemand(string fullPath)
        {
            string[] pathList = new string[] { Directory.GetDemandDir(fullPath, true) };
            new FileIOPermission(FileIOPermissionAccess.PathDiscovery, pathList, false, false).Demand();
        }

        private static string GetFullSearchString(string fullPath, string searchPattern)
        {
            string str = Path.InternalCombine(fullPath, searchPattern);
            char c = str[str.Length - 1];
            if (!Path.IsDirectorySeparator(c) && (c != Path.VolumeSeparatorChar))
            {
                return str;
            }
            return (str + '*');
        }

        private static string GetNormalizedSearchCriteria(string fullSearchString, string fullPathMod)
        {
            char c = fullPathMod[fullPathMod.Length - 1];
            if (Path.IsDirectorySeparator(c))
            {
                return fullSearchString.Substring(fullPathMod.Length);
            }
            return fullSearchString.Substring(fullPathMod.Length + 1);
        }

        [SecurityCritical]
        private void HandleError(int hr, string path)
        {
            base.Dispose();
            __Error.WinIOError(hr, path);
        }

        [SecuritySafeCritical]
        public override bool MoveNext()
        {
            Win32Native.WIN32_FIND_DATA data = new Win32Native.WIN32_FIND_DATA();
            switch (base.state)
            {
                case 1:
                    if (!this.empty)
                    {
                        if (this.searchData.searchOption == SearchOption.TopDirectoryOnly)
                        {
                            base.state = 3;
                            if (base.current != null)
                            {
                                return true;
                            }
                            goto Label_017A;
                        }
                        base.state = 2;
                        break;
                    }
                    base.state = 4;
                    goto Label_0250;

                case 2:
                    break;

                case 3:
                    goto Label_017A;

                case 4:
                    goto Label_0250;

                default:
                    goto Label_0256;
            }
        Label_015D:
            while (this.searchStack.Count > 0)
            {
                this.searchData = this.searchStack[0];
                this.searchStack.RemoveAt(0);
                this.AddSearchableDirsToStack(this.searchData);
                string fileName = this.searchData.fullPath + this.searchCriteria;
                this._hnd = Win32Native.FindFirstFile(fileName, data);
                if (this._hnd.IsInvalid)
                {
                    int hr = Marshal.GetLastWin32Error();
                    switch (hr)
                    {
                        case 2:
                        case 0x12:
                        case 3:
                        {
                            continue;
                        }
                    }
                    this._hnd.Dispose();
                    this.HandleError(hr, this.searchData.fullPath);
                }
                base.state = 3;
                this.needsParentPathDiscoveryDemand = true;
                SearchResult result = this.CreateSearchResult(this.searchData, data);
                if (!this._resultHandler.IsResultIncluded(result))
                {
                    goto Label_017A;
                }
                if (this.needsParentPathDiscoveryDemand)
                {
                    FileSystemEnumerableIterator<TSource>.DoDemand(this.searchData.fullPath);
                    this.needsParentPathDiscoveryDemand = false;
                }
                base.current = this._resultHandler.CreateObject(result);
                return true;
            }
            base.state = 4;
            goto Label_0250;
        Label_017A:
            if ((this.searchData != null) && (this._hnd != null))
            {
                while (Win32Native.FindNextFile(this._hnd, data))
                {
                    SearchResult result2 = this.CreateSearchResult(this.searchData, data);
                    if (this._resultHandler.IsResultIncluded(result2))
                    {
                        if (this.needsParentPathDiscoveryDemand)
                        {
                            FileSystemEnumerableIterator<TSource>.DoDemand(this.searchData.fullPath);
                            this.needsParentPathDiscoveryDemand = false;
                        }
                        base.current = this._resultHandler.CreateObject(result2);
                        return true;
                    }
                }
                int num2 = Marshal.GetLastWin32Error();
                if (this._hnd != null)
                {
                    this._hnd.Dispose();
                }
                if (((num2 != 0) && (num2 != 0x12)) && (num2 != 2))
                {
                    this.HandleError(num2, this.searchData.fullPath);
                }
            }
            if (this.searchData.searchOption == SearchOption.TopDirectoryOnly)
            {
                base.state = 4;
            }
            else
            {
                base.state = 2;
                goto Label_015D;
            }
        Label_0250:
            base.Dispose();
        Label_0256:
            return false;
        }

        private static string NormalizeSearchPattern(string searchPattern)
        {
            string str = searchPattern.TrimEnd(Path.TrimEndChars);
            if (str.Equals("."))
            {
                str = "*";
            }
            Path.CheckSearchPattern(str);
            return str;
        }
    }
}

