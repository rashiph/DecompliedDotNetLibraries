namespace Microsoft.Win32
{
    using System;
    using System.Diagnostics.Eventing;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [SuppressUnmanagedCodeSecurity, SecurityCritical]
    internal static class UnsafeNativeMethods
    {
        internal const string ADVAPI32 = "advapi32.dll";
        internal const int ERROR_ARITHMETIC_OVERFLOW = 0x216;
        internal const int ERROR_MORE_DATA = 0xea;
        internal const int ERROR_NOT_ENOUGH_MEMORY = 8;

        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern uint EventActivityIdControl([In] int ControlCode, [In, Out] ref Guid ActivityId);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern unsafe uint EventRegister([In] ref Guid providerId, [In] EtwEnableCallback enableCallback, [In] void* callbackContext, [In, Out] ref long registrationHandle);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern int EventUnregister([In] long registrationHandle);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern unsafe uint EventWrite([In] long registrationHandle, [In] ref EventDescriptorInternal eventDescriptor, [In] uint userDataCount, [In] void* userData);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern unsafe uint EventWrite([In] long registrationHandle, [In] EventDescriptorInternal* eventDescriptor, [In] uint userDataCount, [In] void* userData);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern unsafe uint EventWriteString([In] long registrationHandle, [In] byte level, [In] long keywords, [In] char* message);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern unsafe uint EventWriteTransfer([In] long registrationHandle, [In] ref EventDescriptorInternal eventDescriptor, [In] ref Guid activityId, [In] ref Guid relatedActivityId, [In] uint userDataCount, [In] void* userData);
        [return: MarshalAs(UnmanagedType.Bool)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern bool FreeLibrary(IntPtr hModule);
        [DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
        internal static extern int GetDynamicTimeZoneInformation(out Win32Native.DynamicTimeZoneInformation lpDynamicTimeZoneInformation);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
        internal static extern bool GetFileMUIPath(int flags, [MarshalAs(UnmanagedType.LPWStr)] string filePath, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder language, ref int languageLength, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder fileMuiPath, ref int fileMuiPathLength, ref long enumerator);
        [DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
        internal static extern int GetTimeZoneInformation(out Win32Native.TimeZoneInformation lpTimeZoneInformation);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern SafeLibraryHandle LoadLibraryEx(string libFilename, IntPtr reserved, int flags);
        [DllImport("user32.dll", EntryPoint="LoadStringW", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        internal static extern int LoadString(SafeLibraryHandle handle, int id, StringBuilder buffer, int bufferLength);

        internal unsafe delegate void EtwEnableCallback([In] ref Guid sourceId, [In] int isEnabled, [In] byte level, [In] long matchAnyKeywords, [In] long matchAllKeywords, [In] UnsafeNativeMethods.EVENT_FILTER_DESCRIPTOR* filterData, [In] void* callbackContext);

        [StructLayout(LayoutKind.Explicit)]
        internal struct EVENT_FILTER_DESCRIPTOR
        {
            [FieldOffset(0)]
            public unsafe byte* Ptr;
            [FieldOffset(8)]
            public int Size;
            [FieldOffset(12)]
            public int Type;
        }
    }
}

