namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal class LockedHandleGlyph : SelectionGlyphBase
    {
        private bool isPrimary;

        internal LockedHandleGlyph(Rectangle controlBounds, bool primarySelection) : base(null)
        {
            this.isPrimary = primarySelection;
            base.hitTestCursor = Cursors.Default;
            base.rules = SelectionRules.None;
            base.bounds = new Rectangle((controlBounds.X + DesignerUtils.LOCKHANDLEOVERLAP) - DesignerUtils.LOCKHANDLEWIDTH, (controlBounds.Y + DesignerUtils.LOCKHANDLEOVERLAP) - DesignerUtils.LOCKHANDLEHEIGHT, DesignerUtils.LOCKHANDLEWIDTH, DesignerUtils.LOCKHANDLEHEIGHT);
            base.hitBounds = base.bounds;
        }

        public override void Paint(PaintEventArgs pe)
        {
            DesignerUtils.DrawLockedHandle(pe.Graphics, base.bounds, this.isPrimary, this);
        }
    }
}

