namespace System.Data.OracleClient
{
    using System;
    using System.Data.Common;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;

    internal sealed class OciFileDescriptor : OciHandle
    {
        internal OciFileDescriptor(OciHandle parent) : base(parent, OCI.HTYPE.OCI_DTYPE_FILE)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal int OCILobFileSetNameWrapper(OciHandle envhp, OciHandle errhp, byte[] dirAlias, ushort dirAliasLength, byte[] fileName, ushort fileNameLength)
        {
            int num;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    IntPtr handle = base.DangerousGetHandle();
                    num = System.Data.Common.UnsafeNativeMethods.OCILobFileSetName(envhp, errhp, ref handle, dirAlias, dirAliasLength, fileName, fileNameLength);
                    base.handle = handle;
                }
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
            return num;
        }
    }
}

