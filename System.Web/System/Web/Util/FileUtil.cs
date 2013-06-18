namespace System.Web.Util
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Web;

    internal class FileUtil
    {
        private static char[] _invalidFileNameChars = Path.GetInvalidFileNameChars();
        private static int _maxPathLength = 0x103;

        private FileUtil()
        {
        }

        internal static void CheckSuspiciousPhysicalPath(string physicalPath)
        {
            if (IsSuspiciousPhysicalPath(physicalPath))
            {
                throw new HttpException(0x194, string.Empty);
            }
        }

        internal static bool DirectoryAccessible(string dirname)
        {
            bool exists = false;
            dirname = RemoveTrailingDirectoryBackSlash(dirname);
            if (HasInvalidLastChar(dirname))
            {
                return false;
            }
            try
            {
                exists = new DirectoryInfo(dirname).Exists;
            }
            catch
            {
            }
            return exists;
        }

        internal static bool DirectoryExists(string dirname)
        {
            bool flag = false;
            dirname = RemoveTrailingDirectoryBackSlash(dirname);
            if (HasInvalidLastChar(dirname))
            {
                return false;
            }
            try
            {
                flag = Directory.Exists(dirname);
            }
            catch
            {
            }
            return flag;
        }

        internal static bool DirectoryExists(string filename, bool trueOnError)
        {
            System.Web.UnsafeNativeMethods.WIN32_FILE_ATTRIBUTE_DATA win_file_attribute_data;
            filename = RemoveTrailingDirectoryBackSlash(filename);
            if (HasInvalidLastChar(filename))
            {
                return false;
            }
            if (System.Web.UnsafeNativeMethods.GetFileAttributesEx(filename, 0, out win_file_attribute_data))
            {
                return ((win_file_attribute_data.fileAttributes & 0x10) == 0x10);
            }
            if (!trueOnError)
            {
                return false;
            }
            int num = Marshal.GetHRForLastWin32Error();
            return ((num != -2147024894) && (num != -2147024893));
        }

        [FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.Read)]
        internal static bool FileExists(string filename)
        {
            bool flag = false;
            try
            {
                flag = File.Exists(filename);
            }
            catch
            {
            }
            return flag;
        }

        internal static string FixUpPhysicalDirectory(string dir)
        {
            if (dir == null)
            {
                return null;
            }
            dir = Path.GetFullPath(dir);
            if (!StringUtil.StringEndsWith(dir, @"\"))
            {
                dir = dir + @"\";
            }
            return dir;
        }

        internal static string GetFirstExistingDirectory(string appRoot, string fileName)
        {
            if (!IsBeneathAppRoot(appRoot, fileName))
            {
                return null;
            }
            string str = appRoot;
            while (true)
            {
                int index = fileName.IndexOf(Path.DirectorySeparatorChar, str.Length + 1);
                if (index <= -1)
                {
                    return str;
                }
                string filename = fileName.Substring(0, index);
                if (!DirectoryExists(filename, false))
                {
                    return str;
                }
                str = filename;
            }
        }

        private static bool HasInvalidLastChar(string physicalPath)
        {
            if (string.IsNullOrEmpty(physicalPath))
            {
                return false;
            }
            char ch = physicalPath[physicalPath.Length - 1];
            if (ch != ' ')
            {
                return (ch == '.');
            }
            return true;
        }

        internal static bool IsBeneathAppRoot(string appRoot, string filePath)
        {
            return (((filePath.Length > (appRoot.Length + 1)) && (filePath.IndexOf(appRoot, StringComparison.OrdinalIgnoreCase) > -1)) && (filePath[appRoot.Length] == Path.DirectorySeparatorChar));
        }

        internal static bool IsSuspiciousPhysicalPath(string physicalPath)
        {
            bool flag;
            if (!IsSuspiciousPhysicalPath(physicalPath, out flag))
            {
                return false;
            }
            if (flag)
            {
                if (physicalPath.IndexOf('/') >= 0)
                {
                    return true;
                }
                string str = @"\..";
                int index = physicalPath.IndexOf(str, StringComparison.Ordinal);
                if ((index >= 0) && ((physicalPath.Length == (index + str.Length)) || (physicalPath[index + str.Length] == '\\')))
                {
                    return true;
                }
                for (int i = physicalPath.LastIndexOf('\\'); i >= 0; i = physicalPath.LastIndexOf('\\', i - 1))
                {
                    if (!IsSuspiciousPhysicalPath(physicalPath.Substring(0, i), out flag))
                    {
                        return false;
                    }
                    if (!flag)
                    {
                        return true;
                    }
                }
            }
            return true;
        }

        [FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery)]
        internal static bool IsSuspiciousPhysicalPath(string physicalPath, out bool pathTooLong)
        {
            bool flag;
            try
            {
                flag = !string.IsNullOrEmpty(physicalPath) && (string.Compare(physicalPath, Path.GetFullPath(physicalPath), StringComparison.OrdinalIgnoreCase) != 0);
                pathTooLong = false;
            }
            catch (PathTooLongException)
            {
                flag = true;
                pathTooLong = true;
            }
            catch (NotSupportedException)
            {
                flag = true;
                pathTooLong = true;
            }
            catch (ArgumentException)
            {
                flag = true;
                pathTooLong = true;
            }
            return flag;
        }

        internal static bool IsValidDirectoryName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            if (name.IndexOfAny(_invalidFileNameChars, 0) != -1)
            {
                return false;
            }
            return (!name.Equals(".") && !name.Equals(".."));
        }

        internal static void PhysicalPathStatus(string physicalPath, bool directoryExistsOnError, bool fileExistsOnError, out bool exists, out bool isDirectory)
        {
            exists = false;
            isDirectory = true;
            if (!string.IsNullOrEmpty(physicalPath))
            {
                using (new ApplicationImpersonationContext())
                {
                    System.Web.UnsafeNativeMethods.WIN32_FILE_ATTRIBUTE_DATA win_file_attribute_data;
                    if (System.Web.UnsafeNativeMethods.GetFileAttributesEx(physicalPath, 0, out win_file_attribute_data))
                    {
                        exists = true;
                        isDirectory = (win_file_attribute_data.fileAttributes & 0x10) == 0x10;
                        if (isDirectory && HasInvalidLastChar(physicalPath))
                        {
                            exists = false;
                        }
                    }
                    else if (directoryExistsOnError || fileExistsOnError)
                    {
                        int num = Marshal.GetHRForLastWin32Error();
                        if ((num != -2147024894) && (num != -2147024893))
                        {
                            exists = true;
                            isDirectory = directoryExistsOnError;
                        }
                    }
                }
            }
        }

        internal static string RemoveTrailingDirectoryBackSlash(string path)
        {
            if (path == null)
            {
                return null;
            }
            int length = path.Length;
            if ((length > 3) && (path[length - 1] == '\\'))
            {
                path = path.Substring(0, length - 1);
            }
            return path;
        }

        internal static string TruncatePathIfNeeded(string path, int reservedLength)
        {
            int num = _maxPathLength - reservedLength;
            if (path.Length > num)
            {
                path = path.Substring(0, num - 13) + path.GetHashCode().ToString(CultureInfo.InvariantCulture);
            }
            return path;
        }
    }
}

