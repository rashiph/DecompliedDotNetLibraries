namespace Microsoft.Build.Tasks
{
    using System;

    internal enum CopyLocalState
    {
        Undecided,
        YesBecauseOfHeuristic,
        YesBecauseReferenceItemHadMetadata,
        NoBecauseFrameworkFile,
        NoBecausePrerequisite,
        NoBecauseReferenceItemHadMetadata,
        NoBecauseReferenceFoundInGAC,
        NoBecauseConflictVictim,
        NoBecauseUnresolved,
        NoBecauseEmbedded,
        NoBecauseParentReferencesFoundInGAC
    }
}

