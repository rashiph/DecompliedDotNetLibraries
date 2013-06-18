namespace System.Drawing.Imaging
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public sealed class BitmapData
    {
        private int width;
        private int height;
        private int stride;
        private int pixelFormat;
        private IntPtr scan0;
        private int reserved;
        public int Width
        {
            get
            {
                return this.width;
            }
            set
            {
                this.width = value;
            }
        }
        public int Height
        {
            get
            {
                return this.height;
            }
            set
            {
                this.height = value;
            }
        }
        public int Stride
        {
            get
            {
                return this.stride;
            }
            set
            {
                this.stride = value;
            }
        }
        public System.Drawing.Imaging.PixelFormat PixelFormat
        {
            get
            {
                return (System.Drawing.Imaging.PixelFormat) this.pixelFormat;
            }
            set
            {
                switch (value)
                {
                    case System.Drawing.Imaging.PixelFormat.Format16bppRgb555:
                    case System.Drawing.Imaging.PixelFormat.Format16bppRgb565:
                    case System.Drawing.Imaging.PixelFormat.Gdi:
                    case System.Drawing.Imaging.PixelFormat.Indexed:
                    case System.Drawing.Imaging.PixelFormat.Undefined:
                    case System.Drawing.Imaging.PixelFormat.Max:
                    case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                    case System.Drawing.Imaging.PixelFormat.Format1bppIndexed:
                    case System.Drawing.Imaging.PixelFormat.Format4bppIndexed:
                    case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    case System.Drawing.Imaging.PixelFormat.PAlpha:
                    case System.Drawing.Imaging.PixelFormat.Format32bppPArgb:
                    case System.Drawing.Imaging.PixelFormat.Extended:
                    case System.Drawing.Imaging.PixelFormat.Alpha:
                    case System.Drawing.Imaging.PixelFormat.Format16bppArgb1555:
                    case System.Drawing.Imaging.PixelFormat.Format16bppGrayScale:
                    case System.Drawing.Imaging.PixelFormat.Format48bppRgb:
                    case System.Drawing.Imaging.PixelFormat.Format64bppPArgb:
                    case System.Drawing.Imaging.PixelFormat.Canonical:
                    case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    case System.Drawing.Imaging.PixelFormat.Format64bppArgb:
                        this.pixelFormat = (int) value;
                        return;
                }
                throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Drawing.Imaging.PixelFormat));
            }
        }
        public IntPtr Scan0
        {
            get
            {
                return this.scan0;
            }
            set
            {
                this.scan0 = value;
            }
        }
        public int Reserved
        {
            get
            {
                return this.reserved;
            }
            set
            {
                this.reserved = value;
            }
        }
    }
}

