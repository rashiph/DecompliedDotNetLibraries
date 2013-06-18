namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;

    internal sealed class ViewPortData
    {
        public Rectangle LogicalViewPort;
        public Bitmap MemoryBitmap;
        public SizeF Scaling = new SizeF(1f, 1f);
        public Size ShadowDepth = Size.Empty;
        public Point Translation = Point.Empty;
        public Color TransparentColor = Color.White;
        public Size ViewPortSize = Size.Empty;
    }
}

