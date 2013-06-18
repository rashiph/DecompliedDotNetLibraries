namespace System.ServiceModel.Activation
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("A9E69610-B80D-11D0-B9B9-00A0C922E750"), SecurityCritical]
    internal class MSAdminBase
    {
        internal const int ALL_METADATA = 0;
        internal const int BINARY_METADATA = 3;
        internal const uint DEFAULT_METABASE_TIMEOUT = 0x7530;
        internal const int DWORD_METADATA = 1;
        internal const int EXPANDSZ_METADATA = 4;
        internal const int IIS_MD_UT_SERVER = 1;
        internal const int METADATA_INHERIT = 1;
        internal const int METADATA_MASTER_ROOT_HANDLE = 0;
        internal const int METADATA_PERMISSION_READ = 1;
        internal const int MULTISZ_METADATA = 5;
        internal const int STRING_METADATA = 2;
    }
}

