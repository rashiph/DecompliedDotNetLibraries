namespace System.Windows.Forms.Design
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal interface IOverlayService
    {
        void InsertOverlay(Control control, int index);
        void InvalidateOverlays(Rectangle screenRectangle);
        void InvalidateOverlays(Region screenRegion);
        int PushOverlay(Control control);
        void RemoveOverlay(Control control);
    }
}

