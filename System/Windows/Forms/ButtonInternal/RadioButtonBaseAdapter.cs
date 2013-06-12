namespace System.Windows.Forms.ButtonInternal
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Internal;

    internal abstract class RadioButtonBaseAdapter : CheckableControlBaseAdapter
    {
        internal RadioButtonBaseAdapter(ButtonBase control) : base(control)
        {
        }

        internal override ButtonBaseAdapter.LayoutOptions CommonLayout()
        {
            ButtonBaseAdapter.LayoutOptions options = base.CommonLayout();
            options.checkAlign = this.Control.CheckAlign;
            return options;
        }

        private static void DrawAndFillEllipse(WindowsGraphics wg, WindowsPen borderPen, WindowsBrush fieldBrush, Rectangle bounds)
        {
            if (wg != null)
            {
                wg.FillRectangle(fieldBrush, new Rectangle(bounds.X + 2, bounds.Y + 2, 8, 8));
                wg.FillRectangle(fieldBrush, new Rectangle(bounds.X + 4, bounds.Y + 1, 4, 10));
                wg.FillRectangle(fieldBrush, new Rectangle(bounds.X + 1, bounds.Y + 4, 10, 4));
                wg.DrawLine(borderPen, new Point(bounds.X + 4, bounds.Y), new Point(bounds.X + 8, bounds.Y));
                wg.DrawLine(borderPen, new Point(bounds.X + 4, bounds.Y + 11), new Point(bounds.X + 8, bounds.Y + 11));
                wg.DrawLine(borderPen, new Point(bounds.X + 2, bounds.Y + 1), new Point(bounds.X + 4, bounds.Y + 1));
                wg.DrawLine(borderPen, new Point(bounds.X + 8, bounds.Y + 1), new Point(bounds.X + 10, bounds.Y + 1));
                wg.DrawLine(borderPen, new Point(bounds.X + 2, bounds.Y + 10), new Point(bounds.X + 4, bounds.Y + 10));
                wg.DrawLine(borderPen, new Point(bounds.X + 8, bounds.Y + 10), new Point(bounds.X + 10, bounds.Y + 10));
                wg.DrawLine(borderPen, new Point(bounds.X, bounds.Y + 4), new Point(bounds.X, bounds.Y + 8));
                wg.DrawLine(borderPen, new Point(bounds.X + 11, bounds.Y + 4), new Point(bounds.X + 11, bounds.Y + 8));
                wg.DrawLine(borderPen, new Point(bounds.X + 1, bounds.Y + 2), new Point(bounds.X + 1, bounds.Y + 4));
                wg.DrawLine(borderPen, new Point(bounds.X + 1, bounds.Y + 8), new Point(bounds.X + 1, bounds.Y + 10));
                wg.DrawLine(borderPen, new Point(bounds.X + 10, bounds.Y + 2), new Point(bounds.X + 10, bounds.Y + 4));
                wg.DrawLine(borderPen, new Point(bounds.X + 10, bounds.Y + 8), new Point(bounds.X + 10, bounds.Y + 10));
            }
        }

        protected void DrawCheckBackground3DLite(PaintEventArgs e, Rectangle bounds, Color checkColor, Color checkBackground, ButtonBaseAdapter.ColorData colors, bool disabledColors)
        {
            Graphics graphics = e.Graphics;
            Color control = checkBackground;
            if (!this.Control.Enabled && disabledColors)
            {
                control = SystemColors.Control;
            }
            using (Brush brush = new SolidBrush(control))
            {
                using (Pen pen = new Pen(colors.buttonShadow))
                {
                    using (Pen pen2 = new Pen(colors.buttonFace))
                    {
                        using (Pen pen3 = new Pen(colors.highlight))
                        {
                            bounds.Width--;
                            bounds.Height--;
                            graphics.DrawPie(pen, bounds, 136f, 88f);
                            graphics.DrawPie(pen, bounds, 226f, 88f);
                            graphics.DrawPie(pen3, bounds, 316f, 88f);
                            graphics.DrawPie(pen3, bounds, 46f, 88f);
                            bounds.Inflate(-1, -1);
                            graphics.FillEllipse(brush, bounds);
                            graphics.DrawEllipse(pen2, bounds);
                        }
                    }
                }
            }
        }

        protected void DrawCheckBackgroundFlat(PaintEventArgs e, Rectangle bounds, Color borderColor, Color checkBackground, bool disabledColors)
        {
            Color control = checkBackground;
            Color contrastControlDark = borderColor;
            if (!this.Control.Enabled && disabledColors)
            {
                contrastControlDark = ControlPaint.ContrastControlDark;
                control = SystemColors.Control;
            }
            float dpiScaleRatio = CheckableControlBaseAdapter.GetDpiScaleRatio(e.Graphics);
            using (WindowsGraphics graphics = WindowsGraphics.FromGraphics(e.Graphics))
            {
                using (WindowsPen pen = new WindowsPen(graphics.DeviceContext, contrastControlDark))
                {
                    using (WindowsBrush brush = new WindowsSolidBrush(graphics.DeviceContext, control))
                    {
                        if (dpiScaleRatio > 1.1)
                        {
                            bounds.Width--;
                            bounds.Height--;
                            graphics.DrawAndFillEllipse(pen, brush, bounds);
                            bounds.Inflate(-1, -1);
                        }
                        else
                        {
                            DrawAndFillEllipse(graphics, pen, brush, bounds);
                        }
                    }
                }
            }
        }

        protected void DrawCheckBox(PaintEventArgs e, ButtonBaseAdapter.LayoutData layout)
        {
            Graphics g = e.Graphics;
            Rectangle checkBounds = layout.checkBounds;
            if (!Application.RenderWithVisualStyles)
            {
                checkBounds.X--;
            }
            ButtonState state = this.GetState();
            if (Application.RenderWithVisualStyles)
            {
                RadioButtonRenderer.DrawRadioButton(g, new Point(checkBounds.Left, checkBounds.Top), RadioButtonRenderer.ConvertFromButtonState(state, this.Control.MouseIsOver));
            }
            else
            {
                ControlPaint.DrawRadioButton(g, checkBounds, state);
            }
        }

        protected void DrawCheckFlat(PaintEventArgs e, ButtonBaseAdapter.LayoutData layout, Color checkColor, Color checkBackground, Color checkBorder)
        {
            this.DrawCheckBackgroundFlat(e, layout.checkBounds, checkBorder, checkBackground, true);
            this.DrawCheckOnly(e, layout, checkColor, checkBackground, true);
        }

        protected void DrawCheckOnly(PaintEventArgs e, ButtonBaseAdapter.LayoutData layout, Color checkColor, Color checkBackground, bool disabledColors)
        {
            if (this.Control.Checked)
            {
                if (!this.Control.Enabled && disabledColors)
                {
                    checkColor = SystemColors.ControlDark;
                }
                float dpiScaleRatio = CheckableControlBaseAdapter.GetDpiScaleRatio(e.Graphics);
                using (WindowsGraphics graphics = WindowsGraphics.FromGraphics(e.Graphics))
                {
                    using (WindowsBrush brush = new WindowsSolidBrush(graphics.DeviceContext, checkColor))
                    {
                        int n = 5;
                        Rectangle rect = new Rectangle(layout.checkBounds.X + GetScaledNumber(n, dpiScaleRatio), layout.checkBounds.Y + GetScaledNumber(n - 1, dpiScaleRatio), GetScaledNumber(2, dpiScaleRatio), GetScaledNumber(4, dpiScaleRatio));
                        graphics.FillRectangle(brush, rect);
                        Rectangle rectangle2 = new Rectangle(layout.checkBounds.X + GetScaledNumber(n - 1, dpiScaleRatio), layout.checkBounds.Y + GetScaledNumber(n, dpiScaleRatio), GetScaledNumber(4, dpiScaleRatio), GetScaledNumber(2, dpiScaleRatio));
                        graphics.FillRectangle(brush, rectangle2);
                    }
                }
            }
        }

        private static int GetScaledNumber(int n, float scale)
        {
            return (int) (n * scale);
        }

        protected ButtonState GetState()
        {
            ButtonState normal = ButtonState.Normal;
            if (this.Control.Checked)
            {
                normal |= ButtonState.Checked;
            }
            else
            {
                normal = normal;
            }
            if (!this.Control.Enabled)
            {
                normal |= ButtonState.Inactive;
            }
            if (this.Control.MouseIsDown)
            {
                normal |= ButtonState.Pushed;
            }
            return normal;
        }

        protected RadioButton Control
        {
            get
            {
                return (RadioButton) base.Control;
            }
        }
    }
}

