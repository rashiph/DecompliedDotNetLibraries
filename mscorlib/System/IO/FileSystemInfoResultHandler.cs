namespace System.IO
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    internal class FileSystemInfoResultHandler : SearchResultHandler<FileSystemInfo>
    {
        [SecurityCritical]
        internal override FileSystemInfo CreateObject(SearchResult result)
        {
            FileSystemEnumerableHelpers.IsFile(result.FindData);
            if (FileSystemEnumerableHelpers.IsDir(result.FindData))
            {
                string str = result.FullPath;
                string str2 = str + @"\.";
                string[] strArray = new string[] { str2 };
                new FileIOPermission(FileIOPermissionAccess.Read, strArray, false, false).Demand();
                DirectoryInfo info = new DirectoryInfo(str, false);
                info.InitializeFrom(result.FindData);
                return info;
            }
            string fullPath = result.FullPath;
            string[] pathList = new string[] { fullPath };
            new FileIOPermission(FileIOPermissionAccess.Read, pathList, false, false).Demand();
            FileInfo info2 = new FileInfo(fullPath, false);
            info2.InitializeFrom(result.FindData);
            return info2;
        }

        [SecurityCritical]
        internal override bool IsResultIncluded(SearchResult result)
        {
            bool flag = FileSystemEnumerableHelpers.IsFile(result.FindData);
            if (!FileSystemEnumerableHelpers.IsDir(result.FindData))
            {
                return flag;
            }
            return true;
        }
    }
}

