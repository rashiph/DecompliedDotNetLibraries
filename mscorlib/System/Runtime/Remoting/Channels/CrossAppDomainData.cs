namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.Remoting;
    using System.Security;
    using System.Threading;

    [Serializable]
    internal class CrossAppDomainData
    {
        private object _ContextID = 0;
        private int _DomainID;
        private string _processGuid;

        internal CrossAppDomainData(IntPtr ctxId, int domainID, string processGuid)
        {
            this._DomainID = domainID;
            this._processGuid = processGuid;
            this._ContextID = ctxId.ToInt32();
        }

        [SecurityCritical]
        internal bool IsFromThisAppDomain()
        {
            return (this.IsFromThisProcess() && (Thread.GetDomain().GetId() == this._DomainID));
        }

        internal bool IsFromThisProcess()
        {
            return Identity.ProcessGuid.Equals(this._processGuid);
        }

        internal virtual IntPtr ContextID
        {
            get
            {
                return new IntPtr((int) this._ContextID);
            }
        }

        internal virtual int DomainID
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                return this._DomainID;
            }
        }

        internal virtual string ProcessGuid
        {
            get
            {
                return this._processGuid;
            }
        }
    }
}

