namespace System.StubHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.ConstrainedExecution;
    using System.Security;

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecurityCritical, ForceTokenStabilization]
    internal sealed class CleanupWorkList
    {
        private List<CleanupWorkListElement> m_list = new List<CleanupWorkListElement>();

        public void Add(CleanupWorkListElement elem)
        {
            this.m_list.Add(elem);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public void Destroy()
        {
            for (int i = this.m_list.Count - 1; i >= 0; i--)
            {
                if (this.m_list[i].m_owned)
                {
                    System.StubHelpers.StubHelpers.SafeHandleRelease(this.m_list[i].m_handle);
                }
            }
        }
    }
}

