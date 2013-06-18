namespace System.Deployment.Application
{
    using System;

    internal enum ExceptionTypes
    {
        Unknown,
        Activation,
        ComponentStore,
        ActivationInProgress,
        InvalidShortcut,
        InvalidARPEntry,
        LockTimeout,
        Subscription,
        SubscriptionState,
        ActivationLimitExceeded,
        DiskIsFull,
        GroupMultipleMatch,
        InvalidManifest,
        Manifest,
        ManifestLoad,
        ManifestParse,
        ManifestSemanticValidation,
        ManifestComponentSemanticValidation,
        UnsupportedElevetaionRequest,
        SubscriptionSemanticValidation,
        UriSchemeNotSupported,
        Zone,
        DeploymentUriDifferent,
        SizeLimitForPartialTrustOnlineAppExceeded,
        Validation,
        HashValidation,
        SignatureValidation,
        RefDefValidation,
        ClrValidation,
        StronglyNamedAssemblyVerification,
        IdentityMatchValidationForMixedModeAssembly,
        AppFileLocationValidation,
        FileSizeValidation,
        TrustFailDependentPlatform
    }
}

