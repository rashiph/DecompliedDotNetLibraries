namespace System.Workflow.ComponentModel
{
    using System;

    internal enum ActivityOperationType : byte
    {
        Cancel = 1,
        Compensate = 2,
        Execute = 0,
        HandleFault = 3
    }
}

