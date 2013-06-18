namespace System.Web
{
    using System;
    using System.Runtime;
    using System.Web.Hosting;

    internal static class EtwTrace
    {
        private static int _traceFlags = 0;
        private static int _traceLevel = 0;
        private static EtwWorkerRequestType s_WrType = EtwWorkerRequestType.Undefined;

        internal static int InferVerbosity(IntegratedTraceType traceType)
        {
            switch (traceType)
            {
                case IntegratedTraceType.TraceWrite:
                    return 5;

                case IntegratedTraceType.TraceWarn:
                    return 3;

                case IntegratedTraceType.DiagCritical:
                    return 1;

                case IntegratedTraceType.DiagError:
                    return 2;

                case IntegratedTraceType.DiagWarning:
                    return 3;

                case IntegratedTraceType.DiagInfo:
                    return 4;

                case IntegratedTraceType.DiagVerbose:
                    return 5;

                case IntegratedTraceType.DiagStart:
                    return 0;

                case IntegratedTraceType.DiagStop:
                    return 0;

                case IntegratedTraceType.DiagSuspend:
                    return 0;

                case IntegratedTraceType.DiagResume:
                    return 0;

                case IntegratedTraceType.DiagTransfer:
                    return 0;
            }
            return 5;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal static bool IsTraceEnabled(int level, int flag)
        {
            return ((level < _traceLevel) && ((flag & _traceFlags) != 0));
        }

        private static void ResolveWorkerRequestType(HttpWorkerRequest workerRequest)
        {
            if (workerRequest is IIS7WorkerRequest)
            {
                s_WrType = EtwWorkerRequestType.IIS7Integrated;
            }
            else if (workerRequest is ISAPIWorkerRequestInProc)
            {
                s_WrType = EtwWorkerRequestType.InProc;
            }
            else if (workerRequest is ISAPIWorkerRequestOutOfProc)
            {
                s_WrType = EtwWorkerRequestType.OutOfProc;
            }
            else
            {
                s_WrType = EtwWorkerRequestType.Unknown;
            }
        }

        internal static void Trace(EtwTraceType traceType, HttpWorkerRequest workerRequest)
        {
            Trace(traceType, workerRequest, null, null);
        }

        internal static void Trace(EtwTraceType traceType, HttpWorkerRequest workerRequest, string data1)
        {
            Trace(traceType, workerRequest, data1, null, null, null);
        }

        internal static void Trace(EtwTraceType traceType, HttpWorkerRequest workerRequest, string data1, string data2)
        {
            Trace(traceType, workerRequest, data1, data2, null, null);
        }

        internal static void Trace(EtwTraceType traceType, IntPtr ecb, string data1, string data2, bool inProc)
        {
            if (inProc)
            {
                UnsafeNativeMethods.TraceRaiseEventWithEcb((int) traceType, ecb, data1, data2, null, null);
            }
            else
            {
                UnsafeNativeMethods.PMTraceRaiseEvent((int) traceType, ecb, data1, data2, null, null);
            }
        }

        internal static void Trace(EtwTraceType traceType, HttpWorkerRequest workerRequest, string data1, string data2, string data3, string data4)
        {
            if (s_WrType == EtwWorkerRequestType.Undefined)
            {
                ResolveWorkerRequestType(workerRequest);
            }
            if ((s_WrType != EtwWorkerRequestType.Unknown) && (workerRequest != null))
            {
                if (s_WrType == EtwWorkerRequestType.IIS7Integrated)
                {
                    UnsafeNativeMethods.TraceRaiseEventMgdHandler((int) traceType, ((IIS7WorkerRequest) workerRequest).RequestContext, data1, data2, data3, data4);
                }
                else if (s_WrType == EtwWorkerRequestType.InProc)
                {
                    UnsafeNativeMethods.TraceRaiseEventWithEcb((int) traceType, ((ISAPIWorkerRequest) workerRequest).Ecb, data1, data2, data3, data4);
                }
                else if (s_WrType == EtwWorkerRequestType.OutOfProc)
                {
                    UnsafeNativeMethods.PMTraceRaiseEvent((int) traceType, ((ISAPIWorkerRequest) workerRequest).Ecb, data1, data2, data3, data4);
                }
            }
        }

        internal static void TraceEnableCheck(EtwTraceConfigType configType, IntPtr p)
        {
            if (HttpRuntime.IsEngineLoaded)
            {
                switch (configType)
                {
                    case EtwTraceConfigType.DOWNLEVEL:
                        UnsafeNativeMethods.GetEtwValues(out _traceLevel, out _traceFlags);
                        return;

                    case EtwTraceConfigType.IIS7_ISAPI:
                    {
                        int[] contentInfo = new int[3];
                        UnsafeNativeMethods.EcbGetTraceFlags(p, contentInfo);
                        _traceFlags = contentInfo[0];
                        _traceLevel = contentInfo[1];
                        return;
                    }
                    case EtwTraceConfigType.IIS7_INTEGRATED:
                        bool flag;
                        UnsafeIISMethods.MgdEtwGetTraceConfig(p, out flag, out _traceFlags, out _traceLevel);
                        return;
                }
            }
        }
    }
}

