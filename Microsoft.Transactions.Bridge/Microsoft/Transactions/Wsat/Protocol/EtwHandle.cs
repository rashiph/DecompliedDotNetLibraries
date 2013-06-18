namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using System;
    using System.ComponentModel;
    using System.Runtime.ConstrainedExecution;

    internal class EtwHandle : CriticalFinalizerObject
    {
        private ulong traceHandle;

        private EtwHandle(ulong traceHandle)
        {
            this.traceHandle = traceHandle;
        }

        ~EtwHandle()
        {
            EtwNativeMethods.UnregisterTraceGuids(this.traceHandle);
        }

        internal static EtwHandle RegisterTraceGuids(EtwTraceCallback cbFunc, Guid controlGuid, TraceGuidRegistration registration)
        {
            ulong regHandle = 0L;
            uint num2 = EtwNativeMethods.RegisterTraceGuids(cbFunc, null, ref controlGuid, 1, ref registration, null, null, out regHandle);
            if (num2 != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception((int) num2));
            }
            return new EtwHandle(regHandle);
        }
    }
}

