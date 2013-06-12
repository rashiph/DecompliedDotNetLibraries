namespace System.Windows.Forms.Design
{
    using System;

    [Flags]
    public enum ToolStripItemDesignerAvailability
    {
        All = 15,
        ContextMenuStrip = 4,
        MenuStrip = 2,
        None = 0,
        StatusStrip = 8,
        ToolStrip = 1
    }
}

