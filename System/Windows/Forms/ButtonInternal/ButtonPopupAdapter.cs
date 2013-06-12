namespace System.Windows.Forms.ButtonInternal
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal class ButtonPopupAdapter : ButtonBaseAdapter
    {
        internal ButtonPopupAdapter(ButtonBase control) : base(control)
        {
        }

        protected override ButtonBaseAdapter.LayoutOptions Layout(PaintEventArgs e)
        {
            return this.PaintPopupLayout(e, false, 0);
        }

        internal override void PaintDown(PaintEventArgs e, CheckState state)
        {
            ButtonBaseAdapter.ColorData colors = base.PaintPopupRender(e.Graphics).Calculate();
            ButtonBaseAdapter.LayoutData layout = this.PaintPopupLayout(e, false, SystemInformation.HighContrast ? 2 : 1).Layout();
            Graphics g = e.Graphics;
            Rectangle clientRectangle = base.Control.ClientRectangle;
            base.PaintButtonBackground(e, clientRectangle, null);
            if (base.Control.IsDefault)
            {
                clientRectangle.Inflate(-1, -1);
            }
            clientRectangle.Inflate(-1, -1);
            base.PaintImage(e, layout);
            base.PaintField(e, layout, colors, colors.windowText, true);
            clientRectangle.Inflate(1, 1);
            ButtonBaseAdapter.DrawDefaultBorder(g, clientRectangle, colors.options.highContrast ? colors.windowText : colors.windowFrame, base.Control.IsDefault);
            ControlPaint.DrawBorder(g, clientRectangle, colors.options.highContrast ? colors.windowText : colors.buttonShadow, ButtonBorderStyle.Solid);
        }

        internal override void PaintOver(PaintEventArgs e, CheckState state)
        {
            ButtonBaseAdapter.ColorData colors = base.PaintPopupRender(e.Graphics).Calculate();
            ButtonBaseAdapter.LayoutData layout = this.PaintPopupLayout(e, state == CheckState.Unchecked, SystemInformation.HighContrast ? 2 : 1).Layout();
            Graphics g = e.Graphics;
            Rectangle clientRectangle = base.Control.ClientRectangle;
            Brush background = null;
            if (state == CheckState.Indeterminate)
            {
                background = ButtonBaseAdapter.CreateDitherBrush(colors.highlight, colors.buttonFace);
            }
            try
            {
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
            if (base.Control.IsDefault)
            {
                clientRectangle.Inflate(-1, -1);
            }
            base.PaintImage(e, layout);
            base.PaintField(e, layout, colors, colors.windowText, true);
            ButtonBaseAdapter.DrawDefaultBorder(g, clientRectangle, colors.options.highContrast ? colors.windowText : colors.buttonShadow, base.Control.IsDefault);
            if (SystemInformation.HighContrast)
            {
                using (Pen pen = new Pen(colors.windowFrame))
                {
                    using (Pen pen2 = new Pen(colors.highlight))
                    {
                        using (Pen pen3 = new Pen(colors.buttonShadow))
                        {
                            g.DrawLine(pen, (int) (clientRectangle.Left + 1), (int) (clientRectangle.Top + 1), (int) (clientRectangle.Right - 2), (int) (clientRectangle.Top + 1));
                            g.DrawLine(pen, (int) (clientRectangle.Left + 1), (int) (clientRectangle.Top + 1), (int) (clientRectangle.Left + 1), (int) (clientRectangle.Bottom - 2));
                            g.DrawLine(pen, clientRectangle.Left, clientRectangle.Bottom - 1, clientRectangle.Right, clientRectangle.Bottom - 1);
                            g.DrawLine(pen, clientRectangle.Right - 1, clientRectangle.Top, clientRectangle.Right - 1, clientRectangle.Bottom);
                            g.DrawLine(pen2, clientRectangle.Left, clientRectangle.Top, clientRectangle.Right, clientRectangle.Top);
                            g.DrawLine(pen2, clientRectangle.Left, clientRectangle.Top, clientRectangle.Left, clientRectangle.Bottom);
                            g.DrawLine(pen3, (int) (clientRectangle.Left + 1), (int) (clientRectangle.Bottom - 2), (int) (clientRectangle.Right - 2), (int) (clientRectangle.Bottom - 2));
                            g.DrawLine(pen3, (int) (clientRectangle.Right - 2), (int) (clientRectangle.Top + 1), (int) (clientRectangle.Right - 2), (int) (clientRectangle.Bottom - 2));
                        }
                    }
                }
                clientRectangle.Inflate(-2, -2);
            }
            else
            {
                ButtonBaseAdapter.Draw3DLiteBorder(g, clientRectangle, colors, true);
            }
        }

        private ButtonBaseAdapter.LayoutOptions PaintPopupLayout(PaintEventArgs e, bool up, int paintedBorder)
        {
            ButtonBaseAdapter.LayoutOptions options = this.CommonLayout();
            options.borderSize = paintedBorder;
            options.paddingSize = 2 - paintedBorder;
            options.hintTextUp = false;
            options.textOffset = !up;
            options.shadowedText = SystemInformation.HighContrast;
            return options;
        }

        internal static ButtonBaseAdapter.LayoutOptions PaintPopupLayout(Graphics g, bool up, int paintedBorder, Rectangle clientRectangle, Padding padding, bool isDefault, Font font, string text, bool enabled, ContentAlignment textAlign, RightToLeft rtl)
        {
            ButtonBaseAdapter.LayoutOptions options = ButtonBaseAdapter.CommonLayout(clientRectangle, padding, isDefault, font, text, enabled, textAlign, rtl);
            options.borderSize = paintedBorder;
            options.paddingSize = 2 - paintedBorder;
            options.hintTextUp = false;
            options.textOffset = !up;
            options.shadowedText = SystemInformation.HighContrast;
            return options;
        }

        internal override void PaintUp(PaintEventArgs e, CheckState state)
        {
            ButtonBaseAdapter.ColorData colors = base.PaintPopupRender(e.Graphics).Calculate();
            ButtonBaseAdapter.LayoutData layout = this.PaintPopupLayout(e, state == CheckState.Unchecked, 1).Layout();
            Graphics g = e.Graphics;
            Rectangle clientRectangle = base.Control.ClientRectangle;
            Brush background = null;
            if (state == CheckState.Indeterminate)
            {
                background = ButtonBaseAdapter.CreateDitherBrush(colors.highlight, colors.buttonFace);
            }
            try
            {
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
            if (base.Control.IsDefault)
            {
                clientRectangle.Inflate(-1, -1);
            }
            base.PaintImage(e, layout);
            base.PaintField(e, layout, colors, colors.windowText, true);
            ButtonBaseAdapter.DrawDefaultBorder(g, clientRectangle, colors.options.highContrast ? colors.windowText : colors.buttonShadow, base.Control.IsDefault);
            if (state == CheckState.Unchecked)
            {
                ButtonBaseAdapter.DrawFlatBorder(g, clientRectangle, colors.options.highContrast ? colors.windowText : colors.buttonShadow);
            }
            else
            {
                ButtonBaseAdapter.Draw3DLiteBorder(g, clientRectangle, colors, false);
            }
        }
    }
}

