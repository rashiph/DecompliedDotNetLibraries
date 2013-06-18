namespace System.Data.OracleClient
{
    using System;

    internal sealed class OciServerHandle : OciHandle
    {
        internal OciServerHandle(OciHandle parent) : base(parent, OCI.HTYPE.OCI_HTYPE_SERVER, OCI.MODE.OCI_DEFAULT, OciHandle.HANDLEFLAG.DEFAULT)
        {
        }
    }
}

