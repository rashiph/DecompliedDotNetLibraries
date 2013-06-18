namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal class SelectionBorderGlyph : SelectionGlyphBase
    {
        internal SelectionBorderGlyph(Rectangle controlBounds, SelectionRules rules, SelectionBorderGlyphType type, System.Windows.Forms.Design.Behavior.Behavior behavior) : base(behavior)
        {
            this.InitializeGlyph(controlBounds, rules, type);
        }

        private void InitializeGlyph(Rectangle controlBounds, SelectionRules selRules, SelectionBorderGlyphType type)
        {
            base.rules = SelectionRules.None;
            base.hitTestCursor = Cursors.Default;
            base.bounds = DesignerUtils.GetBoundsForSelectionType(controlBounds, type);
            base.hitBounds = base.bounds;
            switch (type)
            {
                case SelectionBorderGlyphType.Top:
                    if ((selRules & SelectionRules.TopSizeable) != SelectionRules.None)
                    {
                        base.hitTestCursor = Cursors.SizeNS;
                        base.rules = SelectionRules.TopSizeable;
                    }
                    this.hitBounds.Y -= (DesignerUtils.SELECTIONBORDERHITAREA - DesignerUtils.SELECTIONBORDERSIZE) / 2;
                    this.hitBounds.Height += DesignerUtils.SELECTIONBORDERHITAREA - DesignerUtils.SELECTIONBORDERSIZE;
                    return;

                case SelectionBorderGlyphType.Bottom:
                    if ((selRules & SelectionRules.BottomSizeable) != SelectionRules.None)
                    {
                        base.hitTestCursor = Cursors.SizeNS;
                        base.rules = SelectionRules.BottomSizeable;
                    }
                    this.hitBounds.Y -= (DesignerUtils.SELECTIONBORDERHITAREA - DesignerUtils.SELECTIONBORDERSIZE) / 2;
                    this.hitBounds.Height += DesignerUtils.SELECTIONBORDERHITAREA - DesignerUtils.SELECTIONBORDERSIZE;
                    return;

                case SelectionBorderGlyphType.Left:
                    if ((selRules & SelectionRules.LeftSizeable) != SelectionRules.None)
                    {
                        base.hitTestCursor = Cursors.SizeWE;
                        base.rules = SelectionRules.LeftSizeable;
                    }
                    this.hitBounds.X -= (DesignerUtils.SELECTIONBORDERHITAREA - DesignerUtils.SELECTIONBORDERSIZE) / 2;
                    this.hitBounds.Width += DesignerUtils.SELECTIONBORDERHITAREA - DesignerUtils.SELECTIONBORDERSIZE;
                    return;

                case SelectionBorderGlyphType.Right:
                    if ((selRules & SelectionRules.RightSizeable) != SelectionRules.None)
                    {
                        base.hitTestCursor = Cursors.SizeWE;
                        base.rules = SelectionRules.RightSizeable;
                    }
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

