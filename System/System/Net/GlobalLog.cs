namespace System.Net
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.ConstrainedExecution;

    internal static class GlobalLog
    {
        private static BaseLoggingObject Logobject = LoggingInitialize();

        [Conditional("TRAVE")]
        public static void AddToArray(string msg)
        {
        }

        [ReliabilityContract(Consistency.MayCorruptAppDomain, Cer.None), Conditional("DEBUG"), Conditional("_FORCE_ASSERTS")]
        public static void Assert(string message)
        {
        }

        [Conditional("_FORCE_ASSERTS"), ReliabilityContract(Consistency.MayCorruptAppDomain, Cer.None), Conditional("DEBUG")]
        public static void Assert(string message, string detailMessage)
        {
            try
            {
                Logobject.DumpArray(false);
            }
            finally
            {
                UnsafeNclNativeMethods.DebugBreak();
                Debugger.Break();
            }
        }

        [ReliabilityContract(Consistency.MayCorruptAppDomain, Cer.None), Conditional("DEBUG"), Conditional("_FORCE_ASSERTS")]
        public static void Assert(bool condition, string messageFormat, params object[] data)
        {
            if (!condition)
            {
                string str = string.Format(CultureInfo.InvariantCulture, messageFormat, data);
                if (str.IndexOf('|') != -1)
                {
                    int length = str.Length;
                }
            }
        }

        [Conditional("DEBUG")]
        internal static void DebugAddRequest(HttpWebRequest request, Connection connection, int flags)
        {
        }

        [Conditional("DEBUG")]
        internal static void DebugRemoveRequest(HttpWebRequest request)
        {
        }

        [Conditional("DEBUG")]
        internal static void DebugUpdateRequest(HttpWebRequest request, Connection connection, int flags)
        {
        }

        [Conditional("TRAVE")]
        public static void Dump(byte[] buffer)
        {
        }

        [Conditional("TRAVE")]
        public static void Dump(byte[] buffer, int length)
        {
        }

        [Conditional("TRAVE")]
        public static void Dump(byte[] buffer, int offset, int length)
        {
        }

        [Conditional("TRAVE")]
        public static void Dump(IntPtr buffer, int offset, int length)
        {
        }

        [Conditional("TRAVE")]
        public static void DumpArray()
        {
        }

        [Conditional("TRAVE")]
        public static void Enter(string func)
        {
        }

        [Conditional("TRAVE")]
        public static void Enter(string func, string parms)
        {
        }

        [Conditional("TRAVE")]
        public static void Ignore(object msg)
        {
        }

        [Conditional("TRAVE")]
        public static void Leave(string func)
        {
        }

        [Conditional("TRAVE")]
        public static void Leave(string func, bool returnval)
        {
        }

        [Conditional("TRAVE")]
        public static void Leave(string func, int returnval)
        {
        }

        [Conditional("TRAVE")]
        public static void Leave(string func, string result)
        {
        }

        [Conditional("TRAVE")]
        public static void LeaveException(string func, Exception exception)
        {
        }

        [ReliabilityContract(Consistency.MayCorruptAppDomain, Cer.None)]
        private static BaseLoggingObject LoggingInitialize()
        {
            return new BaseLoggingObject();
        }

        [ReliabilityContract(Consistency.MayCorruptAppDomain, Cer.None), Conditional("TRAVE")]
        public static void Print(string msg)
        {
        }

        [Conditional("TRAVE")]
        public static void PrintHex(string msg, object value)
        {
        }

        [Conditional("DEBUG"), ReliabilityContract(Consistency.MayCorruptAppDomain, Cer.None)]
        internal static void SetThreadSource(ThreadKinds source)
        {
        }

        [ReliabilityContract(Consistency.MayCorruptAppDomain, Cer.None), Conditional("DEBUG")]
        internal static void ThreadContract(ThreadKinds kind, string errorMsg)
        {
        }

        [Conditional("DEBUG"), ReliabilityContract(Consistency.MayCorruptAppDomain, Cer.None)]
        internal static void ThreadContract(ThreadKinds kind, ThreadKinds allowedSources, string errorMsg)
        {
            if (((kind & ThreadKinds.SourceMask) != ThreadKinds.Unknown) || ((allowedSources & ThreadKinds.SourceMask) != allowedSources))
            {
                throw new InternalException();
            }
            ThreadKinds currentThreadKind = CurrentThreadKind;
        }

        internal static ThreadKinds CurrentThreadKind
        {
            get
            {
                return ThreadKinds.Unknown;
            }
        }
    }
}

