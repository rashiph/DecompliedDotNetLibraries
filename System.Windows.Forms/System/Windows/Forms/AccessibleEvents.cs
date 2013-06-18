namespace System.Windows.Forms
{
    using System;

    public enum AccessibleEvents
    {
        AcceleratorChange = 0x8012,
        Create = 0x8000,
        DefaultActionChange = 0x8011,
        DescriptionChange = 0x800d,
        Destroy = 0x8001,
        Focus = 0x8005,
        HelpChange = 0x8010,
        Hide = 0x8003,
        LocationChange = 0x800b,
        NameChange = 0x800c,
        ParentChange = 0x800f,
        Reorder = 0x8004,
        Selection = 0x8006,
        SelectionAdd = 0x8007,
        SelectionRemove = 0x8008,
        SelectionWithin = 0x8009,
        Show = 0x8002,
        StateChange = 0x800a,
        SystemAlert = 2,
        SystemCaptureEnd = 9,
        SystemCaptureStart = 8,
        SystemContextHelpEnd = 13,
        SystemContextHelpStart = 12,
        SystemDialogEnd = 0x11,
        SystemDialogStart = 0x10,
        SystemDragDropEnd = 15,
        SystemDragDropStart = 14,
        SystemForeground = 3,
        SystemMenuEnd = 5,
        SystemMenuPopupEnd = 7,
        SystemMenuPopupStart = 6,
        SystemMenuStart = 4,
        SystemMinimizeEnd = 0x17,
        SystemMinimizeStart = 0x16,
        SystemMoveSizeEnd = 11,
        SystemMoveSizeStart = 10,
        SystemScrollingEnd = 0x13,
        SystemScrollingStart = 0x12,
        SystemSound = 1,
        SystemSwitchEnd = 0x15,
        SystemSwitchStart = 20,
        ValueChange = 0x800e
    }
}

