namespace System.Windows.Forms.ButtonInternal
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal class ButtonFlatAdapter : ButtonBaseAdapter
    {
        private const int BORDERSIZE = 1;

        internal ButtonFlatAdapter(ButtonBase control) : base(control)
        {
        }

        protected override ButtonBaseAdapter.LayoutOptions Layout(PaintEventArgs e)
        {
            return this.PaintFlatLayout(e, false, true, base.Control.FlatAppearance.BorderSize);
        }

        private void PaintBackground(PaintEventArgs e, Rectangle r, Color backColor)
        {
            Rectangle rectangle = r;
            rectangle.Inflate(-base.Control.FlatAppearance.BorderSize, -base.Control.FlatAppearance.BorderSize);
            base.Control.PaintBackground(e, rectangle, backColor, rectangle.Location);
        }

        internal override void PaintDown(PaintEventArgs e, CheckState state)
        {
            bool flag = (base.Control.FlatAppearance.BorderSize != 1) || !base.Control.FlatAppearance.BorderColor.IsEmpty;
            ButtonBaseAdapter.ColorData colors = base.PaintFlatRender(e.Graphics).Calculate();
            ButtonBaseAdapter.LayoutData layout = this.PaintFlatLayout(e, !base.Control.FlatAppearance.CheckedBackColor.IsEmpty || (SystemInformation.HighContrast ? (state != CheckState.Indeterminate) : (state == CheckState.Unchecked)), (!flag && SystemInformation.HighContrast) && (state == CheckState.Checked), base.Control.FlatAppearance.BorderSize).Layout();
            if (!base.Control.FlatAppearance.BorderColor.IsEmpty)
            {
                colors.windowFrame = base.Control.FlatAppearance.BorderColor;
            }
            Graphics g = e.Graphics;
            Rectangle clientRectangle = base.Control.ClientRectangle;
            Color backColor = base.Control.BackColor;
            if (!base.Control.FlatAppearance.MouseDownBackColor.IsEmpty)
            {
                backColor = base.Control.FlatAppearance.MouseDownBackColor;
            }
            else
            {
                switch (state)
                {
                    case CheckState.Unchecked:
                    case CheckState.Checked:
                        backColor = colors.options.highContrast ? colors.buttonShadow : colors.lowHighlight;
                        break;

                    case CheckState.Indeterminate:
                        backColor = ButtonBaseAdapter.MixedColor(colors.options.highContrast ? colors.buttonShadow : colors.lowHighlight, colors.buttonFace);
                        break;
                }
            }
            this.PaintBackground(e, clientRectangle, backColor);
            if (base.Control.IsDefault)
            {
                clientRectangle.Inflate(-1, -1);
            }
            base.PaintImage(e, layout);
            base.PaintField(e, layout, colors, colors.windowText, false);
            if (base.Control.Focused && base.Control.ShowFocusCues)
            {
                ButtonBaseAdapter.DrawFlatFocus(g, layout.focus, colors.options.highContrast ? colors.windowText : colors.constrastButtonShadow);
            }
            if ((!base.Control.IsDefault || !base.Control.Focused) || (base.Control.FlatAppearance.BorderSize != 0))
            {
                ButtonBaseAdapter.DrawDefaultBorder(g, clientRectangle, colors.windowFrame, base.Control.IsDefault);
            }
            if (flag)
            {
                if (base.Control.FlatAppearance.BorderSize != 1)
                {
                    ButtonBaseAdapter.DrawFlatBorderWithSize(g, clientRectangle, colors.windowFrame, base.Control.FlatAppearance.BorderSize);
                }
                else
                {
                    ButtonBaseAdapter.DrawFlatBorder(g, clientRectangle, colors.windowFrame);
                }
            }
            else if ((state == CheckState.Checked) && SystemInformation.HighContrast)
            {
                ButtonBaseAdapter.DrawFlatBorder(g, clientRectangle, colors.windowFrame);
                ButtonBaseAdapter.DrawFlatBorder(g, clientRectangle, colors.buttonShadow);
            }
            else if (state == CheckState.Indeterminate)
            {
                ButtonBaseAdapter.Draw3DLiteBorder(g, clientRectangle, colors, false);
            }
            else
            {
                ButtonBaseAdapter.DrawFlatBorder(g, clientRectangle, colors.windowFrame);
            }
        }

        private ButtonBaseAdapter.LayoutOptions PaintFlatLayout(PaintEventArgs e, bool up, bool check, int borderSize)
        {
            ButtonBaseAdapter.LayoutOptions options = this.CommonLayout();
            options.borderSize = borderSize + (check ? 1 : 0);
            options.paddingSize = check ? 1 : 2;
            options.focusOddEvenFixup = false;
            options.textOffset = !up;
            options.shadowedText = SystemInformation.HighContrast;
            return options;
        }

        internal static ButtonBaseAdapter.LayoutOptions PaintFlatLayout(Graphics g, bool up, bool check, int borderSize, Rectangle clientRectangle, Padding padding, bool isDefault, Font font, string text, bool enabled, ContentAlignment textAlign, RightToLeft rtl)
        {
            ButtonBaseAdapter.LayoutOptions options = ButtonBaseAdapter.CommonLayout(clientRectangle, padding, isDefault, font, text, enabled, textAlign, rtl);
            options.borderSize = borderSize + (check ? 1 : 0);
            options.paddingSize = check ? 1 : 2;
            options.focusOddEvenFixup = false;
            options.textOffset = !up;
            options.shadowedText = SystemInformation.HighContrast;
            return options;
        }

        internal override void PaintOver(PaintEventArgs e, CheckState state)
        {
            if (SystemInformation.HighContrast)
            {
                this.PaintUp(e, state);
            }
            else
            {
                bool flag = (base.Control.FlatAppearance.BorderSize != 1) || !base.Control.FlatAppearance.BorderColor.IsEmpty;
                ButtonBaseAdapter.ColorData colors = base.PaintFlatRender(e.Graphics).Calculate();
                ButtonBaseAdapter.LayoutData layout = this.PaintFlatLayout(e, !base.Control.FlatAppearance.CheckedBackColor.IsEmpty || (state == CheckState.Unchecked), false, base.Control.FlatAppearance.BorderSize).Layout();
                if (!base.Control.FlatAppearance.BorderColor.IsEmpty)
                {
                    colors.windowFrame = base.Control.FlatAppearance.BorderColor;
                }
                Graphics g = e.Graphics;
                Rectangle clientRectangle = base.Control.ClientRectangle;
                Color backColor = base.Control.BackColor;
                if (!base.Control.FlatAppearance.MouseOverBackColor.IsEmpty)
                {
                    backColor = base.Control.FlatAppearance.MouseOverBackColor;
                }
                else if (!base.Control.FlatAppearance.CheckedBackColor.IsEmpty)
                {
                    if ((state == CheckState.Checked) || (state == CheckState.Indeterminate))
                    {
                        backColor = ButtonBaseAdapter.MixedColor(base.Control.FlatAppearance.CheckedBackColor, colors.lowButtonFace);
                    }
                    else
                    {
                        backColor = colors.lowButtonFace;
                    }
                }
                else if (state == CheckState.Indeterminate)
                {
                    backColor = ButtonBaseAdapter.MixedColor(colors.buttonFace, colors.lowButtonFace);
                }
                else
                {
                    backColor = colors.lowButtonFace;
                }
                this.PaintBackground(e, clientRectangle, backColor);
                if (base.Control.IsDefault)
                {
                    clientRectangle.Inflate(-1, -1);
                }
                base.PaintImage(e, layout);
                base.PaintField(e, layout, colors, colors.windowText, false);
                if (base.Control.Focused && base.Control.ShowFocusCues)
                {
                    ButtonBaseAdapter.DrawFlatFocus(g, layout.focus, colors.constrastButtonShadow);
                }
                if ((!base.Control.IsDefault || !base.Control.Focused) || (base.Control.FlatAppearance.BorderSize != 0))
                {
                    ButtonBaseAdapter.DrawDefaultBorder(g, clientRectangle, colors.windowFrame, base.Control.IsDefault);
                }
                if (flag)
                {
                    if (base.Control.FlatAppearance.BorderSize != 1)
                    {
                        ButtonBaseAdapter.DrawFlatBorderWithSize(g, clientRectangle, colors.windowFrame, base.Control.FlatAppearance.BorderSize);
                    }
                    else
                    {
                        ButtonBaseAdapter.DrawFlatBorder(g, clientRectangle, colors.windowFrame);
                    }
                }
                else if (state == CheckState.Unchecked)
                {
                    ButtonBaseAdapter.DrawFlatBorder(g, clientRectangle, colors.windowFrame);
                }
                else
                {
                    ButtonBaseAdapter.Draw3DLiteBorder(g, clientRectangle, colors, false);
                }
            }
        }

        internal override void PaintUp(PaintEventArgs e, CheckState state)
        {
            bool flag = (base.Control.FlatAppearance.BorderSize != 1) || !base.Control.FlatAppearance.BorderColor.IsEmpty;
            ButtonBaseAdapter.ColorData colors = base.PaintFlatRender(e.Graphics).Calculate();
            ButtonBaseAdapter.LayoutData layout = this.PaintFlatLayout(e, !base.Control.FlatAppearance.CheckedBackColor.IsEmpty || (SystemInformation.HighContrast ? (state != CheckState.Indeterminate) : (state == CheckState.Unchecked)), (!flag && SystemInformation.HighContrast) && (state == CheckState.Checked), base.Control.FlatAppearance.BorderSize).Layout();
            if (!base.Control.FlatAppearance.BorderColor.IsEmpty)
            {
                colors.windowFrame = base.Control.FlatAppearance.BorderColor;
            }
            Graphics g = e.Graphics;
            Rectangle clientRectangle = base.Control.ClientRectangle;
            Color backColor = base.Control.BackColor;
            if (!base.Control.FlatAppearance.CheckedBackColor.IsEmpty)
            {
                switch (state)
                {
                    case CheckState.Checked:
                        backColor = base.Control.FlatAppearance.CheckedBackColor;
                        break;

                    case CheckState.Indeterminate:
                        backColor = ButtonBaseAdapter.MixedColor(base.Control.FlatAppearance.CheckedBackColor, colors.buttonFace);
                        break;
                }
            }
            else
            {
                switch (state)
                {
                    case CheckState.Checked:
                        backColor = colors.highlight;
                        break;

                    case CheckState.Indeterminate:
                        backColor = ButtonBaseAdapter.MixedColor(colors.highlight, colors.buttonFace);
                        break;
                }
            }
            this.PaintBackground(e, clientRectangle, backColor);
            if (base.Control.IsDefault)
            {
                clientRectangle.Inflate(-1, -1);
            }
            base.PaintImage(e, layout);
            base.PaintField(e, layout, colors, colors.windowText, false);
            if (base.Control.Focused && base.Control.ShowFocusCues)
            {
                ButtonBaseAdapter.DrawFlatFocus(g, layout.focus, colors.options.highContrast ? colors.windowText : colors.constrastButtonShadow);
            }
            if ((!base.Control.IsDefault || !base.Control.Focused) || (base.Control.FlatAppearance.BorderSize != 0))
            {
                ButtonBaseAdapter.DrawDefaultBorder(g, clientRectangle, colors.windowFrame, base.Control.IsDefault);
            }
            if (flag)
            {
                if (base.Control.FlatAppearance.BorderSize != 1)
                {
                    ButtonBaseAdapter.DrawFlatBorderWithSize(g, clientRectangle, colors.windowFrame, base.Control.FlatAppearance.BorderSize);
                }
                else
                {
                    ButtonBaseAdapter.DrawFlatBorder(g, clientRectangle, colors.windowFrame);
                }
            }
            else if ((state == CheckState.Checked) && SystemInformation.HighContrast)
            {
                ButtonBaseAdapter.DrawFlatBorder(g, clientRectangle, colors.windowFrame);
                ButtonBaseAdapter.DrawFlatBorder(g, clientRectangle, colors.buttonShadow);
            }
            else if (state == CheckState.Indeterminate)
            {
                ButtonBaseAdapter.Draw3DLiteBorder(g, clientRectangle, colors, false);
            }
            else
            {
                ButtonBaseAdapter.DrawFlatBorder(g, clientRectangle, colors.windowFrame);
            }
        }
    }
}

