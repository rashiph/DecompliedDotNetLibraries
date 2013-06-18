namespace System.Deployment.Internal.Isolation
{
    using System;

    [Flags]
    internal enum ISTORE_ENUM_ASSEMBLIES_FLAGS
    {
        ISTORE_ENUM_ASSEMBLIES_FLAG_FORCE_LIBRARY_SEMANTICS = 4,
        ISTORE_ENUM_ASSEMBLIES_FLAG_LIMIT_TO_VISIBLE_ONLY = 1,
        ISTORE_ENUM_ASSEMBLIES_FLAG_MATCH_SERVICING = 2
    }
}

