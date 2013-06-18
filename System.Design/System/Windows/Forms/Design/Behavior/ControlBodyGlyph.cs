namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    public class ControlBodyGlyph : ComponentGlyph
    {
        private Rectangle bounds;
        private IComponent component;
        private Cursor hitTestCursor;

        public ControlBodyGlyph(Rectangle bounds, Cursor cursor, IComponent relatedComponent, System.Windows.Forms.Design.Behavior.Behavior behavior) : base(relatedComponent, behavior)
        {
            this.bounds = bounds;
            this.hitTestCursor = cursor;
            this.component = relatedComponent;
        }

        public ControlBodyGlyph(Rectangle bounds, Cursor cursor, IComponent relatedComponent, ControlDesigner designer) : base(relatedComponent, new ControlDesigner.TransparentBehavior(designer))
        {
            this.bounds = bounds;
            this.hitTestCursor = cursor;
            this.component = relatedComponent;
        }

        public override Cursor GetHitTest(Point p)
        {
            if (((this.component is Control) ? ((Control) this.component).Visible : true) && this.bounds.Contains(p))
            {
                return this.hitTestCursor;
            }
            return null;
        }

        public override Rectangle Bounds
        {
            get
            {
                return this.bounds;
            }
        }
    }
}

