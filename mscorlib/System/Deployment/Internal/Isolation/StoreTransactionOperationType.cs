namespace System.Deployment.Internal.Isolation
{
    using System;

    internal enum StoreTransactionOperationType
    {
        InstallDeployment = 0x18,
        Invalid = 0,
        PinDeployment = 0x15,
        Scavenge = 0x1b,
        SetCanonicalizationContext = 14,
        SetDeploymentMetadata = 0x1a,
        StageComponent = 20,
        StageComponentFile = 0x17,
        UninstallDeployment = 0x19,
        UnpinDeployment = 0x16
    }
}

