namespace System.EnterpriseServices
{
    using System;
    using System.EnterpriseServices.Thunk;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Text;

    internal class Util
    {
        internal const int COMADMIN_E_OBJECTERRORS = -2146368511;
        internal const int CONTEXT_E_ABORTED = -2147164158;
        internal const int CONTEXT_E_ABORTING = -2147164157;
        internal const int CONTEXT_E_NOCONTEXT = -2147164156;
        internal const int CONTEXT_E_TMNOTAVAILABLE = -2147164145;
        internal const int DISP_E_UNKNOWNNAME = -2147352570;
        internal const int E_NOINTERFACE = -2147467262;
        internal const int E_UNEXPECTED = -2147418113;
        internal const int ERROR_NO_TOKEN = 0x3f0;
        internal const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
        internal const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        internal const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        internal static readonly Guid GUID_NULL = new Guid("00000000-0000-0000-0000-000000000000");
        internal static readonly Guid IID_IObjectContext = new Guid("51372AE0-CAE7-11CF-BE81-00AA00A2FA25");
        internal static readonly Guid IID_ISecurityCallContext = new Guid("CAFC823E-B441-11D1-B82B-0000F8757E2A");
        internal static readonly Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");
        internal const int MB_ABORTRETRYIGNORE = 2;
        internal const int MB_ICONEXCLAMATION = 0x30;
        internal const int REGDB_E_CLASSNOTREG = -2147221164;

        [DllImport("ole32.dll", PreserveSig=false)]
        internal static extern void CoGetCallContext([MarshalAs(UnmanagedType.LPStruct)] Guid riid, [MarshalAs(UnmanagedType.Interface)] out ISecurityCallContext iface);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        internal static extern int FormatMessage(int dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId, StringBuilder lpBuffer, int nSize, int arguments);
        internal static string GetErrorString(int hr)
        {
            StringBuilder lpBuffer = new StringBuilder(0x400);
            if (FormatMessage(0x3200, IntPtr.Zero, hr, 0, lpBuffer, lpBuffer.Capacity + 1, 0) == 0)
            {
                return null;
            }
            int length = lpBuffer.Length;
            while (length > 0)
            {
                char ch = lpBuffer[length - 1];
                if ((ch > ' ') && (ch != '.'))
                {
                    break;
                }
                length--;
            }
            return lpBuffer.ToString(0, length);
        }

        [DllImport("mtxex.dll", CallingConvention=CallingConvention.Cdecl)]
        internal static extern int GetObjectContext([MarshalAs(UnmanagedType.Interface)] out System.EnterpriseServices.IObjectContext pCtx);
        [DllImport("oleaut32.dll")]
        internal static extern int LoadRegTypeLib([In, MarshalAs(UnmanagedType.LPStruct)] Guid lidID, short wVerMajor, short wVerMinor, int lcid, [MarshalAs(UnmanagedType.Interface)] out object pptlib);
        [DllImport("oleaut32.dll")]
        internal static extern int LoadTypeLibEx([In, MarshalAs(UnmanagedType.LPWStr)] string str, int regKind, out IntPtr pptlib);
        [DllImport("user32.dll")]
        internal static extern int MessageBox(int hWnd, string lpText, string lpCaption, int type);
        [DllImport("kernel32.dll")]
        internal static extern void OutputDebugString(string msg);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll")]
        internal static extern bool QueryPerformanceCounter(out long count);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll")]
        internal static extern bool QueryPerformanceFrequency(out long count);
        [DllImport("oleaut32.dll")]
        internal static extern int RegisterTypeLib(IntPtr pptlib, [In, MarshalAs(UnmanagedType.LPWStr)] string str, [In, MarshalAs(UnmanagedType.LPWStr)] string help);
        [DllImport("oleaut32.dll", PreserveSig=false)]
        internal static extern void UnRegisterTypeLib([In, MarshalAs(UnmanagedType.LPStruct)] Guid libID, short wVerMajor, short wVerMinor, int lcid, System.Runtime.InteropServices.ComTypes.SYSKIND syskind);

        internal static bool ExtendedLifetime
        {
            get
            {
                return ((Proxy.GetManagedExts() & 1) != 0);
            }
        }
    }
}

