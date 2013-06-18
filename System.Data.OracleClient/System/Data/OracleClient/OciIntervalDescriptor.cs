namespace System.Data.OracleClient
{
    using System;

    internal sealed class OciIntervalDescriptor : OciHandle
    {
        internal OciIntervalDescriptor(OciHandle parent) : base(parent, OCI.HTYPE.OCI_DTYPE_INTERVAL_DS)
        {
        }
    }
}

