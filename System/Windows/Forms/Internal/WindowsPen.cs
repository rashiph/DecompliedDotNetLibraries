namespace System.Windows.Forms.Internal
{
    using System;
    using System.Drawing;
    using System.Globalization;

    internal sealed class WindowsPen : MarshalByRefObject, ICloneable, IDisposable
    {
        private Color color;
        private const int cosmeticPenWidth = 1;
        private const int dashStyleMask = 15;
        private DeviceContext dc;
        private const int endCapMask = 0xf00;
        private const int joinMask = 0xf000;
        private IntPtr nativeHandle;
        private WindowsPenStyle style;
        private int width;
        private WindowsBrush wndBrush;

        public WindowsPen(DeviceContext dc) : this(dc, WindowsPenStyle.Cosmetic, 1, Color.Black)
        {
        }

        public WindowsPen(DeviceContext dc, Color color) : this(dc, WindowsPenStyle.Cosmetic, 1, color)
        {
        }

        public WindowsPen(DeviceContext dc, WindowsBrush windowsBrush) : this(dc, WindowsPenStyle.Cosmetic, 1, windowsBrush)
        {
        }

        public WindowsPen(DeviceContext dc, WindowsPenStyle style, int width, Color color)
        {
            this.style = style;
            this.width = width;
            this.color = color;
            this.dc = dc;
        }

        public WindowsPen(DeviceContext dc, WindowsPenStyle style, int width, WindowsBrush windowsBrush)
        {
            this.style = style;
            this.wndBrush = (WindowsBrush) windowsBrush.Clone();
            this.width = width;
            this.color = windowsBrush.Color;
            this.dc = dc;
        }

        public object Clone()
        {
            if (this.wndBrush == null)
            {
                return new WindowsPen(this.dc, this.style, this.width, this.color);
            }
            return new WindowsPen(this.dc, this.style, this.width, (WindowsBrush) this.wndBrush.Clone());
        }

        private void CreatePen()
        {
            if (this.width > 1)
            {
                this.style |= WindowsPenStyle.Geometric;
            }
            if (this.wndBrush == null)
            {
                this.nativeHandle = IntSafeNativeMethods.CreatePen((int) this.style, this.width, ColorTranslator.ToWin32(this.color));
            }
            else
            {
                IntNativeMethods.LOGBRUSH lplb = new IntNativeMethods.LOGBRUSH {
                    lbColor = ColorTranslator.ToWin32(this.wndBrush.Color),
                    lbStyle = 0,
                    lbHatch = 0
                };
                this.nativeHandle = IntSafeNativeMethods.ExtCreatePen((int) this.style, this.width, lplb, 0, null);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if ((this.nativeHandle != IntPtr.Zero) && (this.dc != null))
            {
                this.dc.DeleteObject(this.nativeHandle, GdiObjectType.Pen);
                this.nativeHandle = IntPtr.Zero;
            }
            if (this.wndBrush != null)
            {
                this.wndBrush.Dispose();
                this.wndBrush = null;
            }
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        ~WindowsPen()
        {
            this.Dispose(false);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}: Style={1}, Color={2}, Width={3}, Brush={4}", new object[] { base.GetType().Name, this.style, this.color, this.width, (this.wndBrush != null) ? this.wndBrush.ToString() : "null" });
        }

        public IntPtr HPen
        {
            get
            {
                if (this.nativeHandle == IntPtr.Zero)
                {
                    this.CreatePen();
                }
                return this.nativeHandle;
            }
        }
    }
}

