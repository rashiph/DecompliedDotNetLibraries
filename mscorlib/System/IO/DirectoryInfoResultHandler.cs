namespace System.IO
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    internal class DirectoryInfoResultHandler : SearchResultHandler<DirectoryInfo>
    {
        [SecurityCritical]
        internal override DirectoryInfo CreateObject(SearchResult result)
        {
            string fullPath = result.FullPath;
            string str2 = fullPath + @"\.";
            string[] pathList = new string[] { str2 };
            new FileIOPermission(FileIOPermissionAccess.Read, pathList, false, false).Demand();
            DirectoryInfo info = new DirectoryInfo(fullPath, false);
            info.InitializeFrom(result.FindData);
            return info;
        }

        [SecurityCritical]
        internal override bool IsResultIncluded(SearchResult result)
        {
            return FileSystemEnumerableHelpers.IsDir(result.FindData);
        }
    }
}

