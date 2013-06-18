namespace System.Data.OracleClient
{
    using System;

    internal sealed class OciRowidDescriptor : OciHandle
    {
        internal OciRowidDescriptor(OciHandle parent) : base(parent, OCI.HTYPE.OCI_DTYPE_ROWID)
        {
        }

        internal void GetRowid(OciStatementHandle statementHandle, OciErrorHandle errorHandle)
        {
            uint sizep = 0;
            int rc = TracedNativeMethods.OCIAttrGet(statementHandle, this, out sizep, OCI.ATTR.OCI_ATTR_ROWID, errorHandle);
            if (100 == rc)
            {
                base.Dispose();
            }
            else if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
        }
    }
}

