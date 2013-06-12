namespace System.Runtime.Remoting.Messaging
{
    using System;

    [Serializable]
    internal class CallContextRemotingData : ICloneable
    {
        private string _logicalCallID;

        public object Clone()
        {
            return new CallContextRemotingData { LogicalCallID = this.LogicalCallID };
        }

        internal bool HasInfo
        {
            get
            {
                return (this._logicalCallID != null);
            }
        }

        internal string LogicalCallID
        {
            get
            {
                return this._logicalCallID;
            }
            set
            {
                this._logicalCallID = value;
            }
        }
    }
}

