namespace System.Windows.Forms.PropertyGridInternal
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.ButtonInternal;

    internal class DropDownButtonAdapter : ButtonStandardAdapter
    {
        internal DropDownButtonAdapter(ButtonBase control) : base(control)
        {
        }

        private void DDB_Draw3DBorder(Graphics g, Rectangle r, bool raised)
        {
            if ((base.Control.BackColor != SystemColors.Control) && SystemInformation.HighContrast)
            {
                if (raised)
                {
                    Color leftColor = ControlPaint.LightLight(base.Control.BackColor);
                    ControlPaint.DrawBorder(g, r, leftColor, 1, ButtonBorderStyle.Outset, leftColor, 1, ButtonBorderStyle.Outset, leftColor, 2, ButtonBorderStyle.Inset, leftColor, 2, ButtonBorderStyle.Inset);
                }
                else
                {
                    ControlPaint.DrawBorder(g, r, ControlPaint.Dark(base.Control.BackColor), ButtonBorderStyle.Solid);
                }
            }
            else if (raised)
            {
                Color color2 = ControlPaint.Light(base.Control.BackColor);
                ControlPaint.DrawBorder(g, r, color2, 1, ButtonBorderStyle.Solid, color2, 1, ButtonBorderStyle.Solid, base.Control.BackColor, 2, ButtonBorderStyle.Outset, base.Control.BackColor, 2, ButtonBorderStyle.Outset);
                Rectangle bounds = r;
                bounds.Offset(1, 1);
                bounds.Width -= 3;
                bounds.Height -= 3;
                color2 = ControlPaint.LightLight(base.Control.BackColor);
                ControlPaint.DrawBorder(g, bounds, color2, 1, ButtonBorderStyle.Solid, color2, 1, ButtonBorderStyle.Solid, color2, 1, ButtonBorderStyle.None, color2, 1, ButtonBorderStyle.None);
            }
            else
            {
                ControlPaint.DrawBorder(g, r, ControlPaint.Dark(base.Control.BackColor), ButtonBorderStyle.Solid);
            }
        }

        internal override void DrawImageCore(Graphics graphics, Image image, Rectangle imageBounds, Point imageStart, ButtonBaseAdapter.LayoutData layout)
        {
            ControlPaint.DrawImageReplaceColor(graphics, image, imageBounds, Color.Black, base.Control.ForeColor);
        }

        internal override void PaintUp(PaintEventArgs pevent, CheckState state)
        {
            base.PaintUp(pevent, state);
            if (!Application.RenderWithVisualStyles)
            {
                this.DDB_Draw3DBorder(pevent.Graphics, base.Control.ClientRectangle, true);
            }
            else
            {
                Color window = SystemColors.Window;
                Rectangle clientRectangle = base.Control.ClientRectangle;
                clientRectangle.Inflate(0, -1);
                ControlPaint.DrawBorder(pevent.Graphics, clientRectangle, window, 1, ButtonBorderStyle.None, window, 1, ButtonBorderStyle.None, window, 1, ButtonBorderStyle.Solid, window, 1, ButtonBorderStyle.None);
            }
        }
    }
}

