namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class ContainerSelectorGlyph : Glyph
    {
        private Bitmap glyph;
        private Rectangle glyphBounds;
        private ContainerSelectorBehavior relatedBehavior;

        internal ContainerSelectorGlyph(Rectangle containerBounds, int glyphSize, int glyphOffset, ContainerSelectorBehavior behavior) : base(behavior)
        {
            this.relatedBehavior = behavior;
            this.glyphBounds = new Rectangle(containerBounds.X + glyphOffset, containerBounds.Y - ((int) (glyphSize * 0.5)), glyphSize, glyphSize);
        }

        public override Cursor GetHitTest(Point p)
        {
            if (!this.glyphBounds.Contains(p) && !this.relatedBehavior.OkToMove)
            {
                return null;
            }
            return Cursors.SizeAll;
        }

        public override void Paint(PaintEventArgs pe)
        {
            pe.Graphics.DrawImage(this.MoveGlyph, this.glyphBounds);
        }

        public override Rectangle Bounds
        {
            get
            {
                return this.glyphBounds;
            }
        }

        private Bitmap MoveGlyph
        {
            get
            {
                if (this.glyph == null)
                {
                    this.glyph = new Bitmap(typeof(ContainerSelectorGlyph), "MoverGlyph.bmp");
                    this.glyph.MakeTransparent();
                }
                return this.glyph;
            }
        }

        public System.Windows.Forms.Design.Behavior.Behavior RelatedBehavior
        {
            get
            {
                return this.relatedBehavior;
            }
        }
    }
}

