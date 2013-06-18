namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public class ComponentGlyph : Glyph
    {
        private IComponent relatedComponent;

        public ComponentGlyph(IComponent relatedComponent) : base(null)
        {
            this.relatedComponent = relatedComponent;
        }

        public ComponentGlyph(IComponent relatedComponent, System.Windows.Forms.Design.Behavior.Behavior behavior) : base(behavior)
        {
            this.relatedComponent = relatedComponent;
        }

        public override Cursor GetHitTest(Point p)
        {
            return null;
        }

        public override void Paint(PaintEventArgs pe)
        {
        }

        public IComponent RelatedComponent
        {
            get
            {
                return this.relatedComponent;
            }
        }
    }
}

