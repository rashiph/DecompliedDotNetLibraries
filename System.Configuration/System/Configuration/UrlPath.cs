namespace System.Configuration
{
    using System;
    using System.IO;

    internal static class UrlPath
    {
        private const string FILE_URL_LOCAL = "file:///";
        private const string FILE_URL_UNC = "file:";

        internal static string ConvertFileNameToUrl(string fileName)
        {
            string str;
            if (IsAbsoluteLocalPhysicalPath(fileName))
            {
                str = "file:///";
            }
            else
            {
                if (!IsAbsoluteUNCPhysicalPath(fileName))
                {
                    throw ExceptionUtil.ParameterInvalid("filename");
                }
                str = "file:";
            }
            return (str + fileName.Replace('\\', '/'));
        }

        internal static string GetDirectoryOrRootName(string path)
        {
            string directoryName = Path.GetDirectoryName(path);
            if (directoryName == null)
            {
                directoryName = Path.GetPathRoot(path);
            }
            return directoryName;
        }

        private static bool IsAbsoluteLocalPhysicalPath(string path)
        {
            if ((path == null) || (path.Length < 3))
            {
                return false;
            }
            return ((path[1] == ':') && IsDirectorySeparatorChar(path[2]));
        }

        private static bool IsAbsoluteUNCPhysicalPath(string path)
        {
            if ((path == null) || (path.Length < 3))
            {
                return false;
            }
            return (IsDirectorySeparatorChar(path[0]) && IsDirectorySeparatorChar(path[1]));
        }

        private static bool IsDirectorySeparatorChar(char ch)
        {
            if (ch != '\\')
            {
                return (ch == '/');
            }
            return true;
        }

        internal static bool IsEqualOrSubdirectory(string dir, string subdir)
        {
            if (!string.IsNullOrEmpty(dir))
            {
                if (string.IsNullOrEmpty(subdir))
                {
                    return false;
                }
                int length = dir.Length;
                if (dir[length - 1] == '\\')
                {
                    length--;
                }
                int num2 = subdir.Length;
                if (subdir[num2 - 1] == '\\')
                {
                    num2--;
                }
                if (num2 < length)
                {
                    return false;
                }
                if (string.Compare(dir, 0, subdir, 0, length, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return false;
                }
                if ((num2 > length) && (subdir[length] != '\\'))
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool IsEqualOrSubpath(string path, string subpath)
        {
            return IsEqualOrSubpathImpl(path, subpath, false);
        }

        private static bool IsEqualOrSubpathImpl(string path, string subpath, bool excludeEqual)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (string.IsNullOrEmpty(subpath))
                {
                    return false;
                }
                int length = path.Length;
                if (path[length - 1] == '/')
                {
                    length--;
                }
                int num2 = subpath.Length;
                if (subpath[num2 - 1] == '/')
                {
                    num2--;
                }
                if (num2 < length)
                {
                    return false;
                }
                if (excludeEqual && (num2 == length))
                {
                    return false;
                }
                if (string.Compare(path, 0, subpath, 0, length, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return false;
                }
                if ((num2 > length) && (subpath[length] != '/'))
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool IsSubpath(string path, string subpath)
        {
            return IsEqualOrSubpathImpl(path, subpath, true);
        }
    }
}

