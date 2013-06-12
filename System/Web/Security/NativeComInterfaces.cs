namespace System.Web.Security
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity, ComVisible(false)]
    internal static class NativeComInterfaces
    {
        internal const int ADS_ESCAPEDMODE_OFF = 3;
        internal const int ADS_ESCAPEDMODE_ON = 2;
        internal const int ADS_FORMAT_PROVIDER = 10;
        internal const int ADS_FORMAT_SERVER = 9;
        internal const int ADS_FORMAT_X500_DN = 7;
        internal const int ADS_SETTYPE_DN = 4;
        internal const int ADS_SETTYPE_FULL = 1;

        [ComImport, Guid("9068270b-0939-11d1-8be1-00c04fd8d503"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
        internal interface IAdsLargeInteger
        {
            long HighPart { [SuppressUnmanagedCodeSecurity] get; [SuppressUnmanagedCodeSecurity] set; }
            long LowPart { [SuppressUnmanagedCodeSecurity] get; [SuppressUnmanagedCodeSecurity] set; }
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("D592AED4-F420-11D0-A36E-00C04FB950DC")]
        internal interface IAdsPathname
        {
            [SuppressUnmanagedCodeSecurity]
            int Set([In, MarshalAs(UnmanagedType.BStr)] string bstrADsPath, [In, MarshalAs(UnmanagedType.U4)] int lnSetType);
            int SetDisplayType([In, MarshalAs(UnmanagedType.U4)] int lnDisplayType);
            [return: MarshalAs(UnmanagedType.BStr)]
            [SuppressUnmanagedCodeSecurity]
            string Retrieve([In, MarshalAs(UnmanagedType.U4)] int lnFormatType);
            [return: MarshalAs(UnmanagedType.U4)]
            int GetNumElements();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetElement([In, MarshalAs(UnmanagedType.U4)] int lnElementIndex);
            void AddLeafElement([In, MarshalAs(UnmanagedType.BStr)] string bstrLeafElement);
            void RemoveLeafElement();
            [return: MarshalAs(UnmanagedType.Interface)]
            object CopyPath();
            [return: MarshalAs(UnmanagedType.BStr)]
            [SuppressUnmanagedCodeSecurity]
            string GetEscapedElement([In, MarshalAs(UnmanagedType.U4)] int lnReserved, [In, MarshalAs(UnmanagedType.BStr)] string bstrInStr);
            int EscapedMode { get; [SuppressUnmanagedCodeSecurity] set; }
        }

        [ComImport, Guid("927971f5-0939-11d1-8be1-00c04fd8d503")]
        internal class LargeInteger
        {
        }

        [ComImport, Guid("080d0d78-f421-11d0-a36e-00c04fb950dc")]
        internal class Pathname
        {
        }
    }
}

