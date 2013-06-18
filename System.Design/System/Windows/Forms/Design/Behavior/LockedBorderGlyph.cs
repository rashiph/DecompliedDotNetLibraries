namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal class LockedBorderGlyph : SelectionGlyphBase
    {
        internal LockedBorderGlyph(Rectangle controlBounds, SelectionBorderGlyphType type) : base(null)
        {
            this.InitializeGlyph(controlBounds, type);
        }

        private void InitializeGlyph(Rectangle controlBounds, SelectionBorderGlyphType type)
        {
            base.hitTestCursor = Cursors.Default;
            base.rules = SelectionRules.None;
            base.bounds = DesignerUtils.GetBoundsForSelectionType(controlBounds, type);
            base.hitBounds = base.bounds;
        }

        public override void Paint(PaintEventArgs pe)
        {
            DesignerUtils.DrawSelectionBorder(pe.Graphics, base.bounds);
        }
    }
}

