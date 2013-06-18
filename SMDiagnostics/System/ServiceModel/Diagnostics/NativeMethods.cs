namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        private const string ADVAPI32 = "advapi32.dll";
        private const string KERNEL32 = "kernel32.dll";

        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern SafeEventLogWriteHandle RegisterEventSource(string uncServerName, string sourceName);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool ReportEvent(SafeHandle hEventLog, ushort type, ushort category, uint eventID, byte[] userSID, ushort numStrings, uint dataLen, HandleRef strings, byte[] rawData);
    }
}

