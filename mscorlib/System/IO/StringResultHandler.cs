namespace System.IO
{
    using System;
    using System.Security;

    internal class StringResultHandler : SearchResultHandler<string>
    {
        private bool _includeDirs;
        private bool _includeFiles;

        internal StringResultHandler(bool includeFiles, bool includeDirs)
        {
            this._includeFiles = includeFiles;
            this._includeDirs = includeDirs;
        }

        [SecurityCritical]
        internal override string CreateObject(SearchResult result)
        {
            return result.UserPath;
        }

        [SecurityCritical]
        internal override bool IsResultIncluded(SearchResult result)
        {
            bool flag = this._includeFiles && FileSystemEnumerableHelpers.IsFile(result.FindData);
            bool flag2 = this._includeDirs && FileSystemEnumerableHelpers.IsDir(result.FindData);
            if (!flag)
            {
                return flag2;
            }
            return true;
        }
    }
}

