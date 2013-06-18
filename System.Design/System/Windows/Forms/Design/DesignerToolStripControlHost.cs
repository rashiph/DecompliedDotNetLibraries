namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class DesignerToolStripControlHost : ToolStripControlHost, IComponent, IDisposable
    {
        private BehaviorService b;
        internal ToolStrip parent;

        public DesignerToolStripControlHost(Control c) : base(c)
        {
            base.Margin = Padding.Empty;
        }

        internal GlyphCollection GetGlyphs(ToolStrip parent, GlyphCollection glyphs, System.Windows.Forms.Design.Behavior.Behavior standardBehavior)
        {
            if (this.b == null)
            {
                this.b = (BehaviorService) parent.Site.GetService(typeof(BehaviorService));
            }
            Point pos = this.b.ControlToAdornerWindow(base.Parent);
            Rectangle bounds = this.Bounds;
            bounds.Offset(pos);
            bounds.Inflate(-2, -2);
            glyphs.Add(new MiniLockedBorderGlyph(bounds, SelectionBorderGlyphType.Top, standardBehavior, true));
            glyphs.Add(new MiniLockedBorderGlyph(bounds, SelectionBorderGlyphType.Bottom, standardBehavior, true));
            glyphs.Add(new MiniLockedBorderGlyph(bounds, SelectionBorderGlyphType.Left, standardBehavior, true));
            glyphs.Add(new MiniLockedBorderGlyph(bounds, SelectionBorderGlyphType.Right, standardBehavior, true));
            return glyphs;
        }

        internal void RefreshSelectionGlyph()
        {
            ToolStrip control = base.Control as ToolStrip;
            if (control != null)
            {
                ToolStripTemplateNode.MiniToolStripRenderer renderer = control.Renderer as ToolStripTemplateNode.MiniToolStripRenderer;
                if (renderer != null)
                {
                    renderer.State = 0;
                    control.Invalidate();
                }
            }
        }

        internal void SelectControl()
        {
            ToolStrip control = base.Control as ToolStrip;
            if (control != null)
            {
                ToolStripTemplateNode.MiniToolStripRenderer renderer = control.Renderer as ToolStripTemplateNode.MiniToolStripRenderer;
                if (renderer != null)
                {
                    renderer.State = 1;
                    control.Invalidate();
                }
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(0x5c, 0x16);
            }
        }
    }
}

