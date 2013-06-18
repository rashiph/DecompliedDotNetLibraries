namespace System.Windows.Forms.ButtonInternal
{
    using System;
    using System.Collections.Specialized;
    using System.Drawing;
    using System.Drawing.Text;
    using System.Windows.Forms;
    using System.Windows.Forms.Internal;
    using System.Windows.Forms.Layout;

    internal abstract class ButtonBaseAdapter
    {
        protected static int buttonBorderSize = 4;
        private ButtonBase control;

        internal ButtonBaseAdapter(ButtonBase control)
        {
            this.control = control;
        }

        internal virtual LayoutOptions CommonLayout()
        {
            LayoutOptions options = new LayoutOptions {
                client = LayoutUtils.DeflateRect(this.Control.ClientRectangle, this.Control.Padding),
                padding = this.Control.Padding,
                growBorderBy1PxWhenDefault = true,
                isDefault = this.Control.IsDefault,
                borderSize = 2,
                paddingSize = 0,
                maxFocus = true,
                focusOddEvenFixup = false,
                font = this.Control.Font,
                text = this.Control.Text,
                imageSize = (this.Control.Image == null) ? Size.Empty : this.Control.Image.Size,
                checkSize = 0,
                checkPaddingSize = 0,
                checkAlign = ContentAlignment.TopLeft,
                imageAlign = this.Control.ImageAlign,
                textAlign = this.Control.TextAlign,
                hintTextUp = false,
                shadowedText = !this.Control.Enabled,
                layoutRTL = RightToLeft.Yes == this.Control.RightToLeft,
                textImageRelation = this.Control.TextImageRelation,
                useCompatibleTextRendering = this.Control.UseCompatibleTextRendering
            };
            if (this.Control.FlatStyle != FlatStyle.System)
            {
                if (options.useCompatibleTextRendering)
                {
                    using (StringFormat format = this.Control.CreateStringFormat())
                    {
                        options.StringFormat = format;
                        return options;
                    }
                }
                options.gdiTextFormatFlags = this.Control.CreateTextFormatFlags();
            }
            return options;
        }

        internal static LayoutOptions CommonLayout(Rectangle clientRectangle, Padding padding, bool isDefault, Font font, string text, bool enabled, ContentAlignment textAlign, RightToLeft rtl)
        {
            return new LayoutOptions { 
                client = LayoutUtils.DeflateRect(clientRectangle, padding), padding = padding, growBorderBy1PxWhenDefault = true, isDefault = isDefault, borderSize = 2, paddingSize = 0, maxFocus = true, focusOddEvenFixup = false, font = font, text = text, imageSize = Size.Empty, checkSize = 0, checkPaddingSize = 0, checkAlign = ContentAlignment.TopLeft, imageAlign = ContentAlignment.MiddleCenter, textAlign = textAlign, 
                hintTextUp = false, shadowedText = !enabled, layoutRTL = RightToLeft.Yes == rtl, textImageRelation = TextImageRelation.Overlay, useCompatibleTextRendering = false
             };
        }

        private ColorOptions CommonRender(Graphics g)
        {
            return new ColorOptions(g, this.Control.ForeColor, this.Control.BackColor) { enabled = this.Control.Enabled };
        }

        private static ColorOptions CommonRender(Graphics g, Color foreColor, Color backColor, bool enabled)
        {
            return new ColorOptions(g, foreColor, backColor) { enabled = enabled };
        }

        internal static Brush CreateDitherBrush(Color color1, Color color2)
        {
            using (Bitmap bitmap = new Bitmap(2, 2))
            {
                bitmap.SetPixel(0, 0, color1);
                bitmap.SetPixel(0, 1, color2);
                bitmap.SetPixel(1, 1, color1);
                bitmap.SetPixel(1, 0, color2);
                return new TextureBrush(bitmap);
            }
        }

        internal virtual StringFormat CreateStringFormat()
        {
            return ControlPaint.CreateStringFormat(this.Control, this.Control.TextAlign, this.Control.ShowToolTip, this.Control.UseMnemonic);
        }

        internal virtual TextFormatFlags CreateTextFormatFlags()
        {
            return ControlPaint.CreateTextFormatFlags(this.Control, this.Control.TextAlign, this.Control.ShowToolTip, this.Control.UseMnemonic);
        }

        protected void Draw3DBorder(Graphics g, Rectangle bounds, ColorData colors, bool raised)
        {
            if ((this.Control.BackColor != SystemColors.Control) && SystemInformation.HighContrast)
            {
                if (raised)
                {
                    this.Draw3DBorderHighContrastRaised(g, ref bounds, colors);
                }
                else
                {
                    ControlPaint.DrawBorder(g, bounds, ControlPaint.Dark(this.Control.BackColor), ButtonBorderStyle.Solid);
                }
            }
            else if (raised)
            {
                this.Draw3DBorderRaised(g, ref bounds, colors);
            }
            else
            {
                this.Draw3DBorderNormal(g, ref bounds, colors);
            }
        }

        private void Draw3DBorderHighContrastRaised(Graphics g, ref Rectangle bounds, ColorData colors)
        {
            bool flag = colors.buttonFace.ToKnownColor() == SystemColors.Control.ToKnownColor();
            using (WindowsGraphics graphics = WindowsGraphics.FromGraphics(g))
            {
                Point point = new Point((bounds.X + bounds.Width) - 1, bounds.Y);
                Point point2 = new Point(bounds.X, bounds.Y);
                Point point3 = new Point(bounds.X, (bounds.Y + bounds.Height) - 1);
                Point point4 = new Point((bounds.X + bounds.Width) - 1, (bounds.Y + bounds.Height) - 1);
                WindowsPen pen = null;
                WindowsPen pen2 = null;
                WindowsPen pen3 = null;
                WindowsPen pen4 = null;
                try
                {
                    pen = flag ? new WindowsPen(graphics.DeviceContext, SystemColors.ControlLightLight) : new WindowsPen(graphics.DeviceContext, colors.highlight);
                    graphics.DrawLine(pen, point, point2);
                    graphics.DrawLine(pen, point2, point3);
                    pen2 = flag ? new WindowsPen(graphics.DeviceContext, SystemColors.ControlDarkDark) : new WindowsPen(graphics.DeviceContext, colors.buttonShadowDark);
                    point.Offset(0, -1);
                    graphics.DrawLine(pen2, point3, point4);
                    graphics.DrawLine(pen2, point4, point);
                    if (flag)
                    {
                        if (SystemInformation.HighContrast)
                        {
                            pen3 = new WindowsPen(graphics.DeviceContext, SystemColors.ControlLight);
                        }
                        else
                        {
                            pen3 = new WindowsPen(graphics.DeviceContext, SystemColors.Control);
                        }
                    }
                    else if (SystemInformation.HighContrast)
                    {
                        pen3 = new WindowsPen(graphics.DeviceContext, colors.highlight);
                    }
                    else
                    {
                        pen3 = new WindowsPen(graphics.DeviceContext, colors.buttonFace);
                    }
                    point.Offset(-1, 2);
                    point2.Offset(1, 1);
                    point3.Offset(1, -1);
                    point4.Offset(-1, -1);
                    graphics.DrawLine(pen3, point, point2);
                    graphics.DrawLine(pen3, point2, point3);
                    pen4 = flag ? new WindowsPen(graphics.DeviceContext, SystemColors.ControlDark) : new WindowsPen(graphics.DeviceContext, colors.buttonShadow);
                    point.Offset(0, -1);
                    graphics.DrawLine(pen4, point3, point4);
                    graphics.DrawLine(pen4, point4, point);
                }
                finally
                {
                    if (pen != null)
                    {
                        pen.Dispose();
                    }
                    if (pen2 != null)
                    {
                        pen2.Dispose();
                    }
                    if (pen3 != null)
                    {
                        pen3.Dispose();
                    }
                    if (pen4 != null)
                    {
                        pen4.Dispose();
                    }
                }
            }
        }

        private void Draw3DBorderNormal(Graphics g, ref Rectangle bounds, ColorData colors)
        {
            using (WindowsGraphics graphics = WindowsGraphics.FromGraphics(g))
            {
                WindowsPen pen;
                Point point = new Point((bounds.X + bounds.Width) - 1, bounds.Y);
                Point point2 = new Point(bounds.X, bounds.Y);
                Point point3 = new Point(bounds.X, (bounds.Y + bounds.Height) - 1);
                Point point4 = new Point((bounds.X + bounds.Width) - 1, (bounds.Y + bounds.Height) - 1);
                using (pen = new WindowsPen(graphics.DeviceContext, colors.buttonShadowDark))
                {
                    graphics.DrawLine(pen, point, point2);
                    graphics.DrawLine(pen, point2, point3);
                }
                using (pen = new WindowsPen(graphics.DeviceContext, colors.highlight))
                {
                    point.Offset(0, -1);
                    graphics.DrawLine(pen, point3, point4);
                    graphics.DrawLine(pen, point4, point);
                }
                pen = new WindowsPen(graphics.DeviceContext, colors.buttonFace);
                point.Offset(-1, 2);
                point2.Offset(1, 1);
                point3.Offset(1, -1);
                point4.Offset(-1, -1);
                try
                {
                    graphics.DrawLine(pen, point, point2);
                    graphics.DrawLine(pen, point2, point3);
                }
                finally
                {
                    pen.Dispose();
                }
                if (colors.buttonFace.ToKnownColor() == SystemColors.Control.ToKnownColor())
                {
                    pen = new WindowsPen(graphics.DeviceContext, SystemColors.ControlLight);
                }
                else
                {
                    pen = new WindowsPen(graphics.DeviceContext, colors.buttonFace);
                }
                try
                {
                    point.Offset(0, -1);
                    graphics.DrawLine(pen, point3, point4);
                    graphics.DrawLine(pen, point4, point);
                }
                finally
                {
                    pen.Dispose();
                }
            }
        }

        private void Draw3DBorderRaised(Graphics g, ref Rectangle bounds, ColorData colors)
        {
            bool flag = colors.buttonFace.ToKnownColor() == SystemColors.Control.ToKnownColor();
            using (WindowsGraphics graphics = WindowsGraphics.FromGraphics(g))
            {
                WindowsPen pen;
                Point point = new Point((bounds.X + bounds.Width) - 1, bounds.Y);
                Point point2 = new Point(bounds.X, bounds.Y);
                Point point3 = new Point(bounds.X, (bounds.Y + bounds.Height) - 1);
                Point point4 = new Point((bounds.X + bounds.Width) - 1, (bounds.Y + bounds.Height) - 1);
                using (pen = flag ? new WindowsPen(graphics.DeviceContext, SystemColors.ControlLightLight) : new WindowsPen(graphics.DeviceContext, colors.highlight))
                {
                    graphics.DrawLine(pen, point, point2);
                    graphics.DrawLine(pen, point2, point3);
                }
                if (flag)
                {
                    pen = new WindowsPen(graphics.DeviceContext, SystemColors.ControlDarkDark);
                }
                else
                {
                    pen = new WindowsPen(graphics.DeviceContext, colors.buttonShadowDark);
                }
                try
                {
                    point.Offset(0, -1);
                    graphics.DrawLine(pen, point3, point4);
                    graphics.DrawLine(pen, point4, point);
                }
                finally
                {
                    pen.Dispose();
                }
                point.Offset(-1, 2);
                point2.Offset(1, 1);
                point3.Offset(1, -1);
                point4.Offset(-1, -1);
                if (flag)
                {
                    if (SystemInformation.HighContrast)
                    {
                        pen = new WindowsPen(graphics.DeviceContext, SystemColors.ControlLight);
                    }
                    else
                    {
                        pen = new WindowsPen(graphics.DeviceContext, SystemColors.Control);
                    }
                }
                else
                {
                    pen = new WindowsPen(graphics.DeviceContext, colors.buttonFace);
                }
                try
                {
                    graphics.DrawLine(pen, point, point2);
                    graphics.DrawLine(pen, point2, point3);
                }
                finally
                {
                    pen.Dispose();
                }
                if (flag)
                {
                    pen = new WindowsPen(graphics.DeviceContext, SystemColors.ControlDark);
                }
                else
                {
                    pen = new WindowsPen(graphics.DeviceContext, colors.buttonShadow);
                }
                try
                {
                    point.Offset(0, -1);
                    graphics.DrawLine(pen, point3, point4);
                    graphics.DrawLine(pen, point4, point);
                }
                finally
                {
                    pen.Dispose();
                }
            }
        }

        protected internal static void Draw3DLiteBorder(Graphics g, Rectangle r, ColorData colors, bool up)
        {
            using (WindowsGraphics graphics = WindowsGraphics.FromGraphics(g))
            {
                WindowsPen pen;
                Point point = new Point(r.Right - 1, r.Top);
                Point point2 = new Point(r.Left, r.Top);
                Point point3 = new Point(r.Left, r.Bottom - 1);
                Point point4 = new Point(r.Right - 1, r.Bottom - 1);
                using (pen = up ? new WindowsPen(graphics.DeviceContext, colors.highlight) : new WindowsPen(graphics.DeviceContext, colors.buttonShadow))
                {
                    graphics.DrawLine(pen, point, point2);
                    graphics.DrawLine(pen, point2, point3);
                }
                using (pen = up ? new WindowsPen(graphics.DeviceContext, colors.buttonShadow) : new WindowsPen(graphics.DeviceContext, colors.highlight))
                {
                    point.Offset(0, -1);
                    graphics.DrawLine(pen, point3, point4);
                    graphics.DrawLine(pen, point4, point);
                }
            }
        }

        internal static void DrawDefaultBorder(Graphics g, Rectangle r, Color c, bool isDefault)
        {
            if (isDefault)
            {
                Pen pen;
                r.Inflate(1, 1);
                if (c.IsSystemColor)
                {
                    pen = SystemPens.FromSystemColor(c);
                }
                else
                {
                    pen = new Pen(c);
                }
                g.DrawRectangle(pen, r.X, r.Y, r.Width - 1, r.Height - 1);
                if (!c.IsSystemColor)
                {
                    pen.Dispose();
                }
            }
        }

        internal static void DrawDitheredFill(Graphics g, Color color1, Color color2, Rectangle bounds)
        {
            using (Brush brush = CreateDitherBrush(color1, color2))
            {
                g.FillRectangle(brush, bounds);
            }
        }

        internal static void DrawFlatBorder(Graphics g, Rectangle r, Color c)
        {
            ControlPaint.DrawBorder(g, r, c, ButtonBorderStyle.Solid);
        }

        internal static void DrawFlatBorderWithSize(Graphics g, Rectangle r, Color c, int size)
        {
            bool isSystemColor = c.IsSystemColor;
            SolidBrush brush = null;
            if (size > 1)
            {
                brush = new SolidBrush(c);
            }
            else if (isSystemColor)
            {
                brush = (SolidBrush) SystemBrushes.FromSystemColor(c);
            }
            else
            {
                brush = new SolidBrush(c);
            }
            try
            {
                size = Math.Min(size, Math.Min(r.Width, r.Height));
                g.FillRectangle(brush, r.X, r.Y, size, r.Height);
                g.FillRectangle(brush, (r.X + r.Width) - size, r.Y, size, r.Height);
                g.FillRectangle(brush, r.X + size, r.Y, r.Width - (size * 2), size);
                g.FillRectangle(brush, r.X + size, (r.Y + r.Height) - size, r.Width - (size * 2), size);
            }
            finally
            {
                if (!isSystemColor && (brush != null))
                {
                    brush.Dispose();
                }
            }
        }

        internal static void DrawFlatFocus(Graphics g, Rectangle r, Color c)
        {
            using (WindowsGraphics graphics = WindowsGraphics.FromGraphics(g))
            {
                using (WindowsPen pen = new WindowsPen(graphics.DeviceContext, c))
                {
                    graphics.DrawRectangle(pen, r);
                }
            }
        }

        private void DrawFocus(Graphics g, Rectangle r)
        {
            if (this.Control.Focused && this.Control.ShowFocusCues)
            {
                ControlPaint.DrawFocusRectangle(g, r, this.Control.ForeColor, this.Control.BackColor);
            }
        }

        private void DrawImage(Graphics graphics, LayoutData layout)
        {
            if (this.Control.Image != null)
            {
                this.DrawImageCore(graphics, this.Control.Image, layout.imageBounds, layout.imageStart, layout);
            }
        }

        internal virtual void DrawImageCore(Graphics graphics, Image image, Rectangle imageBounds, Point imageStart, LayoutData layout)
        {
            Region clip = graphics.Clip;
            if (!layout.options.everettButtonCompat)
            {
                Rectangle rect = new Rectangle(buttonBorderSize, buttonBorderSize, this.Control.Width - (2 * buttonBorderSize), this.Control.Height - (2 * buttonBorderSize));
                Region region2 = clip.Clone();
                region2.Intersect(rect);
                region2.Intersect(imageBounds);
                graphics.Clip = region2;
            }
            else
            {
                imageBounds.Width++;
                imageBounds.Height++;
                imageBounds.X = imageStart.X + 1;
                imageBounds.Y = imageStart.Y + 1;
            }
            try
            {
                if (!this.Control.Enabled)
                {
                    ControlPaint.DrawImageDisabled(graphics, image, imageBounds, this.Control.BackColor, true);
                }
                else
                {
                    graphics.DrawImage(image, imageBounds.X, imageBounds.Y, image.Width, image.Height);
                }
            }
            finally
            {
                if (!layout.options.everettButtonCompat)
                {
                    graphics.Clip = clip;
                }
            }
        }

        private void DrawText(Graphics g, LayoutData layout, Color c, ColorData colors)
        {
            Rectangle textBounds = layout.textBounds;
            bool shadowedText = layout.options.shadowedText;
            if (this.Control.UseCompatibleTextRendering)
            {
                using (StringFormat format = this.CreateStringFormat())
                {
                    Brush brush2;
                    if ((this.Control.TextAlign & (ContentAlignment.BottomCenter | ContentAlignment.MiddleCenter | ContentAlignment.TopCenter)) == ((ContentAlignment) 0))
                    {
                        textBounds.X--;
                    }
                    textBounds.Width++;
                    if (shadowedText && !this.Control.Enabled)
                    {
                        textBounds.Offset(1, 1);
                        using (SolidBrush brush = new SolidBrush(colors.highlight))
                        {
                            g.DrawString(this.Control.Text, this.Control.Font, brush, textBounds, format);
                            textBounds.Offset(-1, -1);
                            brush.Color = colors.buttonShadow;
                            g.DrawString(this.Control.Text, this.Control.Font, brush, textBounds, format);
                            return;
                        }
                    }
                    if (c.IsSystemColor)
                    {
                        brush2 = SystemBrushes.FromSystemColor(c);
                    }
                    else
                    {
                        brush2 = new SolidBrush(c);
                    }
                    g.DrawString(this.Control.Text, this.Control.Font, brush2, textBounds, format);
                    if (!c.IsSystemColor)
                    {
                        brush2.Dispose();
                    }
                    return;
                }
            }
            TextFormatFlags flags = this.CreateTextFormatFlags();
            if (shadowedText && !this.Control.Enabled)
            {
                if (Application.RenderWithVisualStyles)
                {
                    TextRenderer.DrawText(g, this.Control.Text, this.Control.Font, textBounds, colors.buttonShadow, flags);
                }
                else
                {
                    textBounds.Offset(1, 1);
                    TextRenderer.DrawText(g, this.Control.Text, this.Control.Font, textBounds, colors.highlight, flags);
                    textBounds.Offset(-1, -1);
                    TextRenderer.DrawText(g, this.Control.Text, this.Control.Font, textBounds, colors.buttonShadow, flags);
                }
            }
            else
            {
                TextRenderer.DrawText(g, this.Control.Text, this.Control.Font, textBounds, c, flags);
            }
        }

        internal virtual Size GetPreferredSizeCore(Size proposedSize)
        {
            Size preferredSizeCore;
            using (Graphics graphics = WindowsFormsUtils.CreateMeasurementGraphics())
            {
                using (PaintEventArgs args = new PaintEventArgs(graphics, new Rectangle()))
                {
                    preferredSizeCore = this.Layout(args).GetPreferredSizeCore(proposedSize);
                }
            }
            return preferredSizeCore;
        }

        protected abstract LayoutOptions Layout(PaintEventArgs e);
        internal static Color MixedColor(Color color1, Color color2)
        {
            byte a = color1.A;
            byte r = color1.R;
            byte g = color1.G;
            byte b = color1.B;
            byte num5 = color2.A;
            byte num6 = color2.R;
            byte num7 = color2.G;
            byte num8 = color2.B;
            int alpha = (a + num5) / 2;
            int red = (r + num6) / 2;
            int green = (g + num7) / 2;
            int blue = (b + num8) / 2;
            return Color.FromArgb(alpha, red, green, blue);
        }

        internal void Paint(PaintEventArgs pevent)
        {
            if (this.Control.MouseIsDown)
            {
                this.PaintDown(pevent, CheckState.Unchecked);
            }
            else if (this.Control.MouseIsOver)
            {
                this.PaintOver(pevent, CheckState.Unchecked);
            }
            else
            {
                this.PaintUp(pevent, CheckState.Unchecked);
            }
        }

        internal static void PaintButtonBackground(WindowsGraphics wg, Rectangle bounds, WindowsBrush background)
        {
            wg.FillRectangle(background, bounds);
        }

        internal void PaintButtonBackground(PaintEventArgs e, Rectangle bounds, Brush background)
        {
            if (background == null)
            {
                this.Control.PaintBackground(e, bounds);
            }
            else
            {
                e.Graphics.FillRectangle(background, bounds);
            }
        }

        internal abstract void PaintDown(PaintEventArgs e, CheckState state);
        internal void PaintField(PaintEventArgs e, LayoutData layout, ColorData colors, Color foreColor, bool drawFocus)
        {
            Graphics g = e.Graphics;
            Rectangle focus = layout.focus;
            this.DrawText(g, layout, foreColor, colors);
            if (drawFocus)
            {
                this.DrawFocus(g, focus);
            }
        }

        protected ColorOptions PaintFlatRender(Graphics g)
        {
            ColorOptions options = this.CommonRender(g);
            options.disabledTextDim = true;
            return options;
        }

        internal static ColorOptions PaintFlatRender(Graphics g, Color foreColor, Color backColor, bool enabled)
        {
            ColorOptions options = CommonRender(g, foreColor, backColor, enabled);
            options.disabledTextDim = true;
            return options;
        }

        internal void PaintImage(PaintEventArgs e, LayoutData layout)
        {
            Graphics graphics = e.Graphics;
            this.DrawImage(graphics, layout);
        }

        internal abstract void PaintOver(PaintEventArgs e, CheckState state);
        protected ColorOptions PaintPopupRender(Graphics g)
        {
            ColorOptions options = this.CommonRender(g);
            options.disabledTextDim = true;
            return options;
        }

        internal static ColorOptions PaintPopupRender(Graphics g, Color foreColor, Color backColor, bool enabled)
        {
            ColorOptions options = CommonRender(g, foreColor, backColor, enabled);
            options.disabledTextDim = true;
            return options;
        }

        protected ColorOptions PaintRender(Graphics g)
        {
            return this.CommonRender(g);
        }

        internal abstract void PaintUp(PaintEventArgs e, CheckState state);

        protected ButtonBase Control
        {
            get
            {
                return this.control;
            }
        }

        internal class ColorData
        {
            internal Color buttonFace;
            internal Color buttonShadow;
            internal Color buttonShadowDark;
            internal Color constrastButtonShadow;
            internal Color highlight;
            internal Color lowButtonFace;
            internal Color lowHighlight;
            internal ButtonBaseAdapter.ColorOptions options;
            internal Color windowFrame;
            internal Color windowText;

            internal ColorData(ButtonBaseAdapter.ColorOptions options)
            {
                this.options = options;
            }
        }

        internal class ColorOptions
        {
            internal Color backColor;
            internal bool disabledTextDim;
            internal bool enabled;
            internal Color foreColor;
            internal Graphics graphics;
            internal bool highContrast;

            internal ColorOptions(Graphics graphics, Color foreColor, Color backColor)
            {
                this.graphics = graphics;
                this.backColor = backColor;
                this.foreColor = foreColor;
                this.highContrast = SystemInformation.HighContrast;
            }

            internal static int Adjust255(float percentage, int value)
            {
                int num = (int) (percentage * value);
                if (num > 0xff)
                {
                    return 0xff;
                }
                return num;
            }

            internal ButtonBaseAdapter.ColorData Calculate()
            {
                ButtonBaseAdapter.ColorData data = new ButtonBaseAdapter.ColorData(this) {
                    buttonFace = this.backColor
                };
                if (this.backColor == SystemColors.Control)
                {
                    data.buttonShadow = SystemColors.ControlDark;
                    data.buttonShadowDark = SystemColors.ControlDarkDark;
                    data.highlight = SystemColors.ControlLightLight;
                }
                else if (!this.highContrast)
                {
                    data.buttonShadow = ControlPaint.Dark(this.backColor);
                    data.buttonShadowDark = ControlPaint.DarkDark(this.backColor);
                    data.highlight = ControlPaint.LightLight(this.backColor);
                }
                else
                {
                    data.buttonShadow = ControlPaint.Dark(this.backColor);
                    data.buttonShadowDark = ControlPaint.LightLight(this.backColor);
                    data.highlight = ControlPaint.LightLight(this.backColor);
                }
                float percentage = 0.9f;
                if (data.buttonFace.GetBrightness() < 0.5)
                {
                    percentage = 1.2f;
                }
                data.lowButtonFace = Color.FromArgb(Adjust255(percentage, data.buttonFace.R), Adjust255(percentage, data.buttonFace.G), Adjust255(percentage, data.buttonFace.B));
                percentage = 0.9f;
                if (data.highlight.GetBrightness() < 0.5)
                {
                    percentage = 1.2f;
                }
                data.lowHighlight = Color.FromArgb(Adjust255(percentage, data.highlight.R), Adjust255(percentage, data.highlight.G), Adjust255(percentage, data.highlight.B));
                if (this.highContrast && (this.backColor != SystemColors.Control))
                {
                    data.highlight = data.lowHighlight;
                }
                data.windowFrame = this.foreColor;
                if (data.buttonFace.GetBrightness() < 0.5)
                {
                    data.constrastButtonShadow = data.lowHighlight;
                }
                else
                {
                    data.constrastButtonShadow = data.buttonShadow;
                }
                if (!this.enabled && this.disabledTextDim)
                {
                    data.windowText = data.buttonShadow;
                }
                else
                {
                    data.windowText = data.windowFrame;
                }
                IntPtr hdc = this.graphics.GetHdc();
                try
                {
                    using (WindowsGraphics graphics = WindowsGraphics.FromHdc(hdc))
                    {
                        data.buttonFace = graphics.GetNearestColor(data.buttonFace);
                        data.buttonShadow = graphics.GetNearestColor(data.buttonShadow);
                        data.buttonShadowDark = graphics.GetNearestColor(data.buttonShadowDark);
                        data.constrastButtonShadow = graphics.GetNearestColor(data.constrastButtonShadow);
                        data.windowText = graphics.GetNearestColor(data.windowText);
                        data.highlight = graphics.GetNearestColor(data.highlight);
                        data.lowHighlight = graphics.GetNearestColor(data.lowHighlight);
                        data.lowButtonFace = graphics.GetNearestColor(data.lowButtonFace);
                        data.windowFrame = graphics.GetNearestColor(data.windowFrame);
                    }
                }
                finally
                {
                    this.graphics.ReleaseHdc();
                }
                return data;
            }
        }

        internal class LayoutData
        {
            internal Rectangle checkArea;
            internal Rectangle checkBounds;
            internal Rectangle client;
            internal Rectangle face;
            internal Rectangle field;
            internal Rectangle focus;
            internal Rectangle imageBounds;
            internal Point imageStart;
            internal ButtonBaseAdapter.LayoutOptions options;
            internal Rectangle textBounds;

            internal LayoutData(ButtonBaseAdapter.LayoutOptions options)
            {
                this.options = options;
            }
        }

        internal class LayoutOptions
        {
            private static readonly TextImageRelation[] _imageAlignToRelation;
            internal int borderSize;
            internal ContentAlignment checkAlign;
            internal int checkPaddingSize;
            internal int checkSize;
            internal Rectangle client;
            private static readonly int combineCheck = BitVector32.CreateMask();
            private static readonly int combineImageText = BitVector32.CreateMask(combineCheck);
            private bool disableWordWrapping;
            internal bool everettButtonCompat = true;
            internal bool focusOddEvenFixup;
            internal Font font;
            internal StringAlignment gdipAlignment;
            internal StringFormatFlags gdipFormatFlags;
            internal HotkeyPrefix gdipHotkeyPrefix;
            internal StringAlignment gdipLineAlignment;
            internal StringTrimming gdipTrimming;
            internal System.Windows.Forms.TextFormatFlags gdiTextFormatFlags = (System.Windows.Forms.TextFormatFlags.TextBoxControl | System.Windows.Forms.TextFormatFlags.WordBreak);
            internal bool growBorderBy1PxWhenDefault;
            internal bool hintTextUp;
            internal ContentAlignment imageAlign;
            internal Size imageSize;
            internal bool isDefault;
            internal bool layoutRTL;
            internal bool maxFocus;
            internal Padding padding;
            internal int paddingSize;
            internal bool shadowedText;
            internal string text;
            internal ContentAlignment textAlign;
            internal int textImageInset = 2;
            internal TextImageRelation textImageRelation;
            internal bool textOffset;
            internal bool useCompatibleTextRendering;
            internal bool verticalText;

            static LayoutOptions()
            {
                TextImageRelation[] relationArray = new TextImageRelation[11];
                relationArray[0] = TextImageRelation.ImageBeforeText | TextImageRelation.ImageAboveText;
                relationArray[1] = TextImageRelation.ImageAboveText;
                relationArray[2] = TextImageRelation.TextBeforeImage | TextImageRelation.ImageAboveText;
                relationArray[4] = TextImageRelation.ImageBeforeText;
                relationArray[6] = TextImageRelation.TextBeforeImage;
                relationArray[8] = TextImageRelation.ImageBeforeText | TextImageRelation.TextAboveImage;
                relationArray[9] = TextImageRelation.TextAboveImage;
                relationArray[10] = TextImageRelation.TextBeforeImage | TextImageRelation.TextAboveImage;
                _imageAlignToRelation = relationArray;
            }

            private void CalcCheckmarkRectangle(ButtonBaseAdapter.LayoutData layout)
            {
                int fullCheckSize = this.FullCheckSize;
                layout.checkBounds = new Rectangle(this.client.X, this.client.Y, fullCheckSize, fullCheckSize);
                ContentAlignment alignment = this.RtlTranslateContent(this.checkAlign);
                Rectangle rectangle = Rectangle.Inflate(layout.face, -this.paddingSize, -this.paddingSize);
                layout.field = rectangle;
                if (fullCheckSize > 0)
                {
                    if ((alignment & (ContentAlignment.BottomRight | ContentAlignment.MiddleRight | ContentAlignment.TopRight)) != ((ContentAlignment) 0))
                    {
                        layout.checkBounds.X = (rectangle.X + rectangle.Width) - layout.checkBounds.Width;
                    }
                    else if ((alignment & (ContentAlignment.BottomCenter | ContentAlignment.MiddleCenter | ContentAlignment.TopCenter)) != ((ContentAlignment) 0))
                    {
                        layout.checkBounds.X = rectangle.X + ((rectangle.Width - layout.checkBounds.Width) / 2);
                    }
                    if ((alignment & (ContentAlignment.BottomRight | ContentAlignment.BottomCenter | ContentAlignment.BottomLeft)) != ((ContentAlignment) 0))
                    {
                        layout.checkBounds.Y = (rectangle.Y + rectangle.Height) - layout.checkBounds.Height;
                    }
                    else if ((alignment & (ContentAlignment.TopRight | ContentAlignment.TopCenter | ContentAlignment.TopLeft)) != ((ContentAlignment) 0))
                    {
                        layout.checkBounds.Y = rectangle.Y + 2;
                    }
                    else
                    {
                        layout.checkBounds.Y = rectangle.Y + ((rectangle.Height - layout.checkBounds.Height) / 2);
                    }
                    switch (alignment)
                    {
                        case ContentAlignment.TopLeft:
                        case ContentAlignment.MiddleLeft:
                        case ContentAlignment.BottomLeft:
                            layout.checkArea.X = rectangle.X;
                            layout.checkArea.Width = fullCheckSize + 1;
                            layout.checkArea.Y = rectangle.Y;
                            layout.checkArea.Height = rectangle.Height;
                            layout.field.X += fullCheckSize + 1;
                            layout.field.Width -= fullCheckSize + 1;
                            break;

                        case ContentAlignment.TopCenter:
                            layout.checkArea.X = rectangle.X;
                            layout.checkArea.Width = rectangle.Width;
                            layout.checkArea.Y = rectangle.Y;
                            layout.checkArea.Height = fullCheckSize;
                            layout.field.Y += fullCheckSize;
                            layout.field.Height -= fullCheckSize;
                            break;

                        case ContentAlignment.TopRight:
                        case ContentAlignment.BottomRight:
                        case ContentAlignment.MiddleRight:
                            layout.checkArea.X = (rectangle.X + rectangle.Width) - fullCheckSize;
                            layout.checkArea.Width = fullCheckSize + 1;
                            layout.checkArea.Y = rectangle.Y;
                            layout.checkArea.Height = rectangle.Height;
                            layout.field.Width -= fullCheckSize + 1;
                            break;

                        case ContentAlignment.MiddleCenter:
                            layout.checkArea = layout.checkBounds;
                            break;

                        case ContentAlignment.BottomCenter:
                            layout.checkArea.X = rectangle.X;
                            layout.checkArea.Width = rectangle.Width;
                            layout.checkArea.Y = (rectangle.Y + rectangle.Height) - fullCheckSize;
                            layout.checkArea.Height = fullCheckSize;
                            layout.field.Height -= fullCheckSize;
                            break;
                    }
                    layout.checkBounds.Width -= this.checkPaddingSize;
                    layout.checkBounds.Height -= this.checkPaddingSize;
                }
            }

            private Size Compose(Size checkSize, Size imageSize, Size textSize)
            {
                Composition horizontalComposition = this.GetHorizontalComposition();
                Composition verticalComposition = this.GetVerticalComposition();
                return new Size(this.xCompose(horizontalComposition, checkSize.Width, imageSize.Width, textSize.Width), this.xCompose(verticalComposition, checkSize.Height, imageSize.Height, textSize.Height));
            }

            private Size Decompose(Size checkSize, Size imageSize, Size proposedSize)
            {
                Composition horizontalComposition = this.GetHorizontalComposition();
                Composition verticalComposition = this.GetVerticalComposition();
                return new Size(this.xDecompose(horizontalComposition, checkSize.Width, imageSize.Width, proposedSize.Width), this.xDecompose(verticalComposition, checkSize.Height, imageSize.Height, proposedSize.Height));
            }

            private Composition GetHorizontalComposition()
            {
                BitVector32 vector = new BitVector32();
                vector[combineCheck] = (this.checkAlign == ContentAlignment.MiddleCenter) || !LayoutUtils.IsHorizontalAlignment(this.checkAlign);
                vector[combineImageText] = !LayoutUtils.IsHorizontalRelation(this.textImageRelation);
                return (Composition) vector.Data;
            }

            internal Size GetPreferredSizeCore(Size proposedSize)
            {
                int width = (this.borderSize * 2) + (this.paddingSize * 2);
                if (this.growBorderBy1PxWhenDefault)
                {
                    width += 2;
                }
                Size size = new Size(width, width);
                proposedSize -= size;
                int fullCheckSize = this.FullCheckSize;
                Size checkSize = (fullCheckSize > 0) ? new Size(fullCheckSize + 1, fullCheckSize) : Size.Empty;
                Size size3 = new Size(this.textImageInset * 2, this.textImageInset * 2);
                Size imageSize = (this.imageSize != Size.Empty) ? (this.imageSize + size3) : Size.Empty;
                proposedSize -= size3;
                proposedSize = this.Decompose(checkSize, imageSize, proposedSize);
                Size empty = Size.Empty;
                if (!string.IsNullOrEmpty(this.text))
                {
                    try
                    {
                        this.disableWordWrapping = true;
                        empty = this.GetTextSize(proposedSize) + size3;
                    }
                    finally
                    {
                        this.disableWordWrapping = false;
                    }
                }
                return (this.Compose(checkSize, this.imageSize, empty) + size);
            }

            protected virtual Size GetTextSize(Size proposedSize)
            {
                proposedSize = LayoutUtils.FlipSizeIf(this.verticalText, proposedSize);
                Size empty = Size.Empty;
                if (this.useCompatibleTextRendering)
                {
                    using (Graphics graphics = WindowsFormsUtils.CreateMeasurementGraphics())
                    {
                        using (System.Drawing.StringFormat format = this.StringFormat)
                        {
                            empty = Size.Ceiling(graphics.MeasureString(this.text, this.font, new SizeF((float) proposedSize.Width, (float) proposedSize.Height), format));
                        }
                        goto Label_0095;
                    }
                }
                if (!string.IsNullOrEmpty(this.text))
                {
                    empty = TextRenderer.MeasureText(this.text, this.font, proposedSize, this.TextFormatFlags);
                }
            Label_0095:
                return LayoutUtils.FlipSizeIf(this.verticalText, empty);
            }

            private Composition GetVerticalComposition()
            {
                BitVector32 vector = new BitVector32();
                vector[combineCheck] = (this.checkAlign == ContentAlignment.MiddleCenter) || !LayoutUtils.IsVerticalAlignment(this.checkAlign);
                vector[combineImageText] = !LayoutUtils.IsVerticalRelation(this.textImageRelation);
                return (Composition) vector.Data;
            }

            private static TextImageRelation ImageAlignToRelation(ContentAlignment alignment)
            {
                return _imageAlignToRelation[LayoutUtils.ContentAlignmentToIndex(alignment)];
            }

            internal ButtonBaseAdapter.LayoutData Layout()
            {
                ButtonBaseAdapter.LayoutData layout = new ButtonBaseAdapter.LayoutData(this) {
                    client = this.client
                };
                int fullBorderSize = this.FullBorderSize;
                layout.face = Rectangle.Inflate(layout.client, -fullBorderSize, -fullBorderSize);
                this.CalcCheckmarkRectangle(layout);
                this.LayoutTextAndImage(layout);
                if (this.maxFocus)
                {
                    layout.focus = layout.field;
                    layout.focus.Inflate(-1, -1);
                    layout.focus = LayoutUtils.InflateRect(layout.focus, this.padding);
                }
                else
                {
                    Rectangle a = new Rectangle(layout.textBounds.X - 1, layout.textBounds.Y - 1, layout.textBounds.Width + 2, layout.textBounds.Height + 3);
                    if (this.imageSize != Size.Empty)
                    {
                        layout.focus = Rectangle.Union(a, layout.imageBounds);
                    }
                    else
                    {
                        layout.focus = a;
                    }
                }
                if (this.focusOddEvenFixup)
                {
                    if ((layout.focus.Height % 2) == 0)
                    {
                        layout.focus.Y++;
                        layout.focus.Height--;
                    }
                    if ((layout.focus.Width % 2) == 0)
                    {
                        layout.focus.X++;
                        layout.focus.Width--;
                    }
                }
                return layout;
            }

            internal void LayoutTextAndImage(ButtonBaseAdapter.LayoutData layout)
            {
                int num3;
                ContentAlignment align = this.RtlTranslateContent(this.imageAlign);
                ContentAlignment alignment2 = this.RtlTranslateContent(this.textAlign);
                TextImageRelation relation = this.RtlTranslateRelation(this.textImageRelation);
                Rectangle withinThis = Rectangle.Inflate(layout.field, -this.textImageInset, -this.textImageInset);
                if (this.OnePixExtraBorder)
                {
                    withinThis.Inflate(1, 1);
                }
                if (((this.imageSize == Size.Empty) || (this.text == null)) || ((this.text.Length == 0) || (relation == TextImageRelation.Overlay)))
                {
                    Size textSize = this.GetTextSize(withinThis.Size);
                    Size imageSize = this.imageSize;
                    if (layout.options.everettButtonCompat && (this.imageSize != Size.Empty))
                    {
                        imageSize = new Size(imageSize.Width + 1, imageSize.Height + 1);
                    }
                    layout.imageBounds = LayoutUtils.Align(imageSize, withinThis, align);
                    layout.textBounds = LayoutUtils.Align(textSize, withinThis, alignment2);
                }
                else
                {
                    Size proposedSize = LayoutUtils.SubAlignedRegion(withinThis.Size, this.imageSize, relation);
                    Size size4 = this.GetTextSize(proposedSize);
                    Rectangle rectangle2 = withinThis;
                    Size b = LayoutUtils.AddAlignedRegion(size4, this.imageSize, relation);
                    rectangle2.Size = LayoutUtils.UnionSizes(rectangle2.Size, b);
                    Rectangle bounds = LayoutUtils.Align(b, rectangle2, ContentAlignment.MiddleCenter);
                    bool flag = (ImageAlignToRelation(align) & relation) != TextImageRelation.Overlay;
                    bool flag2 = (TextAlignToRelation(alignment2) & relation) != TextImageRelation.Overlay;
                    if (flag)
                    {
                        LayoutUtils.SplitRegion(rectangle2, this.imageSize, (AnchorStyles) relation, out layout.imageBounds, out layout.textBounds);
                    }
                    else if (flag2)
                    {
                        LayoutUtils.SplitRegion(rectangle2, size4, (AnchorStyles) LayoutUtils.GetOppositeTextImageRelation(relation), out layout.textBounds, out layout.imageBounds);
                    }
                    else
                    {
                        LayoutUtils.SplitRegion(bounds, this.imageSize, (AnchorStyles) relation, out layout.imageBounds, out layout.textBounds);
                        LayoutUtils.ExpandRegionsToFillBounds(rectangle2, (AnchorStyles) relation, ref layout.imageBounds, ref layout.textBounds);
                    }
                    layout.imageBounds = LayoutUtils.Align(this.imageSize, layout.imageBounds, align);
                    layout.textBounds = LayoutUtils.Align(size4, layout.textBounds, alignment2);
                }
                switch (relation)
                {
                    case TextImageRelation.TextBeforeImage:
                    case TextImageRelation.ImageBeforeText:
                    {
                        int num = Math.Min(layout.textBounds.Bottom, layout.field.Bottom);
                        int introduced18 = Math.Min(layout.textBounds.Y, layout.field.Y + ((layout.field.Height - layout.textBounds.Height) / 2));
                        layout.textBounds.Y = Math.Max(introduced18, layout.field.Y);
                        layout.textBounds.Height = num - layout.textBounds.Y;
                        break;
                    }
                }
                if ((relation == TextImageRelation.TextAboveImage) || (relation == TextImageRelation.ImageAboveText))
                {
                    int num2 = Math.Min(layout.textBounds.Right, layout.field.Right);
                    int introduced19 = Math.Min(layout.textBounds.X, layout.field.X + ((layout.field.Width - layout.textBounds.Width) / 2));
                    layout.textBounds.X = Math.Max(introduced19, layout.field.X);
                    layout.textBounds.Width = num2 - layout.textBounds.X;
                }
                if ((relation == TextImageRelation.ImageBeforeText) && (layout.imageBounds.Size.Width != 0))
                {
                    layout.imageBounds.Width = Math.Max(0, Math.Min(withinThis.Width - layout.textBounds.Width, layout.imageBounds.Width));
                    layout.textBounds.X = layout.imageBounds.X + layout.imageBounds.Width;
                }
                if ((relation == TextImageRelation.ImageAboveText) && (layout.imageBounds.Size.Height != 0))
                {
                    layout.imageBounds.Height = Math.Max(0, Math.Min(withinThis.Height - layout.textBounds.Height, layout.imageBounds.Height));
                    layout.textBounds.Y = layout.imageBounds.Y + layout.imageBounds.Height;
                }
                layout.textBounds = Rectangle.Intersect(layout.textBounds, layout.field);
                if (this.hintTextUp)
                {
                    layout.textBounds.Y--;
                }
                if (this.textOffset)
                {
                    layout.textBounds.Offset(1, 1);
                }
                if (layout.options.everettButtonCompat)
                {
                    layout.imageStart = layout.imageBounds.Location;
                    layout.imageBounds = Rectangle.Intersect(layout.imageBounds, layout.field);
                }
                else if (!Application.RenderWithVisualStyles)
                {
                    layout.textBounds.X++;
                }
                if (!this.useCompatibleTextRendering)
                {
                    num3 = Math.Min(layout.textBounds.Bottom, withinThis.Bottom);
                    layout.textBounds.Y = Math.Max(layout.textBounds.Y, withinThis.Y);
                }
                else
                {
                    num3 = Math.Min(layout.textBounds.Bottom, layout.field.Bottom);
                    layout.textBounds.Y = Math.Max(layout.textBounds.Y, layout.field.Y);
                }
                layout.textBounds.Height = num3 - layout.textBounds.Y;
            }

            internal ContentAlignment RtlTranslateContent(ContentAlignment align)
            {
                if (this.layoutRTL)
                {
                    ContentAlignment[][] alignmentArray = new ContentAlignment[][] { new ContentAlignment[] { ContentAlignment.TopLeft, ContentAlignment.TopRight }, new ContentAlignment[] { ContentAlignment.MiddleLeft, ContentAlignment.MiddleRight }, new ContentAlignment[] { ContentAlignment.BottomLeft, ContentAlignment.BottomRight } };
                    for (int i = 0; i < 3; i++)
                    {
                        if (alignmentArray[i][0] == align)
                        {
                            return alignmentArray[i][1];
                        }
                        if (alignmentArray[i][1] == align)
                        {
                            return alignmentArray[i][0];
                        }
                    }
                }
                return align;
            }

            private TextImageRelation RtlTranslateRelation(TextImageRelation relation)
            {
                if (!this.layoutRTL)
                {
                    return relation;
                }
                TextImageRelation relation2 = relation;
                if (relation2 != TextImageRelation.ImageBeforeText)
                {
                    if (relation2 == TextImageRelation.TextBeforeImage)
                    {
                        return TextImageRelation.ImageBeforeText;
                    }
                    return relation;
                }
                return TextImageRelation.TextBeforeImage;
            }

            private static TextImageRelation TextAlignToRelation(ContentAlignment alignment)
            {
                return LayoutUtils.GetOppositeTextImageRelation(ImageAlignToRelation(alignment));
            }

            private int xCompose(Composition composition, int checkSize, int imageSize, int textSize)
            {
                switch (composition)
                {
                    case Composition.NoneCombined:
                        return ((checkSize + imageSize) + textSize);

                    case Composition.CheckCombined:
                        return Math.Max(checkSize, imageSize + textSize);

                    case Composition.TextImageCombined:
                        return (Math.Max(imageSize, textSize) + checkSize);

                    case Composition.AllCombined:
                        return Math.Max(Math.Max(checkSize, imageSize), textSize);
                }
                return -7107;
            }

            private int xDecompose(Composition composition, int checkSize, int imageSize, int proposedSize)
            {
                switch (composition)
                {
                    case Composition.NoneCombined:
                        return (proposedSize - (checkSize + imageSize));

                    case Composition.CheckCombined:
                        return (proposedSize - imageSize);

                    case Composition.TextImageCombined:
                        return (proposedSize - checkSize);

                    case Composition.AllCombined:
                        return proposedSize;
                }
                return -7109;
            }

            private int FullBorderSize
            {
                get
                {
                    if (this.OnePixExtraBorder)
                    {
                        this.borderSize++;
                    }
                    return this.borderSize;
                }
            }

            private int FullCheckSize
            {
                get
                {
                    return (this.checkSize + this.checkPaddingSize);
                }
            }

            private bool OnePixExtraBorder
            {
                get
                {
                    return (this.growBorderBy1PxWhenDefault && this.isDefault);
                }
            }

            public System.Drawing.StringFormat StringFormat
            {
                get
                {
                    System.Drawing.StringFormat format = new System.Drawing.StringFormat {
                        FormatFlags = this.gdipFormatFlags,
                        Trimming = this.gdipTrimming,
                        HotkeyPrefix = this.gdipHotkeyPrefix,
                        Alignment = this.gdipAlignment,
                        LineAlignment = this.gdipLineAlignment
                    };
                    if (this.disableWordWrapping)
                    {
                        format.FormatFlags |= StringFormatFlags.NoWrap;
                    }
                    return format;
                }
                set
                {
                    this.gdipFormatFlags = value.FormatFlags;
                    this.gdipTrimming = value.Trimming;
                    this.gdipHotkeyPrefix = value.HotkeyPrefix;
                    this.gdipAlignment = value.Alignment;
                    this.gdipLineAlignment = value.LineAlignment;
                }
            }

            public System.Windows.Forms.TextFormatFlags TextFormatFlags
            {
                get
                {
                    if (this.disableWordWrapping)
                    {
                        return (this.gdiTextFormatFlags & ~System.Windows.Forms.TextFormatFlags.WordBreak);
                    }
                    return this.gdiTextFormatFlags;
                }
            }

            private enum Composition
            {
                NoneCombined,
                CheckCombined,
                TextImageCombined,
                AllCombined
            }
        }
    }
}

