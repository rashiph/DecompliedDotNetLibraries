namespace System.Data.OracleClient
{
    using System;

    internal sealed class OciEnvironmentHandle : OciHandle
    {
        internal OciEnvironmentHandle(OCI.MODE environmentMode, bool unicode) : base(null, OCI.HTYPE.OCI_HTYPE_ENV, environmentMode, unicode ? OciHandle.HANDLEFLAG.UNICODE : OciHandle.HANDLEFLAG.DEFAULT)
        {
        }
    }
}

