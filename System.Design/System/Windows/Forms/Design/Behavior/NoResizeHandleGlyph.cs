namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal class NoResizeHandleGlyph : SelectionGlyphBase
    {
        private bool isPrimary;

        internal NoResizeHandleGlyph(Rectangle controlBounds, SelectionRules selRules, bool primarySelection, System.Windows.Forms.Design.Behavior.Behavior behavior) : base(behavior)
        {
            this.isPrimary = primarySelection;
            base.hitTestCursor = Cursors.Default;
            base.rules = SelectionRules.None;
            if ((selRules & SelectionRules.Moveable) != SelectionRules.None)
            {
                base.rules = SelectionRules.Moveable;
                base.hitTestCursor = Cursors.SizeAll;
            }
            base.bounds = new Rectangle(controlBounds.X - DesignerUtils.NORESIZEHANDLESIZE, controlBounds.Y - DesignerUtils.NORESIZEHANDLESIZE, DesignerUtils.NORESIZEHANDLESIZE, DesignerUtils.NORESIZEHANDLESIZE);
            base.hitBounds = base.bounds;
        }

        public override void Paint(PaintEventArgs pe)
        {
            DesignerUtils.DrawNoResizeHandle(pe.Graphics, base.bounds, this.isPrimary, this);
        }
    }
}

