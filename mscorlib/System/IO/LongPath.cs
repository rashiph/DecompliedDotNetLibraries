namespace System.IO
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(false)]
    internal static class LongPath
    {
        [SecurityCritical]
        internal static string GetDirectoryName(string path)
        {
            if (path != null)
            {
                bool flag;
                string str = TryRemoveLongPathPrefix(path, out flag);
                Path.CheckInvalidPathChars(str);
                path = NormalizePath(str, false);
                int rootLength = GetRootLength(str);
                if (str.Length > rootLength)
                {
                    int length = str.Length;
                    if (length == rootLength)
                    {
                        return null;
                    }
                    while (((length > rootLength) && (str[--length] != Path.DirectorySeparatorChar)) && (str[length] != Path.AltDirectorySeparatorChar))
                    {
                    }
                    string str2 = str.Substring(0, length);
                    if (flag)
                    {
                        str2 = Path.AddLongPathPrefix(str2);
                    }
                    return str2;
                }
            }
            return null;
        }

        [SecurityCritical]
        internal static string GetPathRoot(string path)
        {
            bool flag;
            if (path == null)
            {
                return null;
            }
            string str = NormalizePath(TryRemoveLongPathPrefix(path, out flag), false);
            string str2 = path.Substring(0, GetRootLength(str));
            if (flag)
            {
                str2 = Path.AddLongPathPrefix(str2);
            }
            return str2;
        }

        internal static int GetRootLength(string path)
        {
            bool flag;
            int rootLength = Path.GetRootLength(TryRemoveLongPathPrefix(path, out flag));
            if (flag)
            {
                rootLength += 4;
            }
            return rootLength;
        }

        internal static string InternalCombine(string path1, string path2)
        {
            bool flag;
            string path = Path.InternalCombine(TryRemoveLongPathPrefix(path1, out flag), path2);
            if (flag)
            {
                path = Path.AddLongPathPrefix(path);
            }
            return path;
        }

        internal static bool IsPathRooted(string path)
        {
            return Path.IsPathRooted(Path.RemoveLongPathPrefix(path));
        }

        [SecurityCritical]
        internal static string NormalizePath(string path)
        {
            return NormalizePath(path, true);
        }

        [SecurityCritical]
        internal static string NormalizePath(string path, bool fullCheck)
        {
            return Path.NormalizePath(path, fullCheck, Path.MaxLongPath);
        }

        internal static string TryRemoveLongPathPrefix(string path, out bool removed)
        {
            removed = Path.HasLongPathPrefix(path);
            if (!removed)
            {
                return path;
            }
            return Path.RemoveLongPathPrefix(path);
        }
    }
}

