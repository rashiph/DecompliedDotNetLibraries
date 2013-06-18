namespace System.Data.OracleClient
{
    using System;

    internal sealed class OciDateTimeDescriptor : OciHandle
    {
        internal OciDateTimeDescriptor(OciHandle parent, OCI.HTYPE dateTimeType) : base(parent, AssertDateTimeType(dateTimeType))
        {
        }

        private static OCI.HTYPE AssertDateTimeType(OCI.HTYPE dateTimeType)
        {
            return dateTimeType;
        }
    }
}

