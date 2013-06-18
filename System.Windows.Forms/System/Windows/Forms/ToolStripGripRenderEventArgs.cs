namespace System.Windows.Forms
{
    using System;
    using System.Drawing;

    public class ToolStripGripRenderEventArgs : ToolStripRenderEventArgs
    {
        public ToolStripGripRenderEventArgs(Graphics g, ToolStrip toolStrip) : base(g, toolStrip)
        {
        }

        public Rectangle GripBounds
        {
            get
            {
                return base.ToolStrip.GripRectangle;
            }
        }

        public ToolStripGripDisplayStyle GripDisplayStyle
        {
            get
            {
                return base.ToolStrip.GripDisplayStyle;
            }
        }

        public ToolStripGripStyle GripStyle
        {
            get
            {
                return base.ToolStrip.GripStyle;
            }
        }
    }
}

