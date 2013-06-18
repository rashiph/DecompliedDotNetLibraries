namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;

    internal static class CopyLocalStateUtility
    {
        internal static bool IsCopyLocal(CopyLocalState state)
        {
            switch (state)
            {
                case CopyLocalState.YesBecauseOfHeuristic:
                case CopyLocalState.YesBecauseReferenceItemHadMetadata:
                    return true;

                case CopyLocalState.NoBecauseFrameworkFile:
                case CopyLocalState.NoBecausePrerequisite:
                case CopyLocalState.NoBecauseReferenceItemHadMetadata:
                case CopyLocalState.NoBecauseReferenceFoundInGAC:
                case CopyLocalState.NoBecauseConflictVictim:
                case CopyLocalState.NoBecauseUnresolved:
                case CopyLocalState.NoBecauseEmbedded:
                case CopyLocalState.NoBecauseParentReferencesFoundInGAC:
                    return false;
            }
            throw new Microsoft.Build.Shared.InternalErrorException("Unexpected CopyLocal flag.");
        }
    }
}

