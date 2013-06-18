namespace System.Windows.Forms.Design
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal class GroupBoxDesigner : ParentControlDesigner
    {
        private InheritanceUI inheritanceUI;

        protected override void OnPaintAdornments(PaintEventArgs pe)
        {
            if (this.DrawGrid)
            {
                Control control = this.Control;
                Rectangle displayRectangle = this.Control.DisplayRectangle;
                displayRectangle.Width++;
                displayRectangle.Height++;
                ControlPaint.DrawGrid(pe.Graphics, displayRectangle, base.GridSize, control.BackColor);
            }
            if (base.Inherited)
            {
                if (this.inheritanceUI == null)
                {
                    this.inheritanceUI = (InheritanceUI) this.GetService(typeof(InheritanceUI));
                }
                if (this.inheritanceUI != null)
                {
                    pe.Graphics.DrawImage(this.inheritanceUI.InheritanceGlyph, 0, 0);
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x84)
            {
                base.WndProc(ref m);
                if (((int) ((long) m.Result)) == -1)
                {
                    m.Result = (IntPtr) 1;
                }
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        protected override Point DefaultControlLocation
        {
            get
            {
                GroupBox control = (GroupBox) this.Control;
                return new Point(control.DisplayRectangle.X, control.DisplayRectangle.Y);
            }
        }
    }
}

