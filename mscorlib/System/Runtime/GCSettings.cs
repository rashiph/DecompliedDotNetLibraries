namespace System.Runtime
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Security;
    using System.Security.Permissions;

    public static class GCSettings
    {
        public static bool IsServerGC
        {
            [SecuritySafeCritical]
            get
            {
                return GC.IsServerGC();
            }
        }

        public static GCLatencyMode LatencyMode
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
            get
            {
                return (GCLatencyMode) GC.GetGCLatencyMode();
            }
            [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
            set
            {
                if ((value < GCLatencyMode.Batch) || (value > GCLatencyMode.SustainedLowLatency))
                {
                    throw new ArgumentOutOfRangeException(Environment.GetResourceString("ArgumentOutOfRange_Enum"));
                }
                GC.SetGCLatencyMode((int) value);
            }
        }
    }
}

