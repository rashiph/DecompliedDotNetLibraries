namespace System.Data
{
    using System;
    using System.ComponentModel;

    [Flags, Editor("Microsoft.VSDesigner.Data.Design.DataViewRowStateEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public enum DataViewRowState
    {
        Added = 4,
        CurrentRows = 0x16,
        Deleted = 8,
        ModifiedCurrent = 0x10,
        ModifiedOriginal = 0x20,
        None = 0,
        OriginalRows = 0x2a,
        Unchanged = 2
    }
}

