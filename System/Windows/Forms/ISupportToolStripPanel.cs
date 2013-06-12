namespace System.Windows.Forms
{
    using System;

    internal interface ISupportToolStripPanel
    {
        void BeginDrag();
        void EndDrag();

        bool IsCurrentlyDragging { get; }

        bool Stretch { get; set; }

        System.Windows.Forms.ToolStripPanelCell ToolStripPanelCell { get; }

        System.Windows.Forms.ToolStripPanelRow ToolStripPanelRow { get; set; }
    }
}

