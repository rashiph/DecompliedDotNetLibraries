namespace System.Data.OracleClient
{
    using System;

    internal sealed class OciNlsEnvironmentHandle : OciHandle
    {
        internal OciNlsEnvironmentHandle(OCI.MODE environmentMode) : base(null, OCI.HTYPE.OCI_HTYPE_ENV, environmentMode, OciHandle.HANDLEFLAG.NLS)
        {
        }
    }
}

