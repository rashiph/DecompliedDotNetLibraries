namespace System.Runtime.Remoting.Contexts
{
    using System;
    using System.Security;

    [Serializable]
    internal class CallBackHelper
    {
        private int _flags;
        private IntPtr _privateData;
        internal const int RequestedFromEE = 1;
        internal const int XDomainTransition = 0x100;

        internal CallBackHelper(IntPtr privateData, bool bFromEE, int targetDomainID)
        {
            this.IsEERequested = bFromEE;
            this.IsCrossDomain = targetDomainID != 0;
            this._privateData = privateData;
        }

        [SecurityCritical]
        internal void Func()
        {
            if (this.IsEERequested)
            {
                Context.ExecuteCallBackInEE(this._privateData);
            }
        }

        internal bool IsCrossDomain
        {
            set
            {
                if (value)
                {
                    this._flags |= 0x100;
                }
            }
        }

        internal bool IsEERequested
        {
            get
            {
                return ((this._flags & 1) == 1);
            }
            set
            {
                if (value)
                {
                    this._flags |= 1;
                }
            }
        }
    }
}

