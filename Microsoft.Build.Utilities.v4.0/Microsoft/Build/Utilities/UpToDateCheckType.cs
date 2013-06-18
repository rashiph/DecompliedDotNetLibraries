namespace Microsoft.Build.Utilities
{
    using System;

    public enum UpToDateCheckType
    {
        InputNewerThanOutput,
        InputOrOutputNewerThanTracking,
        InputNewerThanTracking
    }
}

