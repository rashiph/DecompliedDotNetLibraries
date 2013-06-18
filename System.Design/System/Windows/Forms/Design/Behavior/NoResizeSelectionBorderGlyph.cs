namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal class NoResizeSelectionBorderGlyph : SelectionGlyphBase
    {
        internal NoResizeSelectionBorderGlyph(Rectangle controlBounds, SelectionRules rules, SelectionBorderGlyphType type, System.Windows.Forms.Design.Behavior.Behavior behavior) : base(behavior)
        {
            this.InitializeGlyph(controlBounds, rules, type);
        }

        private void InitializeGlyph(Rectangle controlBounds, SelectionRules selRules, SelectionBorderGlyphType type)
        {
            base.rules = SelectionRules.None;
            base.hitTestCursor = Cursors.Default;
            if ((selRules & SelectionRules.Moveable) != SelectionRules.None)
            {
                base.rules = SelectionRules.Moveable;
                base.hitTestCursor = Cursors.SizeAll;
            }
            base.bounds = DesignerUtils.GetBoundsForNoResizeSelectionType(controlBounds, type);
            base.hitBounds = base.bounds;
            switch (type)
            {
                case SelectionBorderGlyphType.Top:
                case SelectionBorderGlyphType.Bottom:
                    this.hitBounds.Y -= (DesignerUtils.SELECTIONBORDERHITAREA - DesignerUtils.SELECTIONBORDERSIZE) / 2;
                    this.hitBounds.Height += DesignerUtils.SELECTIONBORDERHITAREA - DesignerUtils.SELECTIONBORDERSIZE;
                    return;

                case SelectionBorderGlyphType.Left:
                case SelectionBorderGlyphType.Right:
                    this.hitBounds.X -= (DesignerUtils.SELECTIONBORDERHITAREA - DesignerUtils.SELECTIONBORDERSIZE) / 2;
                    this.hitBounds.Width += DesignerUtils.SELECTIONBORDERHITAREA - DesignerUtils.SELECTIONBORDERSIZE;
                    return;
            }
        }

        public override void Paint(PaintEventArgs pe)
        {
            DesignerUtils.DrawSelectionBorder(pe.Graphics, base.bounds);
        }
    }
}

