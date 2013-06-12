namespace System.IO
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    internal class FileInfoResultHandler : SearchResultHandler<FileInfo>
    {
        [SecurityCritical]
        internal override FileInfo CreateObject(SearchResult result)
        {
            string fullPath = result.FullPath;
            string[] pathList = new string[] { fullPath };
            new FileIOPermission(FileIOPermissionAccess.Read, pathList, false, false).Demand();
            FileInfo info = new FileInfo(fullPath, false);
            info.InitializeFrom(result.FindData);
            return info;
        }

        [SecurityCritical]
        internal override bool IsResultIncluded(SearchResult result)
        {
            return FileSystemEnumerableHelpers.IsFile(result.FindData);
        }
    }
}

