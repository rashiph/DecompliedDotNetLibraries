namespace System.Runtime.Interop
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.ComponentModel;
    using System.Diagnostics.Eventing;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        public const string ADVAPI32 = "advapi32.dll";
        public const int ERROR_ARITHMETIC_OVERFLOW = 0x216;
        public const int ERROR_INVALID_HANDLE = 6;
        public const int ERROR_MORE_DATA = 0xea;
        public const int ERROR_NOT_ENOUGH_MEMORY = 8;
        public const string KERNEL32 = "kernel32.dll";

        [SecurityCritical, DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern SafeWaitHandle CreateWaitableTimer(IntPtr mustBeZero, bool manualReset, string timerName);
        [SecurityCritical, DllImport("kernel32.dll")]
        internal static extern void DebugBreak();
        [SecurityCritical, DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern uint EventActivityIdControl([In] int ControlCode, [In, Out] ref Guid ActivityId);
        [SecurityCritical, DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern unsafe uint EventRegister([In] ref Guid providerId, [In] EtwEnableCallback enableCallback, [In] void* callbackContext, [In, Out] ref long registrationHandle);
        [SecurityCritical, DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern uint EventUnregister([In] long registrationHandle);
        [SecurityCritical, DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern unsafe uint EventWrite([In] long registrationHandle, [In] ref System.Diagnostics.Eventing.EventDescriptor eventDescriptor, [In] uint userDataCount, [In] EventData* userData);
        [SecurityCritical, DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern unsafe uint EventWriteString([In] long registrationHandle, [In] byte level, [In] long keywords, [In] char* message);
        [SecurityCritical, DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern unsafe uint EventWriteTransfer([In] long registrationHandle, [In] ref System.Diagnostics.Eventing.EventDescriptor eventDescriptor, [In] ref Guid activityId, [In] ref Guid relatedActivityId, [In] uint userDataCount, [In] EventData* userData);
        [SecurityCritical]
        internal static string GetComputerName(ComputerNameFormat nameType)
        {
            int size = 0;
            if (!GetComputerNameEx(nameType, null, ref size))
            {
                int error = Marshal.GetLastWin32Error();
                if (error != 0xea)
                {
                    throw Fx.Exception.AsError(new Win32Exception(error));
                }
            }
            if (size < 0)
            {
                Fx.AssertAndThrow("GetComputerName returned an invalid length: " + size);
            }
            StringBuilder lpBuffer = new StringBuilder(size);
            if (!GetComputerNameEx(nameType, lpBuffer, ref size))
            {
                int num3 = Marshal.GetLastWin32Error();
                throw Fx.Exception.AsError(new Win32Exception(num3));
            }
            return lpBuffer.ToString();
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical, DllImport("kernel32.dll", SetLastError=true)]
        private static extern bool GetComputerNameEx([In] ComputerNameFormat nameType, [In, Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpBuffer, [In, Out] ref int size);
        [SecurityCritical, DllImport("kernel32.dll")]
        public static extern uint GetSystemTimeAdjustment(out int adjustment, out uint increment, out uint adjustmentDisabled);
        [SecurityCritical, DllImport("kernel32.dll", SetLastError=true)]
        public static extern void GetSystemTimeAsFileTime(out long time);
        [SecurityCritical, DllImport("kernel32.dll")]
        internal static extern bool IsDebuggerPresent();
        [SecurityCritical, DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern void OutputDebugString(string lpOutputString);
        [SecurityCritical, DllImport("kernel32.dll", SetLastError=true)]
        public static extern int QueryPerformanceCounter(out long time);
        [SecurityCritical, DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern System.Runtime.Interop.SafeEventLogWriteHandle RegisterEventSource(string uncServerName, string sourceName);
        [SecurityCritical, DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool ReportEvent(SafeHandle hEventLog, ushort type, ushort category, uint eventID, byte[] userSID, ushort numStrings, uint dataLen, HandleRef strings, byte[] rawData);
        [SecurityCritical, DllImport("kernel32.dll", ExactSpelling=true)]
        public static extern bool SetWaitableTimer(SafeWaitHandle handle, ref long dueTime, int period, IntPtr mustBeZero, IntPtr mustBeZeroAlso, bool resume);

        [SecurityCritical]
        internal unsafe delegate void EtwEnableCallback([In] ref Guid sourceId, [In] int isEnabled, [In] byte level, [In] long matchAnyKeywords, [In] long matchAllKeywords, [In] void* filterData, [In] void* callbackContext);

        [StructLayout(LayoutKind.Explicit, Size=0x10)]
        public struct EventData
        {
            [FieldOffset(0)]
            internal ulong DataPointer;
            [FieldOffset(12)]
            internal int Reserved;
            [FieldOffset(8)]
            internal uint Size;
        }
    }
}

