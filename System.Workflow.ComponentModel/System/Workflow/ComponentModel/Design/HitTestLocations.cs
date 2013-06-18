namespace System.Workflow.ComponentModel.Design
{
    using System;

    [Flags]
    public enum HitTestLocations
    {
        ActionArea = 2,
        Bottom = 0x20,
        Connector = 0x40,
        Designer = 1,
        Left = 4,
        None = 0,
        Right = 0x10,
        Top = 8
    }
}

