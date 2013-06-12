namespace System.Web.Util
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Web;

    internal static class UrlPath
    {
        internal const char appRelativeCharacter = '~';
        internal const string appRelativeCharacterString = "~/";
        private const string dummyProtocolAndServer = "file://foo";
        private static char[] s_slashChars = new char[] { '\\', '/' };

        internal static string AppendSlashToPathIfNeeded(string path)
        {
            if (path == null)
            {
                return null;
            }
            int length = path.Length;
            if ((length != 0) && (path[length - 1] != '/'))
            {
                path = path + '/';
            }
            return path;
        }

        internal static void CheckValidVirtualPath(string path)
        {
            if (IsAbsolutePhysicalPath(path))
            {
                throw new HttpException(System.Web.SR.GetString("Physical_path_not_allowed", new object[] { path }));
            }
            int index = path.IndexOf('?');
            if (index >= 0)
            {
                path = path.Substring(0, index);
            }
            if (HasScheme(path))
            {
                throw new HttpException(System.Web.SR.GetString("Invalid_vpath", new object[] { path }));
            }
        }

        internal static string Combine(string basepath, string relative)
        {
            return Combine(HttpRuntime.AppDomainAppVirtualPathString, basepath, relative);
        }

        private static string Combine(string appPath, string basepath, string relative)
        {
            string str;
            if (string.IsNullOrEmpty(relative))
            {
                throw new ArgumentNullException("relative");
            }
            if (string.IsNullOrEmpty(basepath))
            {
                throw new ArgumentNullException("basepath");
            }
            if ((basepath[0] == '~') && (basepath.Length == 1))
            {
                basepath = "~/";
            }
            else
            {
                int num = basepath.LastIndexOf('/');
                if (num < (basepath.Length - 1))
                {
                    basepath = basepath.Substring(0, num + 1);
                }
            }
            CheckValidVirtualPath(relative);
            if (IsRooted(relative))
            {
                str = relative;
            }
            else
            {
                if ((relative.Length == 1) && (relative[0] == '~'))
                {
                    return appPath;
                }
                if (IsAppRelativePath(relative))
                {
                    if (appPath.Length > 1)
                    {
                        str = appPath + "/" + relative.Substring(2);
                    }
                    else
                    {
                        str = "/" + relative.Substring(2);
                    }
                }
                else
                {
                    str = SimpleCombine(basepath, relative);
                }
            }
            return Reduce(str);
        }

        internal static string FixVirtualPathSlashes(string virtualPath)
        {
            virtualPath = virtualPath.Replace('\\', '/');
            while (true)
            {
                string str = virtualPath.Replace("//", "/");
                if (str == virtualPath)
                {
                    return virtualPath;
                }
                virtualPath = str;
            }
        }

        internal static string GetDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(System.Web.SR.GetString("Empty_path_has_no_directory"));
            }
            if ((path[0] != '/') && (path[0] != '~'))
            {
                throw new ArgumentException(System.Web.SR.GetString("Path_must_be_rooted", new object[] { path }));
            }
            if (path.Length == 1)
            {
                return path;
            }
            int num = path.LastIndexOf('/');
            if (num < 0)
            {
                throw new ArgumentException(System.Web.SR.GetString("Path_must_be_rooted", new object[] { path }));
            }
            return path.Substring(0, num + 1);
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

        internal static string GetExtension(string virtualPath)
        {
            if (virtualPath == null)
            {
                return null;
            }
            int length = virtualPath.Length;
            int startIndex = length;
            while (--startIndex >= 0)
            {
                char ch = virtualPath[startIndex];
                if (ch == '.')
                {
                    if (startIndex != (length - 1))
                    {
                        return virtualPath.Substring(startIndex, length - startIndex);
                    }
                    return string.Empty;
                }
                if (ch == '/')
                {
                    break;
                }
            }
            return string.Empty;
        }

        internal static string GetFileName(string virtualPath)
        {
            if (virtualPath != null)
            {
                int length = virtualPath.Length;
                int num2 = length;
                while (--num2 >= 0)
                {
                    char ch = virtualPath[num2];
                    if (ch == '/')
                    {
                        return virtualPath.Substring(num2 + 1, (length - num2) - 1);
                    }
                }
            }
            return virtualPath;
        }

        internal static string GetFileNameWithoutExtension(string virtualPath)
        {
            virtualPath = GetFileName(virtualPath);
            if (virtualPath == null)
            {
                return null;
            }
            int length = virtualPath.LastIndexOf('.');
            if (length == -1)
            {
                return virtualPath;
            }
            return virtualPath.Substring(0, length);
        }

        private static bool HasScheme(string virtualPath)
        {
            int index = virtualPath.IndexOf(':');
            if (index == -1)
            {
                return false;
            }
            int num2 = virtualPath.IndexOf('/');
            if (num2 != -1)
            {
                return (index < num2);
            }
            return true;
        }

        internal static bool HasTrailingSlash(string virtualPath)
        {
            return (virtualPath[virtualPath.Length - 1] == '/');
        }

        internal static bool IsAbsolutePhysicalPath(string path)
        {
            if ((path == null) || (path.Length < 3))
            {
                return false;
            }
            return (((path[1] == ':') && IsDirectorySeparatorChar(path[2])) || IsUncSharePath(path));
        }

        internal static bool IsAppRelativePath(string path)
        {
            if (path == null)
            {
                return false;
            }
            int length = path.Length;
            if (length == 0)
            {
                return false;
            }
            if (path[0] != '~')
            {
                return false;
            }
            if ((length != 1) && (path[1] != '\\'))
            {
                return (path[1] == '/');
            }
            return true;
        }

        private static bool IsDirectorySeparatorChar(char ch)
        {
            if (ch != '\\')
            {
                return (ch == '/');
            }
            return true;
        }

        internal static bool IsEqualOrSubpath(string path, string subpath)
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
                if (!StringUtil.EqualsIgnoreCase(path, 0, subpath, 0, length))
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

        internal static bool IsPathOnSameServer(string absUriOrLocalPath, Uri currentRequestUri)
        {
            Uri uri;
            if (!Uri.TryCreate(absUriOrLocalPath, UriKind.Absolute, out uri))
            {
                if (AppSettings.AllowRelaxedRelativeUrl)
                {
                    return true;
                }
                if (!IsRooted(absUriOrLocalPath) && !IsRelativeUrl(absUriOrLocalPath))
                {
                    return false;
                }
                return !absUriOrLocalPath.TrimStart(new char[] { ' ' }).StartsWith("//");
            }
            if (!uri.IsLoopback)
            {
                return string.Equals(currentRequestUri.Host, uri.Host, StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }

        internal static bool IsRelativeUrl(string virtualPath)
        {
            if (HasScheme(virtualPath))
            {
                return false;
            }
            return !IsRooted(virtualPath);
        }

        internal static bool IsRooted(string basepath)
        {
            if (!string.IsNullOrEmpty(basepath) && (basepath[0] != '/'))
            {
                return (basepath[0] == '\\');
            }
            return true;
        }

        internal static bool IsUncSharePath(string path)
        {
            return (((path.Length > 2) && IsDirectorySeparatorChar(path[0])) && IsDirectorySeparatorChar(path[1]));
        }

        internal static bool IsValidVirtualPathWithoutProtocol(string path)
        {
            if (path == null)
            {
                return false;
            }
            return !HasScheme(path);
        }

        internal static string MakeRelative(string from, string to)
        {
            string str2;
            from = MakeVirtualPathAppAbsolute(from);
            to = MakeVirtualPathAppAbsolute(to);
            if (!IsRooted(from))
            {
                throw new ArgumentException(System.Web.SR.GetString("Path_must_be_rooted", new object[] { from }));
            }
            if (!IsRooted(to))
            {
                throw new ArgumentException(System.Web.SR.GetString("Path_must_be_rooted", new object[] { to }));
            }
            string str = null;
            if (to != null)
            {
                int index = to.IndexOf('?');
                if (index >= 0)
                {
                    str = to.Substring(index);
                    to = to.Substring(0, index);
                }
            }
            Uri uri = new Uri("file://foo" + from);
            Uri uri2 = new Uri("file://foo" + to);
            if (uri.Equals(uri2))
            {
                int num2 = to.LastIndexOfAny(s_slashChars);
                if (num2 >= 0)
                {
                    if (num2 == (to.Length - 1))
                    {
                        str2 = "./";
                    }
                    else
                    {
                        str2 = to.Substring(num2 + 1);
                    }
                }
                else
                {
                    str2 = to;
                }
            }
            else
            {
                str2 = uri.MakeRelative(uri2);
            }
            return (str2 + str + uri2.Fragment);
        }

        internal static string MakeVirtualPathAppAbsolute(string virtualPath)
        {
            return MakeVirtualPathAppAbsolute(virtualPath, HttpRuntime.AppDomainAppVirtualPathString);
        }

        internal static string MakeVirtualPathAppAbsolute(string virtualPath, string applicationPath)
        {
            if ((virtualPath.Length == 1) && (virtualPath[0] == '~'))
            {
                return applicationPath;
            }
            if (((virtualPath.Length >= 2) && (virtualPath[0] == '~')) && ((virtualPath[1] == '/') || (virtualPath[1] == '\\')))
            {
                if (applicationPath.Length > 1)
                {
                    return (applicationPath + virtualPath.Substring(2));
                }
                return ("/" + virtualPath.Substring(2));
            }
            if (!IsRooted(virtualPath))
            {
                throw new ArgumentOutOfRangeException("virtualPath");
            }
            return virtualPath;
        }

        internal static string MakeVirtualPathAppAbsoluteReduceAndCheck(string virtualPath)
        {
            if (virtualPath == null)
            {
                throw new ArgumentNullException("virtualPath");
            }
            string str = Reduce(MakeVirtualPathAppAbsolute(virtualPath));
            if (!VirtualPathStartsWithAppPath(str))
            {
                throw new ArgumentException(System.Web.SR.GetString("Invalid_app_VirtualPath", new object[] { virtualPath }));
            }
            return str;
        }

        internal static string MakeVirtualPathAppRelative(string virtualPath)
        {
            return MakeVirtualPathAppRelative(virtualPath, HttpRuntime.AppDomainAppVirtualPathString, false);
        }

        internal static string MakeVirtualPathAppRelative(string virtualPath, string applicationPath, bool nullIfNotInApp)
        {
            if (virtualPath == null)
            {
                throw new ArgumentNullException("virtualPath");
            }
            int length = applicationPath.Length;
            int num2 = virtualPath.Length;
            if ((num2 == (length - 1)) && StringUtil.StringStartsWithIgnoreCase(applicationPath, virtualPath))
            {
                return "~/";
            }
            if (!VirtualPathStartsWithVirtualPath(virtualPath, applicationPath))
            {
                if (nullIfNotInApp)
                {
                    return null;
                }
                return virtualPath;
            }
            if (num2 == length)
            {
                return "~/";
            }
            if (length == 1)
            {
                return ('~' + virtualPath);
            }
            return ('~' + virtualPath.Substring(length - 1));
        }

        internal static string MakeVirtualPathAppRelativeOrNull(string virtualPath)
        {
            return MakeVirtualPathAppRelative(virtualPath, HttpRuntime.AppDomainAppVirtualPathString, true);
        }

        internal static bool PathEndsWithExtraSlash(string path)
        {
            if (path == null)
            {
                return false;
            }
            int length = path.Length;
            if ((length == 0) || (path[length - 1] != '\\'))
            {
                return false;
            }
            if ((length == 3) && (path[1] == ':'))
            {
                return false;
            }
            return true;
        }

        internal static bool PathIsDriveRoot(string path)
        {
            return (((path != null) && (path.Length == 3)) && ((path[1] == ':') && (path[2] == '\\')));
        }

        internal static string Reduce(string path)
        {
            string str = null;
            if (path != null)
            {
                int index = path.IndexOf('?');
                if (index >= 0)
                {
                    str = path.Substring(index);
                    path = path.Substring(0, index);
                }
            }
            path = FixVirtualPathSlashes(path);
            path = ReduceVirtualPath(path);
            if (str == null)
            {
                return path;
            }
            return (path + str);
        }

        internal static string ReduceVirtualPath(string path)
        {
            int length = path.Length;
            int startIndex = 0;
            while (true)
            {
                startIndex = path.IndexOf('.', startIndex);
                if (startIndex < 0)
                {
                    return path;
                }
                if (((startIndex == 0) || (path[startIndex - 1] == '/')) && ((((startIndex + 1) == length) || (path[startIndex + 1] == '/')) || ((path[startIndex + 1] == '.') && (((startIndex + 2) == length) || (path[startIndex + 2] == '/')))))
                {
                    break;
                }
                startIndex++;
            }
            ArrayList list = new ArrayList();
            StringBuilder builder = new StringBuilder();
            startIndex = 0;
            do
            {
                int num3 = startIndex;
                startIndex = path.IndexOf('/', num3 + 1);
                if (startIndex < 0)
                {
                    startIndex = length;
                }
                if ((((startIndex - num3) <= 3) && ((startIndex < 1) || (path[startIndex - 1] == '.'))) && (((num3 + 1) >= length) || (path[num3 + 1] == '.')))
                {
                    if ((startIndex - num3) == 3)
                    {
                        if (list.Count == 0)
                        {
                            throw new HttpException(System.Web.SR.GetString("Cannot_exit_up_top_directory"));
                        }
                        if ((list.Count == 1) && IsAppRelativePath(path))
                        {
                            return ReduceVirtualPath(MakeVirtualPathAppAbsolute(path));
                        }
                        builder.Length = (int) list[list.Count - 1];
                        list.RemoveRange(list.Count - 1, 1);
                    }
                }
                else
                {
                    list.Add(builder.Length);
                    builder.Append(path, num3, startIndex - num3);
                }
            }
            while (startIndex != length);
            string str = builder.ToString();
            if (str.Length != 0)
            {
                return str;
            }
            if ((length > 0) && (path[0] == '/'))
            {
                return "/";
            }
            return ".";
        }

        internal static string RemoveSlashFromPathIfNeeded(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            int length = path.Length;
            if ((length > 1) && (path[length - 1] == '/'))
            {
                return path.Substring(0, length - 1);
            }
            return path;
        }

        internal static string SimpleCombine(string basepath, string relative)
        {
            if (HasTrailingSlash(basepath))
            {
                return (basepath + relative);
            }
            return (basepath + "/" + relative);
        }

        internal static bool VirtualPathStartsWithAppPath(string virtualPath)
        {
            return VirtualPathStartsWithVirtualPath(virtualPath, HttpRuntime.AppDomainAppVirtualPathString);
        }

        private static bool VirtualPathStartsWithVirtualPath(string virtualPath1, string virtualPath2)
        {
            if (virtualPath1 == null)
            {
                throw new ArgumentNullException("virtualPath1");
            }
            if (virtualPath2 == null)
            {
                throw new ArgumentNullException("virtualPath2");
            }
            if (!StringUtil.StringStartsWithIgnoreCase(virtualPath1, virtualPath2))
            {
                return false;
            }
            int length = virtualPath2.Length;
            if (virtualPath1.Length != length)
            {
                if (length == 1)
                {
                    return true;
                }
                if (virtualPath2[length - 1] == '/')
                {
                    return true;
                }
                if (virtualPath1[length] != '/')
                {
                    return false;
                }
            }
            return true;
        }
    }
}

