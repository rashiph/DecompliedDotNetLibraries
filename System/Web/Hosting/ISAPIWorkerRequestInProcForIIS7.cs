namespace System.Web.Hosting
{
    using System;
    using System.Web;
    using System.Web.Management;

    internal class ISAPIWorkerRequestInProcForIIS7 : ISAPIWorkerRequestInProcForIIS6
    {
        internal ISAPIWorkerRequestInProcForIIS7(IntPtr ecb) : base(ecb)
        {
            base._trySkipIisCustomErrors = true;
        }

        internal override void RaiseTraceEvent(WebBaseEvent webEvent)
        {
            if ((IntPtr.Zero != base._ecb) && EtwTrace.IsTraceEnabled(webEvent.InferEtwTraceVerbosity(), 1))
            {
                int num;
                string[] strArray;
                int[] numArray;
                string[] strArray2;
                int num2;
                webEvent.DeconstructWebEvent(out num2, out num, out strArray, out numArray, out strArray2);
                UnsafeNativeMethods.EcbEmitWebEventTrace(base._ecb, num2, num, strArray, numArray, strArray2);
            }
        }

        internal override void RaiseTraceEvent(IntegratedTraceType traceType, string eventData)
        {
            if (IntPtr.Zero != base._ecb)
            {
                int flag = (traceType < IntegratedTraceType.DiagCritical) ? 4 : 2;
                if (EtwTrace.IsTraceEnabled(EtwTrace.InferVerbosity(traceType), flag))
                {
                    string str = string.IsNullOrEmpty(eventData) ? string.Empty : eventData;
                    UnsafeNativeMethods.EcbEmitSimpleTrace(base._ecb, (int) traceType, str);
                }
            }
        }

        internal override bool TrySkipIisCustomErrors
        {
            get
            {
                return base._trySkipIisCustomErrors;
            }
            set
            {
                base._trySkipIisCustomErrors = value;
            }
        }
    }
}

