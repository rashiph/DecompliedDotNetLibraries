namespace System.Security
{
    using System;

    [Flags]
    public enum ManifestKinds
    {
        None,
        Deployment,
        Application,
        ApplicationAndDeployment
    }
}

