namespace System.IO
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Text;

    [ComVisible(true)]
    public static class Path
    {
        public static readonly char AltDirectorySeparatorChar = '/';
        public static readonly char DirectorySeparatorChar = '\\';
        private static readonly char[] InvalidFileNameChars = new char[] { 
            '"', '<', '>', '|', '\0', '\x0001', '\x0002', '\x0003', '\x0004', '\x0005', '\x0006', '\a', '\b', '\t', '\n', '\v', 
            '\f', '\r', '\x000e', '\x000f', '\x0010', '\x0011', '\x0012', '\x0013', '\x0014', '\x0015', '\x0016', '\x0017', '\x0018', '\x0019', '\x001a', '\x001b', 
            '\x001c', '\x001d', '\x001e', '\x001f', ':', '*', '?', '\\', '/'
         };
        [Obsolete("Please use GetInvalidPathChars or GetInvalidFileNameChars instead.")]
        public static readonly char[] InvalidPathChars = new char[] { 
            '"', '<', '>', '|', '\0', '\x0001', '\x0002', '\x0003', '\x0004', '\x0005', '\x0006', '\a', '\b', '\t', '\n', '\v', 
            '\f', '\r', '\x000e', '\x000f', '\x0010', '\x0011', '\x0012', '\x0013', '\x0014', '\x0015', '\x0016', '\x0017', '\x0018', '\x0019', '\x001a', '\x001b', 
            '\x001c', '\x001d', '\x001e', '\x001f'
         };
        internal const int MAX_DIRECTORY_PATH = 0xf8;
        internal const int MAX_PATH = 260;
        private static readonly int MaxDirectoryLength = 0xff;
        internal static readonly int MaxLongPath = 0x7d00;
        internal static readonly int MaxPath = 260;
        public static readonly char PathSeparator = ';';
        private static readonly string Prefix = @"\\?\";
        private static readonly char[] RealInvalidPathChars = new char[] { 
            '"', '<', '>', '|', '\0', '\x0001', '\x0002', '\x0003', '\x0004', '\x0005', '\x0006', '\a', '\b', '\t', '\n', '\v', 
            '\f', '\r', '\x000e', '\x000f', '\x0010', '\x0011', '\x0012', '\x0013', '\x0014', '\x0015', '\x0016', '\x0017', '\x0018', '\x0019', '\x001a', '\x001b', 
            '\x001c', '\x001d', '\x001e', '\x001f'
         };
        private static char[] s_Base32Char = new char[] { 
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 
            'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5'
         };
        internal static readonly char[] TrimEndChars = new char[] { '\t', '\n', '\v', '\f', '\r', ' ', '\x0085', '\x00a0' };
        public static readonly char VolumeSeparatorChar = ':';

        internal static string AddLongPathPrefix(string path)
        {
            if (path.StartsWith(Prefix, StringComparison.Ordinal))
            {
                return path;
            }
            return (Prefix + path);
        }

        public static string ChangeExtension(string path, string extension)
        {
            if (path == null)
            {
                return null;
            }
            CheckInvalidPathChars(path);
            string str = path;
            int length = path.Length;
            while (--length >= 0)
            {
                char ch = path[length];
                if (ch == '.')
                {
                    str = path.Substring(0, length);
                    break;
                }
                if (((ch == DirectorySeparatorChar) || (ch == AltDirectorySeparatorChar)) || (ch == VolumeSeparatorChar))
                {
                    break;
                }
            }
            if ((extension == null) || (path.Length == 0))
            {
                return str;
            }
            if ((extension.Length == 0) || (extension[0] != '.'))
            {
                str = str + ".";
            }
            return (str + extension);
        }

        internal static void CheckInvalidPathChars(string path)
        {
            for (int i = 0; i < path.Length; i++)
            {
                int num2 = path[i];
                if (((num2 == 0x22) || (num2 == 60)) || (((num2 == 0x3e) || (num2 == 0x7c)) || (num2 < 0x20)))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidPathChars"));
                }
            }
        }

        [SecuritySafeCritical]
        internal static void CheckSearchPattern(string searchPattern)
        {
            int num;
            while ((num = searchPattern.IndexOf("..", StringComparison.Ordinal)) != -1)
            {
                if ((num + 2) == searchPattern.Length)
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_InvalidSearchPattern"));
                }
                if ((searchPattern[num + 2] == DirectorySeparatorChar) || (searchPattern[num + 2] == AltDirectorySeparatorChar))
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_InvalidSearchPattern"));
                }
                searchPattern = searchPattern.Substring(num + 2);
            }
        }

        public static string Combine(params string[] paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException("paths");
            }
            int capacity = 0;
            int num2 = 0;
            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i] == null)
                {
                    throw new ArgumentNullException("paths");
                }
                if (paths[i].Length != 0)
                {
                    CheckInvalidPathChars(paths[i]);
                    if (IsPathRooted(paths[i]))
                    {
                        num2 = i;
                        capacity = paths[i].Length;
                    }
                    else
                    {
                        capacity += paths[i].Length;
                    }
                    char ch = paths[i][paths[i].Length - 1];
                    if (((ch != DirectorySeparatorChar) && (ch != AltDirectorySeparatorChar)) && (ch != VolumeSeparatorChar))
                    {
                        capacity++;
                    }
                }
            }
            StringBuilder builder = new StringBuilder(capacity);
            for (int j = num2; j < paths.Length; j++)
            {
                if (paths[j].Length != 0)
                {
                    if (builder.Length == 0)
                    {
                        builder.Append(paths[j]);
                    }
                    else
                    {
                        char ch2 = builder[builder.Length - 1];
                        if (((ch2 != DirectorySeparatorChar) && (ch2 != AltDirectorySeparatorChar)) && (ch2 != VolumeSeparatorChar))
                        {
                            builder.Append(DirectorySeparatorChar);
                        }
                        builder.Append(paths[j]);
                    }
                }
            }
            return builder.ToString();
        }

        public static string Combine(string path1, string path2)
        {
            if ((path1 == null) || (path2 == null))
            {
                throw new ArgumentNullException((path1 == null) ? "path1" : "path2");
            }
            CheckInvalidPathChars(path1);
            CheckInvalidPathChars(path2);
            return CombineNoChecks(path1, path2);
        }

        public static string Combine(string path1, string path2, string path3)
        {
            if (((path1 == null) || (path2 == null)) || (path3 == null))
            {
                throw new ArgumentNullException((path1 == null) ? "path1" : ((path2 == null) ? "path2" : "path3"));
            }
            CheckInvalidPathChars(path1);
            CheckInvalidPathChars(path2);
            CheckInvalidPathChars(path3);
            return CombineNoChecks(CombineNoChecks(path1, path2), path3);
        }

        public static string Combine(string path1, string path2, string path3, string path4)
        {
            if (((path1 == null) || (path2 == null)) || ((path3 == null) || (path4 == null)))
            {
                throw new ArgumentNullException((path1 == null) ? "path1" : ((path2 == null) ? "path2" : ((path3 == null) ? "path3" : "path4")));
            }
            CheckInvalidPathChars(path1);
            CheckInvalidPathChars(path2);
            CheckInvalidPathChars(path3);
            CheckInvalidPathChars(path4);
            return CombineNoChecks(CombineNoChecks(CombineNoChecks(path1, path2), path3), path4);
        }

        private static string CombineNoChecks(string path1, string path2)
        {
            if (path2.Length == 0)
            {
                return path1;
            }
            if (path1.Length == 0)
            {
                return path2;
            }
            if (IsPathRooted(path2))
            {
                return path2;
            }
            char ch = path1[path1.Length - 1];
            if (((ch != DirectorySeparatorChar) && (ch != AltDirectorySeparatorChar)) && (ch != VolumeSeparatorChar))
            {
                return (path1 + DirectorySeparatorChar + path2);
            }
            return (path1 + path2);
        }

        [SecuritySafeCritical]
        public static string GetDirectoryName(string path)
        {
            if (path != null)
            {
                CheckInvalidPathChars(path);
                path = NormalizePath(path, false);
                int rootLength = GetRootLength(path);
                if (path.Length > rootLength)
                {
                    int length = path.Length;
                    if (length == rootLength)
                    {
                        return null;
                    }
                    while (((length > rootLength) && (path[--length] != DirectorySeparatorChar)) && (path[length] != AltDirectorySeparatorChar))
                    {
                    }
                    return path.Substring(0, length);
                }
            }
            return null;
        }

        public static string GetExtension(string path)
        {
            if (path == null)
            {
                return null;
            }
            CheckInvalidPathChars(path);
            int length = path.Length;
            int startIndex = length;
            while (--startIndex >= 0)
            {
                char ch = path[startIndex];
                if (ch == '.')
                {
                    if (startIndex != (length - 1))
                    {
                        return path.Substring(startIndex, length - startIndex);
                    }
                    return string.Empty;
                }
                if (((ch == DirectorySeparatorChar) || (ch == AltDirectorySeparatorChar)) || (ch == VolumeSeparatorChar))
                {
                    break;
                }
            }
            return string.Empty;
        }

        public static string GetFileName(string path)
        {
            if (path != null)
            {
                CheckInvalidPathChars(path);
                int length = path.Length;
                int num2 = length;
                while (--num2 >= 0)
                {
                    char ch = path[num2];
                    if (((ch == DirectorySeparatorChar) || (ch == AltDirectorySeparatorChar)) || (ch == VolumeSeparatorChar))
                    {
                        return path.Substring(num2 + 1, (length - num2) - 1);
                    }
                }
            }
            return path;
        }

        public static string GetFileNameWithoutExtension(string path)
        {
            path = GetFileName(path);
            if (path == null)
            {
                return null;
            }
            int length = path.LastIndexOf('.');
            if (length == -1)
            {
                return path;
            }
            return path.Substring(0, length);
        }

        [SecuritySafeCritical]
        public static string GetFullPath(string path)
        {
            string fullPathInternal = GetFullPathInternal(path);
            new FileIOPermission(FileIOPermissionAccess.PathDiscovery, new string[] { fullPathInternal }, false, false).Demand();
            return fullPathInternal;
        }

        internal static string GetFullPathInternal(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            return NormalizePath(path, true);
        }

        public static char[] GetInvalidFileNameChars()
        {
            return (char[]) InvalidFileNameChars.Clone();
        }

        public static char[] GetInvalidPathChars()
        {
            return (char[]) RealInvalidPathChars.Clone();
        }

        [SecuritySafeCritical]
        public static string GetPathRoot(string path)
        {
            if (path == null)
            {
                return null;
            }
            path = NormalizePath(path, false);
            return path.Substring(0, GetRootLength(path));
        }

        public static string GetRandomFileName()
        {
            byte[] data = new byte[10];
            using (null)
            {
                new RNGCryptoServiceProvider().GetBytes(data);
                char[] chArray = ToBase32StringSuitableForDirName(data).ToCharArray();
                chArray[8] = '.';
                return new string(chArray, 0, 12);
            }
        }

        internal static int GetRootLength(string path)
        {
            CheckInvalidPathChars(path);
            int num = 0;
            int length = path.Length;
            if ((length >= 1) && IsDirectorySeparator(path[0]))
            {
                num = 1;
                if ((length >= 2) && IsDirectorySeparator(path[1]))
                {
                    num = 2;
                    int num3 = 2;
                    while ((num < length) && (((path[num] != DirectorySeparatorChar) && (path[num] != AltDirectorySeparatorChar)) || (--num3 > 0)))
                    {
                        num++;
                    }
                }
                return num;
            }
            if ((length >= 2) && (path[1] == VolumeSeparatorChar))
            {
                num = 2;
                if ((length >= 3) && IsDirectorySeparator(path[2]))
                {
                    num++;
                }
            }
            return num;
        }

        [SecuritySafeCritical]
        public static string GetTempFileName()
        {
            string tempPath = GetTempPath();
            new FileIOPermission(FileIOPermissionAccess.Write, tempPath).Demand();
            StringBuilder tmpFileName = new StringBuilder(260);
            if (Win32Native.GetTempFileName(tempPath, "tmp", 0, tmpFileName) == 0)
            {
                __Error.WinIOError();
            }
            return tmpFileName.ToString();
        }

        [SecuritySafeCritical]
        public static string GetTempPath()
        {
            new EnvironmentPermission(PermissionState.Unrestricted).Demand();
            StringBuilder buffer = new StringBuilder(260);
            uint tempPath = Win32Native.GetTempPath(260, buffer);
            string path = buffer.ToString();
            if (tempPath == 0)
            {
                __Error.WinIOError();
            }
            return GetFullPathInternal(path);
        }

        public static bool HasExtension(string path)
        {
            if (path != null)
            {
                CheckInvalidPathChars(path);
                int length = path.Length;
                while (--length >= 0)
                {
                    char ch = path[length];
                    if (ch == '.')
                    {
                        return (length != (path.Length - 1));
                    }
                    if (((ch == DirectorySeparatorChar) || (ch == AltDirectorySeparatorChar)) || (ch == VolumeSeparatorChar))
                    {
                        break;
                    }
                }
            }
            return false;
        }

        internal static bool HasLongPathPrefix(string path)
        {
            return path.StartsWith(Prefix, StringComparison.Ordinal);
        }

        internal static string InternalCombine(string path1, string path2)
        {
            if ((path1 == null) || (path2 == null))
            {
                throw new ArgumentNullException((path1 == null) ? "path1" : "path2");
            }
            CheckInvalidPathChars(path1);
            CheckInvalidPathChars(path2);
            if (path2.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_PathEmpty"), "path2");
            }
            if (IsPathRooted(path2))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_Path2IsRooted"), "path2");
            }
            int length = path1.Length;
            if (length == 0)
            {
                return path2;
            }
            char ch = path1[length - 1];
            if (((ch != DirectorySeparatorChar) && (ch != AltDirectorySeparatorChar)) && (ch != VolumeSeparatorChar))
            {
                return (path1 + DirectorySeparatorChar + path2);
            }
            return (path1 + path2);
        }

        internal static bool IsDirectorySeparator(char c)
        {
            if (c != DirectorySeparatorChar)
            {
                return (c == AltDirectorySeparatorChar);
            }
            return true;
        }

        public static bool IsPathRooted(string path)
        {
            if (path != null)
            {
                CheckInvalidPathChars(path);
                int length = path.Length;
                if (((length >= 1) && ((path[0] == DirectorySeparatorChar) || (path[0] == AltDirectorySeparatorChar))) || ((length >= 2) && (path[1] == VolumeSeparatorChar)))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsRelative(string path)
        {
            return (((((path.Length < 3) || (path[1] != VolumeSeparatorChar)) || (path[2] != DirectorySeparatorChar)) || (((path[0] < 'a') || (path[0] > 'z')) && ((path[0] < 'A') || (path[0] > 'Z')))) && (((path.Length < 2) || (path[0] != '\\')) || (path[1] != '\\')));
        }

        [SecuritySafeCritical]
        internal static string NormalizePath(string path, bool fullCheck)
        {
            return NormalizePath(path, fullCheck, MaxPath);
        }

        [SecurityCritical]
        internal static unsafe string NormalizePath(string path, bool fullCheck, int maxPathLength)
        {
            if (fullCheck)
            {
                path = path.TrimEnd(TrimEndChars);
                CheckInvalidPathChars(path);
            }
            int num = 0;
            PathHelper helper = null;
            if (path.Length <= MaxPath)
            {
                char* charArrayPtr = (char*) stackalloc byte[(((IntPtr) MaxPath) * 2)];
                helper = new PathHelper(charArrayPtr, MaxPath);
            }
            else
            {
                helper = new PathHelper(path.Length + MaxPath, maxPathLength);
            }
            uint num2 = 0;
            uint num3 = 0;
            bool flag = false;
            uint num4 = 0;
            int num5 = -1;
            bool flag2 = false;
            bool flag3 = true;
            bool flag4 = false;
            int num6 = 0;
            if ((path.Length > 0) && ((path[0] == DirectorySeparatorChar) || (path[0] == AltDirectorySeparatorChar)))
            {
                helper.Append('\\');
                num++;
                num5 = 0;
            }
            while (num < path.Length)
            {
                char ch = path[num];
                if ((ch == DirectorySeparatorChar) || (ch == AltDirectorySeparatorChar))
                {
                    if (num4 == 0)
                    {
                        if (num3 > 0)
                        {
                            int num7 = num5 + 1;
                            if (path[num7] != '.')
                            {
                                throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
                            }
                            if (num3 >= 2)
                            {
                                if (flag2 && (num3 > 2))
                                {
                                    throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
                                }
                                if (path[num7 + 1] == '.')
                                {
                                    for (int i = num7 + 2; i < (num7 + num3); i++)
                                    {
                                        if (path[i] != '.')
                                        {
                                            throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
                                        }
                                    }
                                    num3 = 2;
                                }
                                else
                                {
                                    if (num3 > 1)
                                    {
                                        throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
                                    }
                                    num3 = 1;
                                }
                            }
                            if (num3 == 2)
                            {
                                helper.Append('.');
                            }
                            helper.Append('.');
                            flag = false;
                        }
                        if ((((num2 > 0) && flag3) && ((num + 1) < path.Length)) && ((path[num + 1] == DirectorySeparatorChar) || (path[num + 1] == AltDirectorySeparatorChar)))
                        {
                            helper.Append(DirectorySeparatorChar);
                        }
                    }
                    num3 = 0;
                    num2 = 0;
                    if (!flag)
                    {
                        flag = true;
                        helper.Append(DirectorySeparatorChar);
                    }
                    num4 = 0;
                    num5 = num;
                    flag2 = false;
                    flag3 = false;
                    if (flag4)
                    {
                        helper.TryExpandShortFileName();
                        flag4 = false;
                    }
                    int num9 = helper.Length - 1;
                    if ((num9 - num6) > MaxDirectoryLength)
                    {
                        throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
                    }
                    num6 = num9;
                }
                else
                {
                    switch (ch)
                    {
                        case '.':
                            num3++;
                            goto Label_0352;

                        case ' ':
                            num2++;
                            goto Label_0352;

                        case '~':
                            flag4 = true;
                            break;
                    }
                    flag = false;
                    if (flag3 && (ch == VolumeSeparatorChar))
                    {
                        char ch2 = (num > 0) ? path[num - 1] : ' ';
                        if (((num3 != 0) || (num4 < 1)) || (ch2 == ' '))
                        {
                            throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
                        }
                        flag2 = true;
                        if (num4 > 1)
                        {
                            int num10 = 0;
                            while ((num10 < helper.Length) && (helper[num10] == ' '))
                            {
                                num10++;
                            }
                            if ((num4 - num10) == 1L)
                            {
                                helper.Length = 0;
                                helper.Append(ch2);
                            }
                        }
                        num4 = 0;
                    }
                    else
                    {
                        num4 += (1 + num3) + num2;
                    }
                    if ((num3 > 0) || (num2 > 0))
                    {
                        int num11 = (num5 >= 0) ? ((num - num5) - 1) : num;
                        if (num11 > 0)
                        {
                            for (int j = 0; j < num11; j++)
                            {
                                helper.Append(path[(num5 + 1) + j]);
                            }
                        }
                        num3 = 0;
                        num2 = 0;
                    }
                    helper.Append(ch);
                    num5 = num;
                }
            Label_0352:
                num++;
            }
            if (((helper.Length - 1) - num6) > MaxDirectoryLength)
            {
                throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
            }
            if ((num4 == 0) && (num3 > 0))
            {
                int num13 = num5 + 1;
                if (path[num13] != '.')
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
                }
                if (num3 >= 2)
                {
                    if (flag2 && (num3 > 2))
                    {
                        throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
                    }
                    if (path[num13 + 1] == '.')
                    {
                        for (int k = num13 + 2; k < (num13 + num3); k++)
                        {
                            if (path[k] != '.')
                            {
                                throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
                            }
                        }
                        num3 = 2;
                    }
                    else
                    {
                        if (num3 > 1)
                        {
                            throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
                        }
                        num3 = 1;
                    }
                }
                if (num3 == 2)
                {
                    helper.Append('.');
                }
                helper.Append('.');
            }
            if (helper.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
            }
            if (fullCheck && (helper.OrdinalStartsWith("http:", false) || helper.OrdinalStartsWith("file:", false)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_PathUriFormatNotSupported"));
            }
            if (flag4)
            {
                helper.TryExpandShortFileName();
            }
            int fullPathName = 1;
            if (fullCheck)
            {
                fullPathName = helper.GetFullPathName();
                flag4 = false;
                for (int m = 0; (m < helper.Length) && !flag4; m++)
                {
                    if (helper[m] == '~')
                    {
                        flag4 = true;
                    }
                }
                if (flag4 && !helper.TryExpandShortFileName())
                {
                    int lastSlash = -1;
                    for (int n = helper.Length - 1; n >= 0; n--)
                    {
                        if (helper[n] == DirectorySeparatorChar)
                        {
                            lastSlash = n;
                            break;
                        }
                    }
                    if (lastSlash >= 0)
                    {
                        if (helper.Length >= maxPathLength)
                        {
                            throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
                        }
                        int lenSavedName = (helper.Length - lastSlash) - 1;
                        helper.Fixup(lenSavedName, lastSlash);
                    }
                }
            }
            if (((fullPathName != 0) && (helper.Length > 1)) && ((helper[0] == '\\') && (helper[1] == '\\')))
            {
                int num20 = 2;
                while (num20 < fullPathName)
                {
                    if (helper[num20] == '\\')
                    {
                        num20++;
                        break;
                    }
                    num20++;
                }
                if (num20 == fullPathName)
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegalUNC"));
                }
                if (helper.OrdinalStartsWith(@"\\?\globalroot", true))
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_PathGlobalRoot"));
                }
            }
            if (helper.Length >= maxPathLength)
            {
                throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
            }
            if (fullPathName == 0)
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode == 0)
                {
                    errorCode = 0xa1;
                }
                __Error.WinIOError(errorCode, path);
                return null;
            }
            string a = helper.ToString();
            if (string.Equals(a, path, StringComparison.Ordinal))
            {
                a = path;
            }
            return a;
        }

        internal static string RemoveLongPathPrefix(string path)
        {
            if (!path.StartsWith(Prefix, StringComparison.Ordinal))
            {
                return path;
            }
            return path.Substring(4);
        }

        internal static StringBuilder RemoveLongPathPrefix(StringBuilder path)
        {
            if (!path.ToString().StartsWith(Prefix, StringComparison.Ordinal))
            {
                return path;
            }
            return path.Remove(0, 4);
        }

        internal static string ToBase32StringSuitableForDirName(byte[] buff)
        {
            StringBuilder builder = new StringBuilder();
            int length = buff.Length;
            int num7 = 0;
            do
            {
                byte num = (num7 < length) ? buff[num7++] : ((byte) 0);
                byte num2 = (num7 < length) ? buff[num7++] : ((byte) 0);
                byte index = (num7 < length) ? buff[num7++] : ((byte) 0);
                byte num4 = (num7 < length) ? buff[num7++] : ((byte) 0);
                byte num5 = (num7 < length) ? buff[num7++] : ((byte) 0);
                builder.Append(s_Base32Char[num & 0x1f]);
                builder.Append(s_Base32Char[num2 & 0x1f]);
                builder.Append(s_Base32Char[index & 0x1f]);
                builder.Append(s_Base32Char[num4 & 0x1f]);
                builder.Append(s_Base32Char[num5 & 0x1f]);
                builder.Append(s_Base32Char[((num & 0xe0) >> 5) | ((num4 & 0x60) >> 2)]);
                builder.Append(s_Base32Char[((num2 & 0xe0) >> 5) | ((num5 & 0x60) >> 2)]);
                index = (byte) (index >> 5);
                if ((num4 & 0x80) != 0)
                {
                    index = (byte) (index | 8);
                }
                if ((num5 & 0x80) != 0)
                {
                    index = (byte) (index | 0x10);
                }
                builder.Append(s_Base32Char[index]);
            }
            while (num7 < length);
            return builder.ToString();
        }
    }
}

