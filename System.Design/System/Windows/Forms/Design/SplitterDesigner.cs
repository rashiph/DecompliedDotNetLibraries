namespace System.Windows.Forms.Design
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    internal class SplitterDesigner : ControlDesigner
    {
        public SplitterDesigner()
        {
            base.AutoResizeHandles = true;
        }

        private void DrawBorder(Graphics graphics)
        {
            Color white;
            Control control = this.Control;
            Rectangle clientRectangle = control.ClientRectangle;
            if (control.BackColor.GetBrightness() < 0.5)
            {
                white = Color.White;
            }
            else
            {
                white = Color.Black;
            }
            using (Pen pen = new Pen(white))
            {
                pen.DashStyle = DashStyle.Dash;
                clientRectangle.Width--;
                clientRectangle.Height--;
                graphics.DrawRectangle(pen, clientRectangle);
            }
        }

        protected override void OnPaintAdornments(PaintEventArgs pe)
        {
            Splitter component = (Splitter) base.Component;
            base.OnPaintAdornments(pe);
            if (component.BorderStyle == BorderStyle.None)
            {
                this.DrawBorder(pe.Graphics);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x47)
            {
                this.Control.Invalidate();
            }
            base.WndProc(ref m);
        }
    }
}

