namespace System.Data.OracleClient
{
    using System;

    internal sealed class OciDefineHandle : OciSimpleHandle
    {
        internal OciDefineHandle(OciHandle parent, IntPtr value) : base(parent, OCI.HTYPE.OCI_HTYPE_DEFINE, value)
        {
        }
    }
}

