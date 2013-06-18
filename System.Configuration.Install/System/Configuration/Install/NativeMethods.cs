namespace System.Configuration.Install
{
    using System;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        public const int INSTALLMESSAGE_ERROR = 0x1000000;

        [DllImport("msi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int MsiCreateRecord(int cParams);
        [DllImport("msi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int MsiProcessMessage(int hInstall, int messageType, int hRecord);
        [DllImport("msi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int MsiRecordSetInteger(int hRecord, int iField, int iValue);
        [DllImport("msi.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern int MsiRecordSetStringW(int hRecord, int iField, string szValue);
    }
}

