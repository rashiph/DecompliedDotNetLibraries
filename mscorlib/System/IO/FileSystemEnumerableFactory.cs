namespace System.IO
{
    using System;
    using System.Collections.Generic;
    using System.Security;

    internal static class FileSystemEnumerableFactory
    {
        [SecuritySafeCritical]
        internal static IEnumerable<DirectoryInfo> CreateDirectoryInfoIterator(string path, string originalUserPath, string searchPattern, SearchOption searchOption)
        {
            return new FileSystemEnumerableIterator<DirectoryInfo>(path, originalUserPath, searchPattern, searchOption, new DirectoryInfoResultHandler());
        }

        [SecuritySafeCritical]
        internal static IEnumerable<FileInfo> CreateFileInfoIterator(string path, string originalUserPath, string searchPattern, SearchOption searchOption)
        {
            return new FileSystemEnumerableIterator<FileInfo>(path, originalUserPath, searchPattern, searchOption, new FileInfoResultHandler());
        }

        [SecuritySafeCritical]
        internal static IEnumerable<string> CreateFileNameIterator(string path, string originalUserPath, string searchPattern, bool includeFiles, bool includeDirs, SearchOption searchOption)
        {
            return new FileSystemEnumerableIterator<string>(path, originalUserPath, searchPattern, searchOption, new StringResultHandler(includeFiles, includeDirs));
        }

        [SecuritySafeCritical]
        internal static IEnumerable<FileSystemInfo> CreateFileSystemInfoIterator(string path, string originalUserPath, string searchPattern, SearchOption searchOption)
        {
            return new FileSystemEnumerableIterator<FileSystemInfo>(path, originalUserPath, searchPattern, searchOption, new FileSystemInfoResultHandler());
        }
    }
}

