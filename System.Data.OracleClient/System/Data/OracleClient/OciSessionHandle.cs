namespace System.Data.OracleClient
{
    using System;

    internal sealed class OciSessionHandle : OciHandle
    {
        internal OciSessionHandle(OciHandle parent) : base(parent, OCI.HTYPE.OCI_HTYPE_SESSION, OCI.MODE.OCI_DEFAULT, OciHandle.HANDLEFLAG.DEFAULT)
        {
        }
    }
}

