namespace System.Design
{
    using System;
    using System.Internal;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        [DllImport("comctl32.dll", ExactSpelling=true)]
        private static extern bool _TrackMouseEvent(System.Design.NativeMethods.TRACKMOUSEEVENT tme);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);
        public static IntPtr CreatePen(int nStyle, int nWidth, int crColor)
        {
            return System.Internal.HandleCollector.Add(IntCreatePen(nStyle, nWidth, crColor), System.Design.NativeMethods.CommonHandles.GDI);
        }

        [DllImport("gdi32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr CreateSolidBrush(int crColor);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool DeleteObject(HandleRef hObject);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int DrawText(HandleRef hDC, string lpszString, int nCount, ref System.Design.NativeMethods.RECT lpRect, int nFormat);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int GetCurrentProcessId();
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int GetMessagePos();
        [DllImport("gdi32.dll", CharSet=CharSet.Auto)]
        public static extern bool GetTextMetrics(HandleRef hdc, System.Design.NativeMethods.TEXTMETRIC tm);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int GetTickCount();
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int GetWindowTextLength(HandleRef hWnd);
        [DllImport("gdi32.dll", EntryPoint="CreatePen", CharSet=CharSet.Auto, ExactSpelling=true)]
        private static extern IntPtr IntCreatePen(int nStyle, int nWidth, int crColor);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool IsChild(HandleRef parent, HandleRef child);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool Rectangle(HandleRef hdc, int left, int top, int right, int bottom);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool RedrawWindow(IntPtr hwnd, System.Design.NativeMethods.COMRECT rcUpdate, IntPtr hrgnUpdate, int flags);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int RegisterWindowMessage(string msg);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool RoundRect(HandleRef hDC, int left, int top, int right, int bottom, int width, int height);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr SelectObject(HandleRef hDC, HandleRef hObject);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int SetBkColor(HandleRef hDC, int clr);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int SetROP2(HandleRef hDC, int nDrawMode);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int SetWindowTheme(IntPtr hWnd, string subAppName, string subIdList);
        public static bool TrackMouseEvent(System.Design.NativeMethods.TRACKMOUSEEVENT tme)
        {
            return _TrackMouseEvent(tme);
        }
    }
}

