namespace System.Windows.Forms.Internal
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal sealed class WindowsGraphics : MarshalByRefObject, IDeviceContext, IDisposable
    {
        private System.Windows.Forms.Internal.DeviceContext dc;
        private bool disposeDc;
        public const int GdiUnsupportedFlagMask = -16777216;
        private Graphics graphics;
        private const float ItalicPaddingFactor = 0.5f;
        public static readonly Size MaxSize = new Size(0x7fffffff, 0x7fffffff);
        private TextPaddingOptions paddingFlags;

        public WindowsGraphics(System.Windows.Forms.Internal.DeviceContext dc)
        {
            this.dc = dc;
            this.dc.SaveHdc();
        }

        public static Rectangle AdjustForVerticalAlignment(HandleRef hdc, string text, Rectangle bounds, IntTextFormatFlags flags, IntNativeMethods.DRAWTEXTPARAMS dtparams)
        {
            if (((((flags & IntTextFormatFlags.Bottom) == IntTextFormatFlags.Default) && ((flags & IntTextFormatFlags.VerticalCenter) == IntTextFormatFlags.Default)) || ((flags & IntTextFormatFlags.SingleLine) != IntTextFormatFlags.Default)) || ((flags & IntTextFormatFlags.CalculateRectangle) != IntTextFormatFlags.Default))
            {
                return bounds;
            }
            IntNativeMethods.RECT lpRect = new IntNativeMethods.RECT(bounds);
            flags |= IntTextFormatFlags.CalculateRectangle;
            int num = IntUnsafeNativeMethods.DrawTextEx(hdc, text, ref lpRect, (int) flags, dtparams);
            if (num > bounds.Height)
            {
                return bounds;
            }
            Rectangle rectangle = bounds;
            if ((flags & IntTextFormatFlags.VerticalCenter) != IntTextFormatFlags.Default)
            {
                rectangle.Y = (rectangle.Top + (rectangle.Height / 2)) - (num / 2);
                return rectangle;
            }
            rectangle.Y = rectangle.Bottom - num;
            return rectangle;
        }

        public static WindowsGraphics CreateMeasurementWindowsGraphics()
        {
            return new WindowsGraphics(System.Windows.Forms.Internal.DeviceContext.FromCompatibleDC(IntPtr.Zero)) { disposeDc = true };
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void Dispose(bool disposing)
        {
            if (this.dc != null)
            {
                try
                {
                    this.dc.RestoreHdc();
                    if (this.disposeDc)
                    {
                        this.dc.Dispose(disposing);
                    }
                    if (this.graphics != null)
                    {
                        this.graphics.ReleaseHdcInternal(this.dc.Hdc);
                        this.graphics = null;
                    }
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                    {
                        throw;
                    }
                }
                finally
                {
                    this.dc = null;
                }
            }
        }

        public void DrawAndFillEllipse(WindowsPen pen, WindowsBrush brush, Rectangle bounds)
        {
            this.DrawEllipse(pen, brush, bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
        }

        private void DrawEllipse(WindowsPen pen, WindowsBrush brush, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect)
        {
            HandleRef hdc = new HandleRef(this.dc, this.dc.Hdc);
            if (pen != null)
            {
                IntUnsafeNativeMethods.SelectObject(hdc, new HandleRef(pen, pen.HPen));
            }
            if (brush != null)
            {
                IntUnsafeNativeMethods.SelectObject(hdc, new HandleRef(brush, brush.HBrush));
            }
            IntUnsafeNativeMethods.Ellipse(hdc, nLeftRect, nTopRect, nRightRect, nBottomRect);
        }

        public void DrawLine(WindowsPen pen, Point p1, Point p2)
        {
            this.DrawLine(pen, p1.X, p1.Y, p2.X, p2.Y);
        }

        public void DrawLine(WindowsPen pen, int x1, int y1, int x2, int y2)
        {
            HandleRef hdc = new HandleRef(this.dc, this.dc.Hdc);
            DeviceContextBinaryRasterOperationFlags binaryRasterOperation = this.dc.BinaryRasterOperation;
            DeviceContextBackgroundMode backgroundMode = this.dc.BackgroundMode;
            if (binaryRasterOperation != DeviceContextBinaryRasterOperationFlags.CopyPen)
            {
                binaryRasterOperation = this.dc.SetRasterOperation(DeviceContextBinaryRasterOperationFlags.CopyPen);
            }
            if (backgroundMode != DeviceContextBackgroundMode.Transparent)
            {
                backgroundMode = this.dc.SetBackgroundMode(DeviceContextBackgroundMode.Transparent);
            }
            if (pen != null)
            {
                this.dc.SelectObject(pen.HPen, GdiObjectType.Pen);
            }
            IntNativeMethods.POINT pt = new IntNativeMethods.POINT();
            IntUnsafeNativeMethods.MoveToEx(hdc, x1, y1, pt);
            IntUnsafeNativeMethods.LineTo(hdc, x2, y2);
            if (backgroundMode != DeviceContextBackgroundMode.Transparent)
            {
                this.dc.SetBackgroundMode(backgroundMode);
            }
            if (binaryRasterOperation != DeviceContextBinaryRasterOperationFlags.CopyPen)
            {
                this.dc.SetRasterOperation(binaryRasterOperation);
            }
            IntUnsafeNativeMethods.MoveToEx(hdc, pt.x, pt.y, null);
        }

        public void DrawPie(WindowsPen pen, Rectangle bounds, float startAngle, float sweepAngle)
        {
            HandleRef hdc = new HandleRef(this.dc, this.dc.Hdc);
            if (pen != null)
            {
                IntUnsafeNativeMethods.SelectObject(hdc, new HandleRef(pen, pen.HPen));
            }
            int num = Math.Min(bounds.Width, bounds.Height);
            Point point = new Point(bounds.X + (num / 2), bounds.Y + (num / 2));
            int radius = num / 2;
            IntUnsafeNativeMethods.BeginPath(hdc);
            IntUnsafeNativeMethods.MoveToEx(hdc, point.X, point.Y, null);
            IntUnsafeNativeMethods.AngleArc(hdc, point.X, point.Y, radius, startAngle, sweepAngle);
            IntUnsafeNativeMethods.LineTo(hdc, point.X, point.Y);
            IntUnsafeNativeMethods.EndPath(hdc);
            IntUnsafeNativeMethods.StrokePath(hdc);
        }

        public void DrawRectangle(WindowsPen pen, Rectangle rect)
        {
            this.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void DrawRectangle(WindowsPen pen, int x, int y, int width, int height)
        {
            HandleRef hdc = new HandleRef(this.dc, this.dc.Hdc);
            if (pen != null)
            {
                this.dc.SelectObject(pen.HPen, GdiObjectType.Pen);
            }
            DeviceContextBinaryRasterOperationFlags binaryRasterOperation = this.dc.BinaryRasterOperation;
            if (binaryRasterOperation != DeviceContextBinaryRasterOperationFlags.CopyPen)
            {
                binaryRasterOperation = this.dc.SetRasterOperation(DeviceContextBinaryRasterOperationFlags.CopyPen);
            }
            IntUnsafeNativeMethods.SelectObject(hdc, new HandleRef(null, IntUnsafeNativeMethods.GetStockObject(5)));
            IntUnsafeNativeMethods.Rectangle(hdc, x, y, x + width, y + height);
            if (binaryRasterOperation != DeviceContextBinaryRasterOperationFlags.CopyPen)
            {
                this.dc.SetRasterOperation(binaryRasterOperation);
            }
        }

        public void DrawText(string text, WindowsFont font, Point pt, Color foreColor)
        {
            this.DrawText(text, font, pt, foreColor, Color.Empty, IntTextFormatFlags.Default);
        }

        public void DrawText(string text, WindowsFont font, Rectangle bounds, Color foreColor)
        {
            this.DrawText(text, font, bounds, foreColor, Color.Empty);
        }

        public void DrawText(string text, WindowsFont font, Point pt, Color foreColor, Color backColor)
        {
            this.DrawText(text, font, pt, foreColor, backColor, IntTextFormatFlags.Default);
        }

        public void DrawText(string text, WindowsFont font, Point pt, Color foreColor, IntTextFormatFlags flags)
        {
            this.DrawText(text, font, pt, foreColor, Color.Empty, flags);
        }

        public void DrawText(string text, WindowsFont font, Rectangle bounds, Color foreColor, Color backColor)
        {
            this.DrawText(text, font, bounds, foreColor, backColor, IntTextFormatFlags.VerticalCenter | IntTextFormatFlags.HorizontalCenter);
        }

        public void DrawText(string text, WindowsFont font, Rectangle bounds, Color color, IntTextFormatFlags flags)
        {
            this.DrawText(text, font, bounds, color, Color.Empty, flags);
        }

        public void DrawText(string text, WindowsFont font, Point pt, Color foreColor, Color backColor, IntTextFormatFlags flags)
        {
            Rectangle bounds = new Rectangle(pt.X, pt.Y, 0x7fffffff, 0x7fffffff);
            this.DrawText(text, font, bounds, foreColor, backColor, flags);
        }

        public void DrawText(string text, WindowsFont font, Rectangle bounds, Color foreColor, Color backColor, IntTextFormatFlags flags)
        {
            if (!string.IsNullOrEmpty(text) && (foreColor != Color.Transparent))
            {
                HandleRef hdc = new HandleRef(this.dc, this.dc.Hdc);
                if (this.dc.TextAlignment != DeviceContextTextAlignment.Top)
                {
                    this.dc.SetTextAlignment(DeviceContextTextAlignment.Top);
                }
                if (!foreColor.IsEmpty && (foreColor != this.dc.TextColor))
                {
                    this.dc.SetTextColor(foreColor);
                }
                if (font != null)
                {
                    this.dc.SelectFont(font);
                }
                DeviceContextBackgroundMode newMode = (backColor.IsEmpty || (backColor == Color.Transparent)) ? DeviceContextBackgroundMode.Transparent : DeviceContextBackgroundMode.Opaque;
                if (this.dc.BackgroundMode != newMode)
                {
                    this.dc.SetBackgroundMode(newMode);
                }
                if ((newMode != DeviceContextBackgroundMode.Transparent) && (backColor != this.dc.BackgroundColor))
                {
                    this.dc.SetBackgroundColor(backColor);
                }
                IntNativeMethods.DRAWTEXTPARAMS textMargins = this.GetTextMargins(font);
                bounds = AdjustForVerticalAlignment(hdc, text, bounds, flags, textMargins);
                if (bounds.Width == MaxSize.Width)
                {
                    bounds.Width -= bounds.X;
                }
                if (bounds.Height == MaxSize.Height)
                {
                    bounds.Height -= bounds.Y;
                }
                IntNativeMethods.RECT lpRect = new IntNativeMethods.RECT(bounds);
                IntUnsafeNativeMethods.DrawTextEx(hdc, text, ref lpRect, (int) flags, textMargins);
            }
        }

        public void FillRectangle(WindowsBrush brush, Rectangle rect)
        {
            this.FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void FillRectangle(WindowsBrush brush, int x, int y, int width, int height)
        {
            HandleRef hDC = new HandleRef(this.dc, this.dc.Hdc);
            IntPtr hBrush = brush.HBrush;
            IntNativeMethods.RECT rect = new IntNativeMethods.RECT(x, y, x + width, y + height);
            IntUnsafeNativeMethods.FillRect(hDC, ref rect, new HandleRef(brush, hBrush));
        }

        ~WindowsGraphics()
        {
            this.Dispose(false);
        }

        public static WindowsGraphics FromGraphics(Graphics g)
        {
            ApplyGraphicsProperties all = ApplyGraphicsProperties.All;
            return FromGraphics(g, all);
        }

        public static WindowsGraphics FromGraphics(Graphics g, ApplyGraphicsProperties properties)
        {
            WindowsRegion wr = null;
            float[] elements = null;
            Region region = null;
            Matrix matrix = null;
            if (((properties & ApplyGraphicsProperties.TranslateTransform) != ApplyGraphicsProperties.None) || ((properties & ApplyGraphicsProperties.Clipping) != ApplyGraphicsProperties.None))
            {
                object[] contextInfo = g.GetContextInfo() as object[];
                if ((contextInfo != null) && (contextInfo.Length == 2))
                {
                    region = contextInfo[0] as Region;
                    matrix = contextInfo[1] as Matrix;
                }
                if (matrix != null)
                {
                    if ((properties & ApplyGraphicsProperties.TranslateTransform) != ApplyGraphicsProperties.None)
                    {
                        elements = matrix.Elements;
                    }
                    matrix.Dispose();
                }
                if (region != null)
                {
                    if (((properties & ApplyGraphicsProperties.Clipping) != ApplyGraphicsProperties.None) && !region.IsInfinite(g))
                    {
                        wr = WindowsRegion.FromRegion(region, g);
                    }
                    region.Dispose();
                }
            }
            WindowsGraphics graphics = FromHdc(g.GetHdc());
            graphics.graphics = g;
            if (wr != null)
            {
                using (wr)
                {
                    graphics.DeviceContext.IntersectClip(wr);
                }
            }
            if (elements != null)
            {
                graphics.DeviceContext.TranslateTransform((int) elements[4], (int) elements[5]);
            }
            return graphics;
        }

        public static WindowsGraphics FromHdc(IntPtr hDc)
        {
            return new WindowsGraphics(System.Windows.Forms.Internal.DeviceContext.FromHdc(hDc)) { disposeDc = true };
        }

        public static WindowsGraphics FromHwnd(IntPtr hWnd)
        {
            return new WindowsGraphics(System.Windows.Forms.Internal.DeviceContext.FromHwnd(hWnd)) { disposeDc = true };
        }

        public IntPtr GetHdc()
        {
            return this.dc.Hdc;
        }

        public Color GetNearestColor(Color color)
        {
            HandleRef hDC = new HandleRef(null, this.dc.Hdc);
            return ColorTranslator.FromWin32(IntUnsafeNativeMethods.GetNearestColor(hDC, ColorTranslator.ToWin32(color)));
        }

        public float GetOverhangPadding(WindowsFont font)
        {
            WindowsFont font2 = font;
            if (font2 == null)
            {
                font2 = this.dc.Font;
            }
            float num = ((float) font2.Height) / 6f;
            if (font2 != font)
            {
                font2.Dispose();
            }
            return num;
        }

        public Size GetTextExtent(string text, WindowsFont font)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Size.Empty;
            }
            IntNativeMethods.SIZE size = new IntNativeMethods.SIZE();
            HandleRef hDC = new HandleRef(null, this.dc.Hdc);
            if (font != null)
            {
                this.dc.SelectFont(font);
            }
            IntUnsafeNativeMethods.GetTextExtentPoint32(hDC, text, size);
            if ((font != null) && !MeasurementDCInfo.IsMeasurementDC(this.dc))
            {
                this.dc.ResetFont();
            }
            return new Size(size.cx, size.cy);
        }

        public IntNativeMethods.DRAWTEXTPARAMS GetTextMargins(WindowsFont font)
        {
            int leftMargin = 0;
            int rightMargin = 0;
            float overhangPadding = 0f;
            switch (this.TextPadding)
            {
                case TextPaddingOptions.GlyphOverhangPadding:
                    overhangPadding = this.GetOverhangPadding(font);
                    leftMargin = (int) Math.Ceiling((double) overhangPadding);
                    rightMargin = (int) Math.Ceiling((double) (overhangPadding * 1.5f));
                    break;

                case TextPaddingOptions.LeftAndRightPadding:
                    overhangPadding = this.GetOverhangPadding(font);
                    leftMargin = (int) Math.Ceiling((double) (2f * overhangPadding));
                    rightMargin = (int) Math.Ceiling((double) (overhangPadding * 2.5f));
                    break;
            }
            return new IntNativeMethods.DRAWTEXTPARAMS(leftMargin, rightMargin);
        }

        public IntNativeMethods.TEXTMETRIC GetTextMetrics()
        {
            IntNativeMethods.TEXTMETRIC lptm = new IntNativeMethods.TEXTMETRIC();
            HandleRef hDC = new HandleRef(this.dc, this.dc.Hdc);
            bool flag = this.dc.MapMode != DeviceContextMapMode.Text;
            if (flag)
            {
                this.dc.SaveHdc();
            }
            try
            {
                if (flag)
                {
                    DeviceContextMapMode mode = this.dc.SetMapMode(DeviceContextMapMode.Text);
                }
                IntUnsafeNativeMethods.GetTextMetrics(hDC, ref lptm);
            }
            finally
            {
                if (flag)
                {
                    this.dc.RestoreHdc();
                }
            }
            return lptm;
        }

        public Size MeasureText(string text, WindowsFont font)
        {
            return this.MeasureText(text, font, MaxSize, IntTextFormatFlags.Default);
        }

        public Size MeasureText(string text, WindowsFont font, Size proposedSize)
        {
            return this.MeasureText(text, font, proposedSize, IntTextFormatFlags.Default);
        }

        public Size MeasureText(string text, WindowsFont font, Size proposedSize, IntTextFormatFlags flags)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Size.Empty;
            }
            IntNativeMethods.DRAWTEXTPARAMS lpDTParams = null;
            if (MeasurementDCInfo.IsMeasurementDC(this.DeviceContext))
            {
                lpDTParams = MeasurementDCInfo.GetTextMargins(this, font);
            }
            if (lpDTParams == null)
            {
                lpDTParams = this.GetTextMargins(font);
            }
            int num = (1 + lpDTParams.iLeftMargin) + lpDTParams.iRightMargin;
            if (proposedSize.Width <= num)
            {
                proposedSize.Width = num;
            }
            if (proposedSize.Height <= 0)
            {
                proposedSize.Height = 1;
            }
            IntNativeMethods.RECT lpRect = IntNativeMethods.RECT.FromXYWH(0, 0, proposedSize.Width, proposedSize.Height);
            HandleRef hDC = new HandleRef(null, this.dc.Hdc);
            if (font != null)
            {
                this.dc.SelectFont(font);
            }
            if ((proposedSize.Height >= MaxSize.Height) && ((flags & IntTextFormatFlags.SingleLine) != IntTextFormatFlags.Default))
            {
                flags &= ~(IntTextFormatFlags.Bottom | IntTextFormatFlags.VerticalCenter);
            }
            if (proposedSize.Width == MaxSize.Width)
            {
                flags &= ~IntTextFormatFlags.WordBreak;
            }
            flags |= IntTextFormatFlags.CalculateRectangle;
            IntUnsafeNativeMethods.DrawTextEx(hDC, text, ref lpRect, (int) flags, lpDTParams);
            return lpRect.Size;
        }

        public void ReleaseHdc()
        {
            this.dc.Dispose();
        }

        public System.Windows.Forms.Internal.DeviceContext DeviceContext
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.dc;
            }
        }

        public TextPaddingOptions TextPadding
        {
            get
            {
                return this.paddingFlags;
            }
            set
            {
                if (this.paddingFlags != value)
                {
                    this.paddingFlags = value;
                }
            }
        }
    }
}

