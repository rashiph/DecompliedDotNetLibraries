namespace System.Runtime.ConstrainedExecution
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode=true)]
    public abstract class CriticalFinalizerObject
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected CriticalFinalizerObject()
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        ~CriticalFinalizerObject()
        {
        }
    }
}

