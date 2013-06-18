namespace Microsoft.InfoCards.Diagnostics
{
    using Microsoft.InfoCards;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.ServiceModel.Diagnostics;
    using System.Threading;

    internal static class InfoCardTrace
    {
        private const string InfoCardEventSource = "CardSpace 4.0.0.0";
        private static Dictionary<int, string> traceCodes;

        static InfoCardTrace()
        {
            Dictionary<int, string> dictionary = new Dictionary<int, string>(9);
            dictionary.Add(0xd0001, "GeneralInformation");
            dictionary.Add(0xd0002, "StoreLoading");
            dictionary.Add(0xd0003, "StoreBeginTransaction");
            dictionary.Add(0xd0004, "StoreCommitTransaction");
            dictionary.Add(0xd0005, "StoreRollbackTransaction");
            dictionary.Add(0xd0006, "StoreClosing");
            dictionary.Add(0xd0007, "StoreFailedToOpenStore");
            dictionary.Add(0xd0008, "StoreSignatureNotValid");
            dictionary.Add(0xd0009, "StoreDeleting");
            traceCodes = dictionary;
        }

        public static void Assert(bool condition, string format, params object[] parameters)
        {
            if (!condition)
            {
                string message = format;
                if ((parameters != null) && (parameters.Length != 0))
                {
                    message = string.Format(CultureInfo.InvariantCulture, format, parameters);
                }
                FailFast(message);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void Audit(EventCode code)
        {
            LogEvent(code, null, EventLogEntryType.Information);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void Audit(EventCode code, string message)
        {
            LogEvent(code, message, EventLogEntryType.Information);
        }

        private static string BuildMessage(InfoCardBaseException ie)
        {
            Exception innerException = ie;
            string str = innerException.Message + "\n";
            if (innerException.InnerException != null)
            {
                while (innerException.InnerException != null)
                {
                    str = str + string.Format(CultureInfo.CurrentUICulture, Microsoft.InfoCards.SR.GetString("InnerExceptionTraceFormat"), new object[] { innerException.InnerException.Message });
                    innerException = innerException.InnerException;
                }
                return (str + string.Format(CultureInfo.CurrentUICulture, Microsoft.InfoCards.SR.GetString("CallStackTraceFormat"), new object[] { ie.ToString() }));
            }
            if (!string.IsNullOrEmpty(Environment.StackTrace))
            {
                str = str + string.Format(CultureInfo.CurrentUICulture, Microsoft.InfoCards.SR.GetString("CallStackTraceFormat"), new object[] { Environment.StackTrace });
            }
            return str;
        }

        public static void CloseInvalidOutSafeHandle(SafeHandle handle)
        {
            Utility.CloseInvalidOutSafeHandle(handle);
        }

        [Conditional("DEBUG")]
        public static void DebugAssert(bool condition, string format, params object[] parameters)
        {
        }

        public static void FailFast(string message)
        {
            DiagnosticUtility.FailFast(message);
        }

        public static Guid GetActivityId()
        {
            return DiagnosticTrace.ActivityId;
        }

        private static string GetMsdnTraceCode(int traceCode)
        {
            return DiagnosticTrace.GenerateMsdnTraceCode("System.IdentityModel.Selectors", GetTraceString(traceCode));
        }

        private static string GetTraceString(int traceCode)
        {
            return traceCodes[traceCode];
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static bool IsFatal(Exception e)
        {
            return Fx.IsFatal(e);
        }

        private static void LogEvent(EventCode code, string message, EventLogEntryType type)
        {
            using (SafeEventLogHandle handle = SafeEventLogHandle.Construct())
            {
                string str = message;
                if (handle != null)
                {
                    if (string.IsNullOrEmpty(str))
                    {
                        str = Microsoft.InfoCards.SR.GetString("GeneralExceptionMessage");
                    }
                    IntPtr[] ptrArray = new IntPtr[1];
                    GCHandle handle2 = new GCHandle();
                    GCHandle handle3 = new GCHandle();
                    try
                    {
                        handle3 = GCHandle.Alloc(ptrArray, GCHandleType.Pinned);
                        handle2 = GCHandle.Alloc(str, GCHandleType.Pinned);
                        ptrArray[0] = handle2.AddrOfPinnedObject();
                        HandleRef strings = new HandleRef(handle, handle3.AddrOfPinnedObject());
                        SecurityIdentifier user = WindowsIdentity.GetCurrent().User;
                        byte[] binaryForm = new byte[user.BinaryLength];
                        user.GetBinaryForm(binaryForm, 0);
                        if (!ReportEvent(handle, (short) type, 1, (uint) code, binaryForm, 1, 0, strings, null))
                        {
                            Marshal.GetLastWin32Error();
                        }
                    }
                    finally
                    {
                        if (handle3.IsAllocated)
                        {
                            handle3.Free();
                        }
                        if (handle2.IsAllocated)
                        {
                            handle2.Free();
                        }
                    }
                }
            }
        }

        [DllImport("advapi32", EntryPoint="ReportEventW", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        private static extern bool ReportEvent([In] SafeHandle hEventLog, [In] short type, [In] ushort category, [In] uint eventID, [In] byte[] userSID, [In] short numStrings, [In] int dataLen, [In] HandleRef strings, [In] byte[] rawData);
        public static void SetActivityId(Guid activityId)
        {
            DiagnosticTrace.ActivityId = activityId;
        }

        public static bool ShouldTrace(TraceEventType type)
        {
            return DiagnosticUtility.ShouldTrace(type);
        }

        public static Exception ThrowHelperArgument(string message)
        {
            return DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(message);
        }

        public static Exception ThrowHelperArgumentNull(string err)
        {
            return DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(err);
        }

        public static Exception ThrowHelperArgumentNull(string err, string message)
        {
            return DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(err, message);
        }

        public static Exception ThrowHelperCritical(Exception e)
        {
            TraceAndLogException(e);
            return DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(e);
        }

        public static Exception ThrowHelperError(Exception e)
        {
            TraceAndLogException(e);
            return DiagnosticUtility.ExceptionUtility.ThrowHelperError(e);
        }

        public static Exception ThrowHelperErrorWithNoLogging(Exception e)
        {
            return DiagnosticUtility.ExceptionUtility.ThrowHelperError(e);
        }

        public static Exception ThrowHelperWarning(Exception e)
        {
            TraceAndLogException(e);
            return DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(e);
        }

        public static void ThrowInvalidArgumentConditional(bool condition, string argument)
        {
            if (condition)
            {
                throw ThrowHelperError(new InfoCardArgumentException(string.Format(CultureInfo.CurrentUICulture, Microsoft.InfoCards.SR.GetString("ServiceInvalidArgument"), new object[] { argument })));
            }
        }

        public static TimerCallback ThunkCallback(TimerCallback callback)
        {
            return Fx.ThunkCallback(callback);
        }

        public static WaitCallback ThunkCallback(WaitCallback callback)
        {
            return Fx.ThunkCallback(callback);
        }

        [Conditional("DEBUG")]
        public static void Trace(TraceEventType level, int traceCode)
        {
        }

        [Conditional("DEBUG")]
        public static void Trace(TraceEventType level, int traceCode, params object[] parameters)
        {
        }

        public static void TraceAndLogException(Exception e)
        {
            bool flag = false;
            bool flag2 = false;
            InfoCardBaseException ie = e as InfoCardBaseException;
            if (((ie != null) && !(ie is UserCancelledException)) && !ie.Logged)
            {
                flag = true;
            }
            if (flag)
            {
                for (Exception exception2 = ie.InnerException; exception2 != null; exception2 = exception2.InnerException)
                {
                    if (exception2 is UserCancelledException)
                    {
                        flag = false;
                        break;
                    }
                    if ((exception2 is InfoCardBaseException) && (exception2 as InfoCardBaseException).Logged)
                    {
                        flag2 = true;
                    }
                }
            }
            if (flag)
            {
                EventLogEntryType type = flag2 ? EventLogEntryType.Information : EventLogEntryType.Error;
                string message = ie.Message;
                if (!flag2)
                {
                    message = BuildMessage(ie);
                }
                LogEvent((EventCode) ie.NativeHResult, message, type);
            }
        }

        [Conditional("DEBUG")]
        public static void TraceCritical(int traceCode)
        {
        }

        [Conditional("DEBUG")]
        public static void TraceCritical(int traceCode, params object[] parameters)
        {
        }

        [Conditional("DEBUG")]
        public static void TraceDebug(string message)
        {
        }

        [Conditional("DEBUG")]
        public static void TraceDebug(string format, params object[] parameters)
        {
        }

        [Conditional("DEBUG")]
        public static void TraceError(int traceCode)
        {
        }

        [Conditional("DEBUG")]
        public static void TraceError(int traceCode, params object[] parameters)
        {
        }

        [Conditional("DEBUG")]
        public static void TraceException(Exception e)
        {
            Exception innerException = e;
            for (int i = 0; innerException != null; i++)
            {
                innerException = innerException.InnerException;
            }
        }

        [Conditional("DEBUG")]
        public static void TraceInfo(int traceCode)
        {
        }

        [Conditional("DEBUG")]
        public static void TraceInfo(int traceCode, params object[] parameters)
        {
        }

        [Conditional("DEBUG")]
        private static void TraceInternal(TraceEventType level, int traceCode, params object[] parameters)
        {
        }

        [Conditional("DEBUG")]
        public static void TraceVerbose(int traceCode)
        {
        }

        [Conditional("DEBUG")]
        public static void TraceVerbose(int traceCode, params object[] parameters)
        {
        }

        [Conditional("DEBUG")]
        public static void TraceWarning(int traceCode)
        {
        }

        [Conditional("DEBUG")]
        public static void TraceWarning(int traceCode, params object[] parameters)
        {
        }

        public static bool ShouldTraceCritical
        {
            get
            {
                return DiagnosticUtility.ShouldTraceCritical;
            }
        }

        public static bool ShouldTraceError
        {
            get
            {
                return DiagnosticUtility.ShouldTraceError;
            }
        }

        public static bool ShouldTraceInformation
        {
            get
            {
                return DiagnosticUtility.ShouldTraceInformation;
            }
        }

        public static bool ShouldTraceVerbose
        {
            get
            {
                return DiagnosticUtility.ShouldTraceVerbose;
            }
        }

        public static bool ShouldTraceWarning
        {
            get
            {
                return DiagnosticUtility.ShouldTraceWarning;
            }
        }

        internal class SafeEventLogHandle : SafeHandle
        {
            private SafeEventLogHandle() : base(IntPtr.Zero, true)
            {
            }

            public static InfoCardTrace.SafeEventLogHandle Construct()
            {
                InfoCardTrace.SafeEventLogHandle handle = RegisterEventSource(null, "CardSpace 4.0.0.0");
                if ((handle == null) || handle.IsInvalid)
                {
                    Marshal.GetLastWin32Error();
                }
                return handle;
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("advapi32", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            private static extern bool DeregisterEventSource(IntPtr eventLog);
            [DllImport("advapi32", EntryPoint="RegisterEventSourceW", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            private static extern InfoCardTrace.SafeEventLogHandle RegisterEventSource(string uncServerName, string sourceName);
            protected override bool ReleaseHandle()
            {
                return DeregisterEventSource(base.handle);
            }

            public override bool IsInvalid
            {
                get
                {
                    return (IntPtr.Zero == base.handle);
                }
            }
        }

        private static class TraceCode
        {
            public const int GeneralInformation = 0xd0001;
            public const int IdentityModelSelectors = 0xd0000;
            public const int StoreBeginTransaction = 0xd0003;
            public const int StoreClosing = 0xd0006;
            public const int StoreCommitTransaction = 0xd0004;
            public const int StoreDeleting = 0xd0009;
            public const int StoreFailedToOpenStore = 0xd0007;
            public const int StoreLoading = 0xd0002;
            public const int StoreRollbackTransaction = 0xd0005;
            public const int StoreSignatureNotValid = 0xd0008;
        }
    }
}

