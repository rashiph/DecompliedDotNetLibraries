namespace Microsoft.Build.Tasks
{
    using System;

    internal enum ConflictLossReason
    {
        DidntLose,
        HadLowerVersion,
        InsolubleConflict,
        WasNotPrimary,
        FusionEquivalentWithSameVersion
    }
}

