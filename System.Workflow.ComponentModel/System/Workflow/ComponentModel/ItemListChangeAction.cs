namespace System.Workflow.ComponentModel
{
    using System;

    [Flags]
    internal enum ItemListChangeAction
    {
        Add = 1,
        Remove = 2,
        Replace = 3
    }
}

