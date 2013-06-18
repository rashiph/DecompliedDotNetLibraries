namespace System.Windows.Forms.Internal
{
    using System;
    using System.Internal;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal static class IntSafeNativeMethods
    {
        public static IntPtr CreatePen(int fnStyle, int nWidth, int crColor)
        {
            return System.Internal.HandleCollector.Add(IntCreatePen(fnStyle, nWidth, crColor), CommonHandles.GDI);
        }

        public static IntPtr CreateRectRgn(int x1, int y1, int x2, int y2)
        {
            return System.Internal.HandleCollector.Add(IntCreateRectRgn(x1, y1, x2, y2), CommonHandles.GDI);
        }

        public static IntPtr CreateSolidBrush(int crColor)
        {
            return System.Internal.HandleCollector.Add(IntCreateSolidBrush(crColor), CommonHandles.GDI);
        }

        public static IntPtr ExtCreatePen(int fnStyle, int dwWidth, IntNativeMethods.LOGBRUSH lplb, int dwStyleCount, int[] lpStyle)
        {
            return System.Internal.HandleCollector.Add(IntExtCreatePen(fnStyle, dwWidth, lplb, dwStyleCount, lpStyle), CommonHandles.GDI);
        }

        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool GdiFlush();
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int GetUserDefaultLCID();
        [DllImport("gdi32.dll", EntryPoint="CreatePen", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr IntCreatePen(int fnStyle, int nWidth, int crColor);
        [DllImport("gdi32.dll", EntryPoint="CreateRectRgn", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntPtr IntCreateRectRgn(int x1, int y1, int x2, int y2);
        [DllImport("gdi32.dll", EntryPoint="CreateSolidBrush", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr IntCreateSolidBrush(int crColor);
        [DllImport("gdi32.dll", EntryPoint="ExtCreatePen", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr IntExtCreatePen(int fnStyle, int dwWidth, IntNativeMethods.LOGBRUSH lplb, int dwStyleCount, [MarshalAs(UnmanagedType.LPArray)] int[] lpStyle);

        public sealed class CommonHandles
        {
            public static readonly int EMF = System.Internal.HandleCollector.RegisterType("EnhancedMetaFile", 20, 500);
            public static readonly int GDI = System.Internal.HandleCollector.RegisterType("GDI", 90, 50);
            public static readonly int HDC = System.Internal.HandleCollector.RegisterType("HDC", 100, 2);
        }
    }
}

