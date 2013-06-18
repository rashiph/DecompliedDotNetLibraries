namespace System.Data.OracleClient
{
    using System;

    internal abstract class OciSimpleHandle : OciHandle
    {
        internal OciSimpleHandle(OciHandle parent, OCI.HTYPE handleType, IntPtr value) : base(handleType)
        {
            base.handle = value;
        }

        public override bool IsInvalid
        {
            get
            {
                return true;
            }
        }
    }
}

