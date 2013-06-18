namespace System.Activities.Statements
{
    using System;

    internal enum CompensationBookmarkName
    {
        Confirmed,
        Canceled,
        Compensated,
        OnConfirmation,
        OnCompensation,
        OnCancellation,
        OnSecondaryRootScheduled
    }
}

