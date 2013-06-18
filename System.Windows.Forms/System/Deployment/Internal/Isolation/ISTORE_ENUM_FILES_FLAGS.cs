namespace System.Deployment.Internal.Isolation
{
    using System;

    [Flags]
    internal enum ISTORE_ENUM_FILES_FLAGS
    {
        ISTORE_ENUM_FILES_FLAG_INCLUDE_INSTALLED_FILES = 1,
        ISTORE_ENUM_FILES_FLAG_INCLUDE_MISSING_FILES = 2
    }
}

