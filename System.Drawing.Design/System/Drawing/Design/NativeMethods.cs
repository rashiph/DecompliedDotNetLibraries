namespace System.Drawing.Design
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    internal class NativeMethods
    {
        public const int CC_ANYCOLOR = 0x100;
        public const int CC_ENABLEHOOK = 0x10;
        public const int CC_ENABLETEMPLATE = 0x20;
        public const int CC_ENABLETEMPLATEHANDLE = 0x40;
        public const int CC_FULLOPEN = 2;
        public const int CC_PREVENTFULLOPEN = 4;
        public const int CC_SHOWHELP = 8;
        public const int CC_SOLIDCOLOR = 0x80;
        public const int EC_LEFTMARGIN = 1;
        public const int EC_RIGHTMARGIN = 2;
        public const int EC_USEFONTINFO = 0xffff;
        public const int EM_CANUNDO = 0xc6;
        public const int EM_CHARFROMPOS = 0xd7;
        public const int EM_EMPTYUNDOBUFFER = 0xcd;
        public const int EM_FMTLINES = 200;
        public const int EM_GETFIRSTVISIBLELINE = 0xce;
        public const int EM_GETHANDLE = 0xbd;
        public const int EM_GETLIMITTEXT = 0xd5;
        public const int EM_GETLINE = 0xc4;
        public const int EM_GETLINECOUNT = 0xba;
        public const int EM_GETMARGINS = 0xd4;
        public const int EM_GETMODIFY = 0xb8;
        public const int EM_GETPASSWORDCHAR = 210;
        public const int EM_GETRECT = 0xb2;
        public const int EM_GETSEL = 0xb0;
        public const int EM_GETTHUMB = 190;
        public const int EM_GETWORDBREAKPROC = 0xd1;
        public const int EM_LIMITTEXT = 0xc5;
        public const int EM_LINEFROMCHAR = 0xc9;
        public const int EM_LINEINDEX = 0xbb;
        public const int EM_LINELENGTH = 0xc1;
        public const int EM_LINESCROLL = 0xb6;
        public const int EM_POSFROMCHAR = 0xd6;
        public const int EM_REPLACESEL = 0xc2;
        public const int EM_SCROLL = 0xb5;
        public const int EM_SCROLLCARET = 0xb7;
        public const int EM_SETHANDLE = 0xbc;
        public const int EM_SETLIMITTEXT = 0xc5;
        public const int EM_SETMARGINS = 0xd3;
        public const int EM_SETMODIFY = 0xb9;
        public const int EM_SETPASSWORDCHAR = 0xcc;
        public const int EM_SETREADONLY = 0xcf;
        public const int EM_SETRECT = 0xb3;
        public const int EM_SETRECTNP = 180;
        public const int EM_SETSEL = 0xb1;
        public const int EM_SETTABSTOPS = 0xcb;
        public const int EM_SETWORDBREAKPROC = 0xd0;
        public const int EM_UNDO = 0xc7;
        public const int IDABORT = 3;
        public const int IDCANCEL = 2;
        public const int IDCLOSE = 8;
        public const int IDHELP = 9;
        public const int IDIGNORE = 5;
        public const int IDNO = 7;
        public const int IDOK = 1;
        public const int IDRETRY = 4;
        public const int IDYES = 6;
        public static IntPtr InvalidIntPtr = ((IntPtr) (-1));
        public const int SWP_ASYNCWINDOWPOS = 0x4000;
        public const int SWP_DEFERERASE = 0x2000;
        public const int SWP_DRAWFRAME = 0x20;
        public const int SWP_FRAMECHANGED = 0x20;
        public const int SWP_HIDEWINDOW = 0x80;
        public const int SWP_NOACTIVATE = 0x10;
        public const int SWP_NOCOPYBITS = 0x100;
        public const int SWP_NOMOVE = 2;
        public const int SWP_NOOWNERZORDER = 0x200;
        public const int SWP_NOREDRAW = 8;
        public const int SWP_NOREPOSITION = 0x200;
        public const int SWP_NOSENDCHANGING = 0x400;
        public const int SWP_NOSIZE = 1;
        public const int SWP_NOZORDER = 4;
        public const int SWP_SHOWWINDOW = 0x40;
        public const int WM_COMMAND = 0x111;
        public const int WM_INITDIALOG = 0x110;

        private NativeMethods()
        {
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool EnableWindow(IntPtr hWnd, bool enable);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GetDlgItem(IntPtr hWnd, int nIDDlgItem);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int GetDlgItemInt(IntPtr hWnd, int nIDDlgItem, bool[] err, bool signed);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendDlgItemMessage(IntPtr hDlg, int nIDDlgItem, int Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);

        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int x;
            public int y;
            public POINT()
            {
            }

            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public class Util
        {
            private Util()
            {
            }

            public static int HIWORD(int n)
            {
                return ((n >> 0x10) & 0xffff);
            }

            public static int LOWORD(int n)
            {
                return (n & 0xffff);
            }

            [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
            private static extern int lstrlen(string s);
            public static int MAKELONG(int low, int high)
            {
                return ((high << 0x10) | (low & 0xffff));
            }

            public static int MAKELPARAM(int low, int high)
            {
                return ((high << 0x10) | (low & 0xffff));
            }

            [DllImport("user32.dll", CharSet=CharSet.Auto)]
            internal static extern int RegisterWindowMessage(string msg);
            public static int SignedHIWORD(int n)
            {
                int num = (short) ((n >> 0x10) & 0xffff);
                num = num << 0x10;
                return (num >> 0x10);
            }

            public static int SignedLOWORD(int n)
            {
                int num = (short) (n & 0xffff);
                num = num << 0x10;
                return (num >> 0x10);
            }
        }
    }
}

