namespace System.Windows.Forms
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    public class Help
    {
        private const int HH_ADD_BUTTON = 11;
        private const int HH_ADD_NAV_UI = 10;
        private const int HH_ALINK_LOOKUP = 0x13;
        private const int HH_CLOSE_ALL = 0x12;
        private const int HH_DISPLAY_INDEX = 2;
        private const int HH_DISPLAY_SEARCH = 3;
        private const int HH_DISPLAY_TEXT_POPUP = 14;
        private const int HH_DISPLAY_TOC = 1;
        private const int HH_DISPLAY_TOPIC = 0;
        private const int HH_ENUM_CATEGORY = 0x15;
        private const int HH_ENUM_CATEGORY_IT = 0x16;
        private const int HH_ENUM_INFO_TYPE = 7;
        private const int HH_GET_LAST_ERROR = 20;
        private const int HH_GET_WIN_HANDLE = 6;
        private const int HH_GET_WIN_TYPE = 5;
        private const int HH_GETBROWSER_APP = 12;
        private const int HH_HELP_CONTEXT = 15;
        private const int HH_HELP_FINDER = 0;
        private const int HH_KEYWORD_LOOKUP = 13;
        private const int HH_RESET_IT_FILTER = 0x17;
        private const int HH_SET_EXCLUSIVE_FILTER = 0x19;
        private const int HH_SET_GUID = 0x1a;
        private const int HH_SET_INCLUSIVE_FILTER = 0x18;
        private const int HH_SET_INFO_TYPE = 8;
        private const int HH_SET_WIN_TYPE = 4;
        private const int HH_SYNC = 9;
        private const int HH_TP_HELP_CONTEXTMENU = 0x10;
        private const int HH_TP_HELP_WM_HELP = 0x11;
        private const int HTML10HELP = 2;
        private const int HTMLFILE = 3;
        internal static readonly TraceSwitch WindowsFormsHelpTrace;

        private Help()
        {
        }

        private static int GetHelpFileType(string url)
        {
            if (url != null)
            {
                Uri uri = Resolve(url);
                if ((uri == null) || (uri.Scheme == "file"))
                {
                    switch (Path.GetExtension((uri == null) ? url : (uri.LocalPath + uri.Fragment)).ToLower(CultureInfo.InvariantCulture))
                    {
                        case ".chm":
                        case ".col":
                            return 2;
                    }
                }
            }
            return 3;
        }

        private static int MapCommandToHTMLCommand(HelpNavigator command, string param, out object htmlParam)
        {
            htmlParam = param;
            if (string.IsNullOrEmpty(param) && ((command == HelpNavigator.AssociateIndex) || (command == HelpNavigator.KeywordIndex)))
            {
                return 2;
            }
            switch (command)
            {
                case HelpNavigator.Topic:
                    return 0;

                case HelpNavigator.TableOfContents:
                    return 1;

                case HelpNavigator.Index:
                    return 2;

                case HelpNavigator.Find:
                {
                    System.Windows.Forms.NativeMethods.HH_FTS_QUERY hh_fts_query = new System.Windows.Forms.NativeMethods.HH_FTS_QUERY {
                        pszSearchQuery = param
                    };
                    htmlParam = hh_fts_query;
                    return 3;
                }
                case HelpNavigator.AssociateIndex:
                case HelpNavigator.KeywordIndex:
                    break;

                case HelpNavigator.TopicId:
                    try
                    {
                        htmlParam = int.Parse(param, CultureInfo.InvariantCulture);
                        return 15;
                    }
                    catch
                    {
                        return 2;
                    }
                    break;

                default:
                    return (int) command;
            }
            System.Windows.Forms.NativeMethods.HH_AKLINK hh_aklink = new System.Windows.Forms.NativeMethods.HH_AKLINK {
                pszKeywords = param,
                fIndexOnFail = true,
                fReserved = false
            };
            htmlParam = hh_aklink;
            if (command != HelpNavigator.KeywordIndex)
            {
                return 0x13;
            }
            return 13;
        }

        private static Uri Resolve(string partialUri)
        {
            Uri uri = null;
            if (!string.IsNullOrEmpty(partialUri))
            {
                try
                {
                    uri = new Uri(partialUri);
                }
                catch (UriFormatException)
                {
                }
                catch (ArgumentNullException)
                {
                }
            }
            if ((uri != null) && (uri.Scheme == "file"))
            {
                string localPath = System.Windows.Forms.NativeMethods.GetLocalPath(partialUri);
                new FileIOPermission(FileIOPermissionAccess.Read, localPath).Assert();
                try
                {
                    if (!System.IO.File.Exists(localPath))
                    {
                        uri = null;
                    }
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
            if (uri == null)
            {
                try
                {
                    uri = new Uri(new Uri(AppDomain.CurrentDomain.SetupInformation.ApplicationBase), partialUri);
                }
                catch (UriFormatException)
                {
                }
                catch (ArgumentNullException)
                {
                }
                if ((uri == null) || !(uri.Scheme == "file"))
                {
                    return uri;
                }
                string path = uri.LocalPath + uri.Fragment;
                new FileIOPermission(FileIOPermissionAccess.Read, path).Assert();
                try
                {
                    if (!System.IO.File.Exists(path))
                    {
                        uri = null;
                    }
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
            return uri;
        }

        public static void ShowHelp(Control parent, string url)
        {
            ShowHelp(parent, url, HelpNavigator.TableOfContents, null);
        }

        public static void ShowHelp(Control parent, string url, string keyword)
        {
            if ((keyword != null) && (keyword.Length != 0))
            {
                ShowHelp(parent, url, HelpNavigator.Topic, keyword);
            }
            else
            {
                ShowHelp(parent, url, HelpNavigator.TableOfContents, null);
            }
        }

        public static void ShowHelp(Control parent, string url, HelpNavigator navigator)
        {
            ShowHelp(parent, url, navigator, null);
        }

        public static void ShowHelp(Control parent, string url, HelpNavigator command, object parameter)
        {
            switch (GetHelpFileType(url))
            {
                case 2:
                    ShowHTML10Help(parent, url, command, parameter);
                    return;

                case 3:
                    ShowHTMLFile(parent, url, command, parameter);
                    return;
            }
        }

        public static void ShowHelpIndex(Control parent, string url)
        {
            ShowHelp(parent, url, HelpNavigator.Index, null);
        }

        private static void ShowHTML10Help(Control parent, string url, HelpNavigator command, object param)
        {
            HandleRef ref2;
            object obj2;
            System.Windows.Forms.IntSecurity.UnmanagedCode.Demand();
            Uri uri = null;
            string pszFile = url;
            uri = Resolve(url);
            if (uri != null)
            {
                pszFile = uri.AbsoluteUri;
            }
            if ((uri == null) || uri.IsFile)
            {
                StringBuilder lpszShortPath = new StringBuilder();
                string lpszLongPath = ((uri != null) && uri.IsFile) ? uri.LocalPath : url;
                uint cchBuffer = System.Windows.Forms.UnsafeNativeMethods.GetShortPathName(lpszLongPath, lpszShortPath, 0);
                if (cchBuffer > 0)
                {
                    lpszShortPath.Capacity = (int) cchBuffer;
                    cchBuffer = System.Windows.Forms.UnsafeNativeMethods.GetShortPathName(lpszLongPath, lpszShortPath, cchBuffer);
                    pszFile = lpszShortPath.ToString();
                }
            }
            if (parent != null)
            {
                ref2 = new HandleRef(parent, parent.Handle);
            }
            else
            {
                ref2 = new HandleRef(null, System.Windows.Forms.UnsafeNativeMethods.GetActiveWindow());
            }
            string str3 = param as string;
            if (str3 != null)
            {
                int uCommand = MapCommandToHTMLCommand(command, str3, out obj2);
                string dwData = obj2 as string;
                if (dwData != null)
                {
                    System.Windows.Forms.SafeNativeMethods.HtmlHelp(ref2, pszFile, uCommand, dwData);
                }
                else if (obj2 is int)
                {
                    System.Windows.Forms.SafeNativeMethods.HtmlHelp(ref2, pszFile, uCommand, (int) obj2);
                }
                else if (obj2 is System.Windows.Forms.NativeMethods.HH_FTS_QUERY)
                {
                    System.Windows.Forms.SafeNativeMethods.HtmlHelp(ref2, pszFile, uCommand, (System.Windows.Forms.NativeMethods.HH_FTS_QUERY) obj2);
                }
                else if (obj2 is System.Windows.Forms.NativeMethods.HH_AKLINK)
                {
                    System.Windows.Forms.SafeNativeMethods.HtmlHelp(System.Windows.Forms.NativeMethods.NullHandleRef, pszFile, 0, (string) null);
                    System.Windows.Forms.SafeNativeMethods.HtmlHelp(ref2, pszFile, uCommand, (System.Windows.Forms.NativeMethods.HH_AKLINK) obj2);
                }
                else
                {
                    System.Windows.Forms.SafeNativeMethods.HtmlHelp(ref2, pszFile, uCommand, (string) param);
                }
            }
            else if (param == null)
            {
                System.Windows.Forms.SafeNativeMethods.HtmlHelp(ref2, pszFile, MapCommandToHTMLCommand(command, null, out obj2), 0);
            }
            else if (param is System.Windows.Forms.NativeMethods.HH_POPUP)
            {
                System.Windows.Forms.SafeNativeMethods.HtmlHelp(ref2, pszFile, 14, (System.Windows.Forms.NativeMethods.HH_POPUP) param);
            }
            else if (param.GetType() == typeof(int))
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "param", "Integer" }));
            }
        }

        private static void ShowHTMLFile(Control parent, string url, HelpNavigator command, object param)
        {
            HandleRef ref2;
            string str;
            Uri uri = Resolve(url);
            if (uri == null)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("HelpInvalidURL", new object[] { url }), "url");
            }
            if (((str = uri.Scheme) != null) && ((str == "http") || (str == "https")))
            {
                new WebPermission(NetworkAccess.Connect, url).Demand();
            }
            else
            {
                System.Windows.Forms.IntSecurity.UnmanagedCode.Demand();
            }
            switch (command)
            {
                case HelpNavigator.Topic:
                    if ((param != null) && (param is string))
                    {
                        uri = new Uri(uri.ToString() + "#" + ((string) param));
                    }
                    break;
            }
            if (parent != null)
            {
                ref2 = new HandleRef(parent, parent.Handle);
            }
            else
            {
                ref2 = new HandleRef(null, System.Windows.Forms.UnsafeNativeMethods.GetActiveWindow());
            }
            System.Windows.Forms.UnsafeNativeMethods.ShellExecute_NoBFM(ref2, null, uri.ToString(), null, null, 1);
        }

        public static void ShowPopup(Control parent, string caption, Point location)
        {
            System.Windows.Forms.NativeMethods.HH_POPUP param = new System.Windows.Forms.NativeMethods.HH_POPUP();
            IntPtr ptr = Marshal.StringToCoTaskMemAuto(caption);
            try
            {
                param.pszText = ptr;
                param.idString = 0;
                param.pt = new System.Windows.Forms.NativeMethods.POINT(location.X, location.Y);
                param.clrBackground = Color.FromKnownColor(KnownColor.Window).ToArgb() & 0xffffff;
                ShowHTML10Help(parent, null, HelpNavigator.Topic, param);
            }
            finally
            {
                Marshal.FreeCoTaskMem(ptr);
            }
        }
    }
}

