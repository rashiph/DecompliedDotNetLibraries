namespace System.Deployment.Internal.CodeSigning
{
    using System;

    [Flags]
    internal enum CmiManifestSignerFlag
    {
        None,
        DontReplacePublicKeyToken
    }
}

