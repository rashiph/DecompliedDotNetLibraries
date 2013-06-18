namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal class GrabHandleGlyph : SelectionGlyphBase
    {
        private bool isPrimary;

        internal GrabHandleGlyph(Rectangle controlBounds, GrabHandleGlyphType type, System.Windows.Forms.Design.Behavior.Behavior behavior, bool primarySelection) : base(behavior)
        {
            this.isPrimary = primarySelection;
            base.hitTestCursor = Cursors.Default;
            base.rules = SelectionRules.None;
            switch (type)
            {
                case GrabHandleGlyphType.UpperLeft:
                    base.bounds = new Rectangle((controlBounds.X + DesignerUtils.HANDLEOVERLAP) - DesignerUtils.HANDLESIZE, (controlBounds.Y + DesignerUtils.HANDLEOVERLAP) - DesignerUtils.HANDLESIZE, DesignerUtils.HANDLESIZE, DesignerUtils.HANDLESIZE);
                    base.hitTestCursor = Cursors.SizeNWSE;
                    base.rules = SelectionRules.LeftSizeable | SelectionRules.TopSizeable;
                    break;

                case GrabHandleGlyphType.UpperRight:
                    base.bounds = new Rectangle(controlBounds.Right - DesignerUtils.HANDLEOVERLAP, (controlBounds.Y + DesignerUtils.HANDLEOVERLAP) - DesignerUtils.HANDLESIZE, DesignerUtils.HANDLESIZE, DesignerUtils.HANDLESIZE);
                    base.hitTestCursor = Cursors.SizeNESW;
                    base.rules = SelectionRules.RightSizeable | SelectionRules.TopSizeable;
                    break;

                case GrabHandleGlyphType.LowerLeft:
                    base.bounds = new Rectangle((controlBounds.X + DesignerUtils.HANDLEOVERLAP) - DesignerUtils.HANDLESIZE, controlBounds.Bottom - DesignerUtils.HANDLEOVERLAP, DesignerUtils.HANDLESIZE, DesignerUtils.HANDLESIZE);
                    base.hitTestCursor = Cursors.SizeNESW;
                    base.rules = SelectionRules.LeftSizeable | SelectionRules.BottomSizeable;
                    break;

                case GrabHandleGlyphType.LowerRight:
                    base.bounds = new Rectangle(controlBounds.Right - DesignerUtils.HANDLEOVERLAP, controlBounds.Bottom - DesignerUtils.HANDLEOVERLAP, DesignerUtils.HANDLESIZE, DesignerUtils.HANDLESIZE);
                    base.hitTestCursor = Cursors.SizeNWSE;
                    base.rules = SelectionRules.RightSizeable | SelectionRules.BottomSizeable;
                    break;

                case GrabHandleGlyphType.MiddleTop:
                    if (controlBounds.Width >= ((2 * DesignerUtils.HANDLEOVERLAP) + (2 * DesignerUtils.HANDLESIZE)))
                    {
                        base.bounds = new Rectangle((controlBounds.X + (controlBounds.Width / 2)) - (DesignerUtils.HANDLESIZE / 2), (controlBounds.Y + DesignerUtils.HANDLEOVERLAP) - DesignerUtils.HANDLESIZE, DesignerUtils.HANDLESIZE, DesignerUtils.HANDLESIZE);
                        base.hitTestCursor = Cursors.SizeNS;
                        base.rules = SelectionRules.TopSizeable;
                    }
                    break;

                case GrabHandleGlyphType.MiddleBottom:
                    if (controlBounds.Width >= ((2 * DesignerUtils.HANDLEOVERLAP) + (2 * DesignerUtils.HANDLESIZE)))
                    {
                        base.bounds = new Rectangle((controlBounds.X + (controlBounds.Width / 2)) - (DesignerUtils.HANDLESIZE / 2), controlBounds.Bottom - DesignerUtils.HANDLEOVERLAP, DesignerUtils.HANDLESIZE, DesignerUtils.HANDLESIZE);
                        base.hitTestCursor = Cursors.SizeNS;
                        base.rules = SelectionRules.BottomSizeable;
                    }
                    break;

                case GrabHandleGlyphType.MiddleLeft:
                    if (controlBounds.Height >= ((2 * DesignerUtils.HANDLEOVERLAP) + (2 * DesignerUtils.HANDLESIZE)))
                    {
                        base.bounds = new Rectangle((controlBounds.X + DesignerUtils.HANDLEOVERLAP) - DesignerUtils.HANDLESIZE, (controlBounds.Y + (controlBounds.Height / 2)) - (DesignerUtils.HANDLESIZE / 2), DesignerUtils.HANDLESIZE, DesignerUtils.HANDLESIZE);
                        base.hitTestCursor = Cursors.SizeWE;
                        base.rules = SelectionRules.LeftSizeable;
                    }
                    break;

                case GrabHandleGlyphType.MiddleRight:
                    if (controlBounds.Height >= ((2 * DesignerUtils.HANDLEOVERLAP) + (2 * DesignerUtils.HANDLESIZE)))
                    {
                        base.bounds = new Rectangle(controlBounds.Right - DesignerUtils.HANDLEOVERLAP, (controlBounds.Y + (controlBounds.Height / 2)) - (DesignerUtils.HANDLESIZE / 2), DesignerUtils.HANDLESIZE, DesignerUtils.HANDLESIZE);
                        base.hitTestCursor = Cursors.SizeWE;
                        base.rules = SelectionRules.RightSizeable;
                    }
                    break;
            }
            base.hitBounds = base.bounds;
        }

        public override void Paint(PaintEventArgs pe)
        {
            DesignerUtils.DrawGrabHandle(pe.Graphics, base.bounds, this.isPrimary, this);
        }
    }
}

