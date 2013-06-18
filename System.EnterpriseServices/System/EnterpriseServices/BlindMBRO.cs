namespace System.EnterpriseServices
{
    using System;

    internal class BlindMBRO : MarshalByRefObject
    {
        private MarshalByRefObject _holder;

        public BlindMBRO(MarshalByRefObject holder)
        {
            this._holder = holder;
        }
    }
}

