namespace System.Windows.Forms.ButtonInternal
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.Internal;

    internal abstract class CheckBoxBaseAdapter : CheckableControlBaseAdapter
    {
        [ThreadStatic]
        private static Bitmap checkImageChecked = null;
        [ThreadStatic]
        private static Color checkImageCheckedBackColor = Color.Empty;
        [ThreadStatic]
        private static Bitmap checkImageIndeterminate = null;
        [ThreadStatic]
        private static Color checkImageIndeterminateBackColor = Color.Empty;
        protected const int flatCheckSize = 11;

        internal CheckBoxBaseAdapter(ButtonBase control) : base(control)
        {
        }

        internal override ButtonBaseAdapter.LayoutOptions CommonLayout()
        {
            ButtonBaseAdapter.LayoutOptions options = base.CommonLayout();
            options.checkAlign = this.Control.CheckAlign;
            options.textOffset = false;
            options.shadowedText = !this.Control.Enabled;
            options.layoutRTL = RightToLeft.Yes == this.Control.RightToLeft;
            return options;
        }

        protected void DrawCheckBackground(PaintEventArgs e, Rectangle bounds, Color checkColor, Color checkBackground, bool disabledColors, ButtonBaseAdapter.ColorData colors)
        {
            if (this.Control.CheckState == CheckState.Indeterminate)
            {
                ButtonBaseAdapter.DrawDitheredFill(e.Graphics, colors.buttonFace, checkBackground, bounds);
            }
            else
            {
                DrawCheckBackground(this.Control.Enabled, this.Control.CheckState, e.Graphics, bounds, checkColor, checkBackground, disabledColors, colors);
            }
        }

        internal static void DrawCheckBackground(bool controlEnabled, CheckState controlCheckState, Graphics g, Rectangle bounds, Color checkColor, Color checkBackground, bool disabledColors, ButtonBaseAdapter.ColorData colors)
        {
            using (WindowsGraphics graphics = WindowsGraphics.FromGraphics(g))
            {
                WindowsBrush brush;
                if (!controlEnabled && disabledColors)
                {
                    brush = new WindowsSolidBrush(graphics.DeviceContext, SystemColors.Control);
                }
                else if (((controlCheckState == CheckState.Indeterminate) && (checkBackground == SystemColors.Window)) && disabledColors)
                {
                    Color color = SystemInformation.HighContrast ? SystemColors.ControlDark : SystemColors.Control;
                    byte red = (byte) ((color.R + SystemColors.Window.R) / 2);
                    byte green = (byte) ((color.G + SystemColors.Window.G) / 2);
                    byte blue = (byte) ((color.B + SystemColors.Window.B) / 2);
                    brush = new WindowsSolidBrush(graphics.DeviceContext, Color.FromArgb(red, green, blue));
                }
                else
                {
                    brush = new WindowsSolidBrush(graphics.DeviceContext, checkBackground);
                }
                try
                {
                    graphics.FillRectangle(brush, bounds);
                }
                finally
                {
                    if (brush != null)
                    {
                        brush.Dispose();
                    }
                }
            }
        }

        protected void DrawCheckBox(PaintEventArgs e, ButtonBaseAdapter.LayoutData layout)
        {
            Graphics g = e.Graphics;
            ButtonState state = this.GetState();
            if (this.Control.CheckState == CheckState.Indeterminate)
            {
                if (Application.RenderWithVisualStyles)
                {
                    CheckBoxRenderer.DrawCheckBox(g, new Point(layout.checkBounds.Left, layout.checkBounds.Top), CheckBoxRenderer.ConvertFromButtonState(state, true, this.Control.MouseIsOver));
                }
                else
                {
                    ControlPaint.DrawMixedCheckBox(g, layout.checkBounds, state);
                }
            }
            else if (Application.RenderWithVisualStyles)
            {
                CheckBoxRenderer.DrawCheckBox(g, new Point(layout.checkBounds.Left, layout.checkBounds.Top), CheckBoxRenderer.ConvertFromButtonState(state, false, this.Control.MouseIsOver));
            }
            else
            {
                ControlPaint.DrawCheckBox(g, layout.checkBounds, state);
            }
        }

        protected void DrawCheckFlat(PaintEventArgs e, ButtonBaseAdapter.LayoutData layout, Color checkColor, Color checkBackground, Color checkBorder, ButtonBaseAdapter.ColorData colors)
        {
            Rectangle checkBounds = layout.checkBounds;
            if (!layout.options.everettButtonCompat)
            {
                checkBounds.Width--;
                checkBounds.Height--;
            }
            using (WindowsGraphics graphics = WindowsGraphics.FromGraphics(e.Graphics))
            {
                using (WindowsPen pen = new WindowsPen(graphics.DeviceContext, checkBorder))
                {
                    graphics.DrawRectangle(pen, checkBounds);
                }
                if (layout.options.everettButtonCompat)
                {
                    checkBounds.Width--;
                    checkBounds.Height--;
                }
                checkBounds.Inflate(-1, -1);
            }
            if (this.Control.CheckState == CheckState.Indeterminate)
            {
                checkBounds.Width++;
                checkBounds.Height++;
                ButtonBaseAdapter.DrawDitheredFill(e.Graphics, colors.buttonFace, checkBackground, checkBounds);
            }
            else
            {
                using (WindowsGraphics graphics2 = WindowsGraphics.FromGraphics(e.Graphics))
                {
                    using (WindowsBrush brush = new WindowsSolidBrush(graphics2.DeviceContext, checkBackground))
                    {
                        checkBounds.Width++;
                        checkBounds.Height++;
                        graphics2.FillRectangle(brush, checkBounds);
                    }
                }
            }
            this.DrawCheckOnly(e, layout, colors, checkColor, checkBackground, true);
        }

        protected void DrawCheckOnly(PaintEventArgs e, ButtonBaseAdapter.LayoutData layout, ButtonBaseAdapter.ColorData colors, Color checkColor, Color checkBackground, bool disabledColors)
        {
            DrawCheckOnly(11, this.Control.Checked, this.Control.Enabled, this.Control.CheckState, e.Graphics, layout, colors, checkColor, checkBackground, disabledColors);
        }

        internal static void DrawCheckOnly(int checkSize, bool controlChecked, bool controlEnabled, CheckState controlCheckState, Graphics g, ButtonBaseAdapter.LayoutData layout, ButtonBaseAdapter.ColorData colors, Color checkColor, Color checkBackground, bool disabledColors)
        {
            if (controlChecked)
            {
                if (!controlEnabled && disabledColors)
                {
                    checkColor = colors.buttonShadow;
                }
                else if ((controlCheckState == CheckState.Indeterminate) && disabledColors)
                {
                    checkColor = SystemInformation.HighContrast ? colors.highlight : colors.buttonShadow;
                }
                Rectangle checkBounds = layout.checkBounds;
                if (checkBounds.Width == checkSize)
                {
                    checkBounds.Width++;
                    checkBounds.Height++;
                }
                checkBounds.Width++;
                checkBounds.Height++;
                Bitmap image = null;
                if (controlCheckState == CheckState.Checked)
                {
                    image = GetCheckBoxImage(checkColor, checkBounds, ref checkImageCheckedBackColor, ref checkImageChecked);
                }
                else
                {
                    image = GetCheckBoxImage(checkColor, checkBounds, ref checkImageIndeterminateBackColor, ref checkImageIndeterminate);
                }
                if (layout.options.everettButtonCompat)
                {
                    checkBounds.Y--;
                }
                else
                {
                    checkBounds.Y -= 2;
                }
                ControlPaint.DrawImageColorized(g, image, checkBounds, checkColor);
            }
        }

        internal static Rectangle DrawPopupBorder(Graphics g, Rectangle r, ButtonBaseAdapter.ColorData colors)
        {
            using (WindowsGraphics graphics = WindowsGraphics.FromGraphics(g))
            {
                using (WindowsPen pen = new WindowsPen(graphics.DeviceContext, colors.highlight))
                {
                    using (WindowsPen pen2 = new WindowsPen(graphics.DeviceContext, colors.buttonShadow))
                    {
                        using (WindowsPen pen3 = new WindowsPen(graphics.DeviceContext, colors.buttonFace))
                        {
                            graphics.DrawLine(pen, r.Right - 1, r.Top, r.Right - 1, r.Bottom);
                            graphics.DrawLine(pen, r.Left, r.Bottom - 1, r.Right, r.Bottom - 1);
                            graphics.DrawLine(pen2, r.Left, r.Top, r.Left, r.Bottom);
                            graphics.DrawLine(pen2, r.Left, r.Top, r.Right - 1, r.Top);
                            graphics.DrawLine(pen3, r.Right - 2, r.Top + 1, r.Right - 2, r.Bottom - 1);
                            graphics.DrawLine(pen3, r.Left + 1, r.Bottom - 2, r.Right - 1, r.Bottom - 2);
                        }
                    }
                }
            }
            r.Inflate(-1, -1);
            return r;
        }

        private static Bitmap GetCheckBoxImage(Color checkColor, Rectangle fullSize, ref Color cacheCheckColor, ref Bitmap cacheCheckImage)
        {
            if (((cacheCheckImage == null) || !cacheCheckColor.Equals(checkColor)) || ((cacheCheckImage.Width != fullSize.Width) || (cacheCheckImage.Height != fullSize.Height)))
            {
                if (cacheCheckImage != null)
                {
                    cacheCheckImage.Dispose();
                    cacheCheckImage = null;
                }
                System.Windows.Forms.NativeMethods.RECT rect = System.Windows.Forms.NativeMethods.RECT.FromXYWH(0, 0, fullSize.Width, fullSize.Height);
                Bitmap image = new Bitmap(fullSize.Width, fullSize.Height);
                Graphics wrapper = Graphics.FromImage(image);
                wrapper.Clear(Color.Transparent);
                IntPtr hdc = wrapper.GetHdc();
                try
                {
                    System.Windows.Forms.SafeNativeMethods.DrawFrameControl(new HandleRef(wrapper, hdc), ref rect, 2, 1);
                }
                finally
                {
                    wrapper.ReleaseHdcInternal(hdc);
                    wrapper.Dispose();
                }
                image.MakeTransparent();
                cacheCheckImage = image;
                cacheCheckColor = checkColor;
            }
            return cacheCheckImage;
        }

        protected ButtonState GetState()
        {
            ButtonState normal = ButtonState.Normal;
            if (this.Control.CheckState == CheckState.Unchecked)
            {
                normal = normal;
            }
            else
            {
                normal |= ButtonState.Checked;
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

        protected CheckBox Control
        {
            get
            {
                return (CheckBox) base.Control;
            }
        }
    }
}

