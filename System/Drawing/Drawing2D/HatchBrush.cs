namespace System.Drawing.Drawing2D
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    public sealed class HatchBrush : Brush
    {
        internal HatchBrush(IntPtr nativeBrush)
        {
            base.SetNativeBrushInternal(nativeBrush);
        }

        public HatchBrush(System.Drawing.Drawing2D.HatchStyle hatchstyle, Color foreColor) : this(hatchstyle, foreColor, Color.FromArgb(-16777216))
        {
        }

        public HatchBrush(System.Drawing.Drawing2D.HatchStyle hatchstyle, Color foreColor, Color backColor)
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateHatchBrush((int) hatchstyle, foreColor.ToArgb(), backColor.ToArgb(), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeBrushInternal(zero);
        }

        public override object Clone()
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCloneBrush(new HandleRef(this, base.NativeBrush), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return new HatchBrush(zero);
        }

        public Color BackgroundColor
        {
            get
            {
                int num;
                int status = SafeNativeMethods.Gdip.GdipGetHatchBackgroundColor(new HandleRef(this, base.NativeBrush), out num);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return Color.FromArgb(num);
            }
        }

        public Color ForegroundColor
        {
            get
            {
                int num;
                int status = SafeNativeMethods.Gdip.GdipGetHatchForegroundColor(new HandleRef(this, base.NativeBrush), out num);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return Color.FromArgb(num);
            }
        }

        public System.Drawing.Drawing2D.HatchStyle HatchStyle
        {
            get
            {
                int hatchstyle = 0;
                int status = SafeNativeMethods.Gdip.GdipGetHatchStyle(new HandleRef(this, base.NativeBrush), out hatchstyle);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return (System.Drawing.Drawing2D.HatchStyle) hatchstyle;
            }
        }
    }
}

