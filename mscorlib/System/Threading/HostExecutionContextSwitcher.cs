namespace System.Threading
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Security;

    internal class HostExecutionContextSwitcher
    {
        internal HostExecutionContext currentHostContext;
        internal ExecutionContext executionContext;
        internal HostExecutionContext previousHostContext;

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static void Undo(object switcherObject)
        {
            if (switcherObject != null)
            {
                HostExecutionContextManager currentHostExecutionContextManager = HostExecutionContextManager.GetCurrentHostExecutionContextManager();
                if (currentHostExecutionContextManager != null)
                {
                    currentHostExecutionContextManager.Revert(switcherObject);
                }
            }
        }
    }
}

