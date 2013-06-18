namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;

    public sealed class ShadowGlyph : DesignerGlyph
    {
        private static ShadowGlyph defaultShadowGlyph;

        public override Rectangle GetBounds(ActivityDesigner designer, bool activated)
        {
            if (designer == null)
            {
                throw new ArgumentNullException("designer");
            }
            Rectangle bounds = designer.Bounds;
            bounds.Inflate(5, 5);
            return bounds;
        }

        protected override void OnPaint(Graphics graphics, bool activated, AmbientTheme ambientTheme, ActivityDesigner designer)
        {
            if (!this.GetBounds(designer, activated).Size.IsEmpty)
            {
                bool roundEdges = (designer.DesignerTheme.DesignerGeometry == DesignerGeometry.RoundedRectangle) && !designer.IsRootDesigner;
                ActivityDesignerPaint.DrawDropShadow(graphics, designer.Bounds, designer.DesignerTheme.BorderPen.Color, 4, LightSourcePosition.Top | LightSourcePosition.Left, 0.5f, roundEdges);
            }
        }

        internal static ShadowGlyph Default
        {
            get
            {
                if (defaultShadowGlyph == null)
                {
                    defaultShadowGlyph = new ShadowGlyph();
                }
                return defaultShadowGlyph;
            }
        }

        public override int Priority
        {
            get
            {
                return 0xf4240;
            }
        }
    }
}

