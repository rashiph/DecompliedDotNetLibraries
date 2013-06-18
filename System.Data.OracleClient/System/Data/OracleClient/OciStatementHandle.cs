namespace System.Data.OracleClient
{
    using System;

    internal sealed class OciStatementHandle : OciHandle
    {
        internal OciStatementHandle(OciHandle parent) : base(parent, OCI.HTYPE.OCI_HTYPE_STMT)
        {
        }

        internal OciParameterDescriptor GetDescriptor(int i, OciErrorHandle errorHandle)
        {
            IntPtr ptr;
            int rc = TracedNativeMethods.OCIParamGet(this, base.HandleType, errorHandle, out ptr, i + 1);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return new OciParameterDescriptor(this, ptr);
        }

        internal OciRowidDescriptor GetRowid(OciHandle environmentHandle, OciErrorHandle errorHandle)
        {
            OciRowidDescriptor descriptor = new OciRowidDescriptor(environmentHandle);
            descriptor.GetRowid(this, errorHandle);
            return descriptor;
        }
    }
}

