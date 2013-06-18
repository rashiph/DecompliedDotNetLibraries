namespace System.Data.OracleClient
{
    using System;

    internal sealed class OciLobDescriptor : OciHandle
    {
        internal OciLobDescriptor(OciHandle parent) : base(parent, OCI.HTYPE.OCI_DTYPE_FIRST)
        {
        }
    }
}

