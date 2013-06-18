namespace Microsoft.Build.Tasks
{
    using System;

    internal enum NoMatchReason
    {
        Unknown,
        FileNotFound,
        FusionNamesDidNotMatch,
        TargetHadNoFusionName,
        NotInGac,
        NotAFileNameOnDisk,
        ProcessorArchitectureDoesNotMatch
    }
}

