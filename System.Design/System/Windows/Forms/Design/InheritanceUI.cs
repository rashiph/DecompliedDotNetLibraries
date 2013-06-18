namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing;
    using System.Windows.Forms;

    internal class InheritanceUI
    {
        private static Bitmap inheritanceGlyph;
        private static Rectangle inheritanceGlyphRect;
        private ToolTip tooltip;

        public void AddInheritedControl(Control c, InheritanceLevel level)
        {
            string str;
            if (this.tooltip == null)
            {
                this.tooltip = new ToolTip();
                this.tooltip.ShowAlways = true;
            }
            if (level == InheritanceLevel.InheritedReadOnly)
            {
                str = System.Design.SR.GetString("DesignerInheritedReadOnly");
            }
            else
            {
                str = System.Design.SR.GetString("DesignerInherited");
            }
            this.tooltip.SetToolTip(c, str);
            foreach (Control control in c.Controls)
            {
                if (control.Site == null)
                {
                    this.tooltip.SetToolTip(control, str);
                }
            }
        }

        public void Dispose()
        {
            if (this.tooltip != null)
            {
                this.tooltip.Dispose();
            }
        }

        public void RemoveInheritedControl(Control c)
        {
            if ((this.tooltip != null) && (this.tooltip.GetToolTip(c).Length > 0))
            {
                this.tooltip.SetToolTip(c, null);
                foreach (Control control in c.Controls)
                {
                    if (control.Site == null)
                    {
                        this.tooltip.SetToolTip(control, null);
                    }
                }
            }
        }

        public Bitmap InheritanceGlyph
        {
            get
            {
                if (inheritanceGlyph == null)
                {
                    inheritanceGlyph = new Bitmap(typeof(InheritanceUI), "InheritedGlyph.bmp");
                    inheritanceGlyph.MakeTransparent();
                }
                return inheritanceGlyph;
            }
        }

        public Rectangle InheritanceGlyphRectangle
        {
            get
            {
                if (inheritanceGlyphRect == Rectangle.Empty)
                {
                    Size size = this.InheritanceGlyph.Size;
                    inheritanceGlyphRect = new Rectangle(0, 0, size.Width, size.Height);
                }
                return inheritanceGlyphRect;
            }
        }
    }
}

