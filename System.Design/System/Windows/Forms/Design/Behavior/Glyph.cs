namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public abstract class Glyph
    {
        private System.Windows.Forms.Design.Behavior.Behavior behavior;

        protected Glyph(System.Windows.Forms.Design.Behavior.Behavior behavior)
        {
            this.behavior = behavior;
        }

        public abstract Cursor GetHitTest(Point p);
        public abstract void Paint(PaintEventArgs pe);
        protected void SetBehavior(System.Windows.Forms.Design.Behavior.Behavior behavior)
        {
            this.behavior = behavior;
        }

        public virtual System.Windows.Forms.Design.Behavior.Behavior Behavior
        {
            get
            {
                return this.behavior;
            }
        }

        public virtual Rectangle Bounds
        {
            get
            {
                return Rectangle.Empty;
            }
        }
    }
}

