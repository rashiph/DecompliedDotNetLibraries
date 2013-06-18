namespace System.Workflow.ComponentModel.Compiler
{
    using System;

    [Flags]
    public enum AccessTypes
    {
        Read = 1,
        ReadWrite = 3,
        Write = 2
    }
}

