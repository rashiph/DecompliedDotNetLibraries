namespace System.Drawing.Design
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal class UnsafeNativeMethods
    {
        public const int OBJID_CLIENT = -4;

        private UnsafeNativeMethods()
        {
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int ClientToScreen(HandleRef hWnd, [In, Out] System.Drawing.Design.NativeMethods.POINT pt);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GetFocus();
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern void NotifyWinEvent(int winEvent, HandleRef hwnd, int objType, int objID);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int ScreenToClient(HandleRef hWnd, [In, Out] System.Drawing.Design.NativeMethods.POINT pt);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr SetFocus(HandleRef hWnd);
    }
}

