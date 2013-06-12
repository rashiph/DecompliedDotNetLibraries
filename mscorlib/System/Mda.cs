namespace System
{
    using System.Runtime.CompilerServices;
    using System.Security;

    internal static class Mda
    {
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void DateTimeInvalidLocalFormat();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void FireInvalidGCHandleCookieProbe(IntPtr cookie);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool IsInvalidGCHandleCookieProbeEnabled();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool IsStreamWriterBufferedDataLostCaptureAllocatedCallStack();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool IsStreamWriterBufferedDataLostEnabled();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void MemberInfoCacheCreation();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void ReportErrorSafeHandleRelease(Exception ex);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void ReportStreamWriterBufferedDataLost(string text);

        internal static class StreamWriterBufferedDataLost
        {
            private static int _captureAllocatedCallStackState;
            private static int _enabledState;

            [SecuritySafeCritical]
            internal static void ReportError(string text)
            {
                Mda.ReportStreamWriterBufferedDataLost(text);
            }

            internal static bool CaptureAllocatedCallStack
            {
                [SecuritySafeCritical]
                get
                {
                    if (_captureAllocatedCallStackState == 0)
                    {
                        if (Mda.IsStreamWriterBufferedDataLostCaptureAllocatedCallStack())
                        {
                            _captureAllocatedCallStackState = 1;
                        }
                        else
                        {
                            _captureAllocatedCallStackState = 2;
                        }
                    }
                    return (_captureAllocatedCallStackState == 1);
                }
            }

            internal static bool Enabled
            {
                [SecuritySafeCritical]
                get
                {
                    if (_enabledState == 0)
                    {
                        if (Mda.IsStreamWriterBufferedDataLostEnabled())
                        {
                            _enabledState = 1;
                        }
                        else
                        {
                            _enabledState = 2;
                        }
                    }
                    return (_enabledState == 1);
                }
            }
        }
    }
}

