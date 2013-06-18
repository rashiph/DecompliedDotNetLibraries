namespace System.Windows.Forms.ButtonInternal
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    internal class ButtonStandardAdapter : ButtonBaseAdapter
    {
        private const int borderWidth = 2;

        internal ButtonStandardAdapter(ButtonBase control) : base(control)
        {
        }

        private PushButtonState DetermineState(bool up)
        {
            PushButtonState normal = PushButtonState.Normal;
            if (!up)
            {
                return PushButtonState.Pressed;
            }
            if (base.Control.MouseIsOver)
            {
                return PushButtonState.Hot;
            }
            if (!base.Control.Enabled)
            {
                return PushButtonState.Disabled;
            }
            if (!base.Control.Focused && !base.Control.IsDefault)
            {
                return normal;
            }
            return PushButtonState.Default;
        }

        protected override ButtonBaseAdapter.LayoutOptions Layout(PaintEventArgs e)
        {
            return this.PaintLayout(e, false);
        }

        internal override void PaintDown(PaintEventArgs e, CheckState state)
        {
            this.PaintWorker(e, false, state);
        }

        private ButtonBaseAdapter.LayoutOptions PaintLayout(PaintEventArgs e, bool up)
        {
            ButtonBaseAdapter.LayoutOptions options = this.CommonLayout();
            options.textOffset = !up;
            options.everettButtonCompat = !Application.RenderWithVisualStyles;
            return options;
        }

        internal override void PaintOver(PaintEventArgs e, CheckState state)
        {
            this.PaintUp(e, state);
        }

        private void PaintThemedButtonBackground(PaintEventArgs e, Rectangle bounds, bool up)
        {
            PushButtonState state = this.DetermineState(up);
            if (ButtonRenderer.IsBackgroundPartiallyTransparent(state))
            {
                ButtonRenderer.DrawParentBackground(e.Graphics, bounds, base.Control);
            }
            ButtonRenderer.DrawButton(e.Graphics, base.Control.ClientRectangle, false, state);
            bounds.Inflate(-ButtonBaseAdapter.buttonBorderSize, -ButtonBaseAdapter.buttonBorderSize);
            if (!base.Control.UseVisualStyleBackColor)
            {
                bool flag = false;
                Color backColor = base.Control.BackColor;
                if (((backColor.A == 0xff) && (e.HDC != IntPtr.Zero)) && (DisplayInformation.BitsPerPixel > 8))
                {
                    System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT(bounds.X, bounds.Y, bounds.Right, bounds.Bottom);
                    System.Windows.Forms.SafeNativeMethods.FillRect(new HandleRef(e, e.HDC), ref rect, new HandleRef(this, base.Control.BackColorBrush));
                    flag = true;
                }
                if (!flag && (backColor.A > 0))
                {
                    if (backColor.A == 0xff)
                    {
                        backColor = e.Graphics.GetNearestColor(backColor);
                    }
                    using (Brush brush = new SolidBrush(backColor))
                    {
                        e.Graphics.FillRectangle(brush, bounds);
                    }
                }
            }
            if ((base.Control.BackgroundImage != null) && !DisplayInformation.HighContrast)
            {
                ControlPaint.DrawBackgroundImage(e.Graphics, base.Control.BackgroundImage, Color.Transparent, base.Control.BackgroundImageLayout, base.Control.ClientRectangle, bounds, base.Control.DisplayRectangle.Location, base.Control.RightToLeft);
            }
        }

        internal override void PaintUp(PaintEventArgs e, CheckState state)
        {
            this.PaintWorker(e, true, state);
        }

        private void PaintWorker(PaintEventArgs e, bool up, CheckState state)
        {
            ButtonBaseAdapter.LayoutData data2;
            up = up && (state == CheckState.Unchecked);
            ButtonBaseAdapter.ColorData colors = base.PaintRender(e.Graphics).Calculate();
            if (Application.RenderWithVisualStyles)
            {
                data2 = this.PaintLayout(e, true).Layout();
            }
            else
            {
                data2 = this.PaintLayout(e, up).Layout();
            }
            Graphics g = e.Graphics;
            ButtonBase control = base.Control;
            if (Application.RenderWithVisualStyles)
            {
                this.PaintThemedButtonBackground(e, base.Control.ClientRectangle, up);
            }
            else
            {
                Brush background = null;
                if (state == CheckState.Indeterminate)
                {
                    background = ButtonBaseAdapter.CreateDitherBrush(colors.highlight, colors.buttonFace);
                }
                try
                {
                    Rectangle clientRectangle = base.Control.ClientRectangle;
                    if (up)
                    {
                        clientRectangle.Inflate(-2, -2);
                    }
                    else
                    {
                        clientRectangle.Inflate(-1, -1);
                    }
                    base.PaintButtonBackground(e, clientRectangle, background);
                }
                finally
                {
                    if (background != null)
                    {
                        background.Dispose();
                        background = null;
                    }
                }
            }
            base.PaintImage(e, data2);
            if (Application.RenderWithVisualStyles)
            {
                data2.focus.Inflate(1, 1);
            }
            base.PaintField(e, data2, colors, colors.windowText, true);
            if (!Application.RenderWithVisualStyles)
            {
                Rectangle r = base.Control.ClientRectangle;
                if (base.Control.IsDefault)
                {
                    r.Inflate(-1, -1);
                }
                ButtonBaseAdapter.DrawDefaultBorder(g, r, colors.windowFrame, base.Control.IsDefault);
                if (up)
                {
                    base.Draw3DBorder(g, r, colors, up);
                }
                else
                {
                    ControlPaint.DrawBorder(g, r, colors.buttonShadow, ButtonBorderStyle.Solid);
                }
            }
        }
    }
}

