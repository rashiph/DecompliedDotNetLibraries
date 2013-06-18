namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal class MiniLockedBorderGlyph : SelectionGlyphBase
    {
        private SelectionBorderGlyphType type;

        internal MiniLockedBorderGlyph(Rectangle controlBounds, SelectionBorderGlyphType type, System.Windows.Forms.Design.Behavior.Behavior behavior, bool primarySelection) : base(behavior)
        {
            this.InitializeGlyph(controlBounds, type, primarySelection);
        }

        private void InitializeGlyph(Rectangle controlBounds, SelectionBorderGlyphType type, bool primarySelection)
        {
            base.hitTestCursor = Cursors.Default;
            base.rules = SelectionRules.None;
            int borderSize = 1;
            this.type = type;
            base.bounds = DesignerUtils.GetBoundsForSelectionType(controlBounds, type, borderSize);
            base.hitBounds = base.bounds;
        }

        public override void Paint(PaintEventArgs pe)
        {
            pe.Graphics.FillRectangle(Brushes.Black, base.bounds);
        }
    }
}

