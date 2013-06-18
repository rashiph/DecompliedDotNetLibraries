namespace System.Data.OracleClient
{
    using System;

    internal sealed class OciServiceContextHandle : OciHandle
    {
        internal OciServiceContextHandle(OciHandle parent) : base(parent, OCI.HTYPE.OCI_HTYPE_SVCCTX, OCI.MODE.OCI_DEFAULT, OciHandle.HANDLEFLAG.DEFAULT)
        {
        }
    }
}

