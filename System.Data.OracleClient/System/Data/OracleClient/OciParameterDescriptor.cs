namespace System.Data.OracleClient
{
    using System;

    internal sealed class OciParameterDescriptor : OciSimpleHandle
    {
        internal OciParameterDescriptor(OciHandle parent, IntPtr value) : base(parent, OCI.HTYPE.OCI_DTYPE_PARAM, value)
        {
        }
    }
}

