namespace System.Data.OracleClient
{
    using System;

    internal sealed class OciBindHandle : OciSimpleHandle
    {
        internal OciBindHandle(OciHandle parent, IntPtr value) : base(parent, OCI.HTYPE.OCI_HTYPE_BIND, value)
        {
        }
    }
}

