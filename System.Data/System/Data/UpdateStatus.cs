namespace System.Data
{
    using System;

    public enum UpdateStatus
    {
        Continue,
        ErrorsOccurred,
        SkipCurrentRow,
        SkipAllRemainingRows
    }
}

