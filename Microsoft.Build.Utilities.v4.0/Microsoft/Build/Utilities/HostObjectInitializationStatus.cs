namespace Microsoft.Build.Utilities
{
    using System;

    public enum HostObjectInitializationStatus
    {
        UseHostObjectToExecute,
        UseAlternateToolToExecute,
        NoActionReturnSuccess,
        NoActionReturnFailure
    }
}

