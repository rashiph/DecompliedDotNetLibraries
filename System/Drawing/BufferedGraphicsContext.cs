namespace System.Drawing
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    public sealed class BufferedGraphicsContext : IDisposable
    {
        private BufferedGraphics buffer;
        private const int BUFFER_BUSY_DISPOSING = 2;
        private const int BUFFER_BUSY_PAINTING = 1;
        private const int BUFFER_FREE = 0;
        private Size bufferSize;
        private int busy;
        private IntPtr compatDC;
        private Graphics compatGraphics;
        private IntPtr dib;
        private static TraceSwitch doubleBuffering;
        private bool invalidateWhenFree;
        private Size maximumBuffer;
        private IntPtr oldBitmap;
        private Point targetLoc;
        private Size virtualSize;

        public BufferedGraphicsContext()
        {
            this.maximumBuffer.Width = 0xe1;
            this.maximumBuffer.Height = 0x60;
            this.bufferSize = Size.Empty;
        }

        public BufferedGraphics Allocate(Graphics targetGraphics, Rectangle targetRectangle)
        {
            if (this.ShouldUseTempManager(targetRectangle))
            {
                return this.AllocBufferInTempManager(targetGraphics, IntPtr.Zero, targetRectangle);
            }
            return this.AllocBuffer(targetGraphics, IntPtr.Zero, targetRectangle);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public BufferedGraphics Allocate(IntPtr targetDC, Rectangle targetRectangle)
        {
            if (this.ShouldUseTempManager(targetRectangle))
            {
                return this.AllocBufferInTempManager(null, targetDC, targetRectangle);
            }
            return this.AllocBuffer(null, targetDC, targetRectangle);
        }

        private BufferedGraphics AllocBuffer(Graphics targetGraphics, IntPtr targetDC, Rectangle targetRectangle)
        {
            if (Interlocked.CompareExchange(ref this.busy, 1, 0) != 0)
            {
                return this.AllocBufferInTempManager(targetGraphics, targetDC, targetRectangle);
            }
            this.targetLoc = new Point(targetRectangle.X, targetRectangle.Y);
            try
            {
                Graphics graphics;
                if (targetGraphics != null)
                {
                    IntPtr hdc = targetGraphics.GetHdc();
                    try
                    {
                        graphics = this.CreateBuffer(hdc, -this.targetLoc.X, -this.targetLoc.Y, targetRectangle.Width, targetRectangle.Height);
                    }
                    finally
                    {
                        targetGraphics.ReleaseHdcInternal(hdc);
                    }
                }
                else
                {
                    graphics = this.CreateBuffer(targetDC, -this.targetLoc.X, -this.targetLoc.Y, targetRectangle.Width, targetRectangle.Height);
                }
                this.buffer = new BufferedGraphics(graphics, this, targetGraphics, targetDC, this.targetLoc, this.virtualSize);
            }
            catch
            {
                this.busy = 0;
                throw;
            }
            return this.buffer;
        }

        private BufferedGraphics AllocBufferInTempManager(Graphics targetGraphics, IntPtr targetDC, Rectangle targetRectangle)
        {
            BufferedGraphicsContext context = null;
            BufferedGraphics graphics = null;
            try
            {
                context = new BufferedGraphicsContext();
                if (context != null)
                {
                    graphics = context.AllocBuffer(targetGraphics, targetDC, targetRectangle);
                    graphics.DisposeContext = true;
                }
            }
            finally
            {
                if ((context != null) && ((graphics == null) || ((graphics != null) && !graphics.DisposeContext)))
                {
                    context.Dispose();
                }
            }
            return graphics;
        }

        private bool bFillBitmapInfo(IntPtr hdc, IntPtr hpal, ref System.Drawing.NativeMethods.BITMAPINFO_FLAT pbmi)
        {
            IntPtr zero = IntPtr.Zero;
            bool flag = false;
            try
            {
                zero = SafeNativeMethods.CreateCompatibleBitmap(new HandleRef(null, hdc), 1, 1);
                if (zero == IntPtr.Zero)
                {
                    throw new OutOfMemoryException(System.Drawing.SR.GetString("GraphicsBufferQueryFail"));
                }
                pbmi.bmiHeader_biSize = Marshal.SizeOf(typeof(System.Drawing.NativeMethods.BITMAPINFOHEADER));
                pbmi.bmiColors = new byte[0x400];
                SafeNativeMethods.GetDIBits(new HandleRef(null, hdc), new HandleRef(null, zero), 0, 0, IntPtr.Zero, ref pbmi, 0);
                if (pbmi.bmiHeader_biBitCount <= 8)
                {
                    return this.bFillColorTable(hdc, hpal, ref pbmi);
                }
                if (pbmi.bmiHeader_biCompression == 3)
                {
                    SafeNativeMethods.GetDIBits(new HandleRef(null, hdc), new HandleRef(null, zero), 0, pbmi.bmiHeader_biHeight, IntPtr.Zero, ref pbmi, 0);
                }
                flag = true;
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    SafeNativeMethods.DeleteObject(new HandleRef(null, zero));
                    zero = IntPtr.Zero;
                }
            }
            return flag;
        }

        private unsafe bool bFillColorTable(IntPtr hdc, IntPtr hpal, ref System.Drawing.NativeMethods.BITMAPINFO_FLAT pbmi)
        {
            bool flag = false;
            byte[] lppe = new byte[sizeof(System.Drawing.NativeMethods.PALETTEENTRY) * 0x100];
            fixed (byte* numRef = pbmi.bmiColors)
            {
                fixed (byte* numRef2 = lppe)
                {
                    System.Drawing.NativeMethods.RGBQUAD* rgbquadPtr = (System.Drawing.NativeMethods.RGBQUAD*) numRef;
                    System.Drawing.NativeMethods.PALETTEENTRY* paletteentryPtr = (System.Drawing.NativeMethods.PALETTEENTRY*) numRef2;
                    int nEntries = ((int) 1) << pbmi.bmiHeader_biBitCount;
                    if (nEntries <= 0x100)
                    {
                        uint num3;
                        IntPtr zero = IntPtr.Zero;
                        if (hpal == IntPtr.Zero)
                        {
                            zero = Graphics.GetHalftonePalette();
                            num3 = SafeNativeMethods.GetPaletteEntries(new HandleRef(null, zero), 0, nEntries, lppe);
                        }
                        else
                        {
                            num3 = SafeNativeMethods.GetPaletteEntries(new HandleRef(null, hpal), 0, nEntries, lppe);
                        }
                        if (num3 != 0)
                        {
                            for (int i = 0; i < nEntries; i++)
                            {
                                rgbquadPtr[i].rgbRed = paletteentryPtr[i].peRed;
                                rgbquadPtr[i].rgbGreen = paletteentryPtr[i].peGreen;
                                rgbquadPtr[i].rgbBlue = paletteentryPtr[i].peBlue;
                                rgbquadPtr[i].rgbReserved = 0;
                            }
                            flag = true;
                        }
                    }
                }
            }
            return flag;
        }

        private Graphics CreateBuffer(IntPtr src, int offsetX, int offsetY, int width, int height)
        {
            this.busy = 2;
            this.DisposeDC();
            this.busy = 1;
            this.compatDC = System.Drawing.UnsafeNativeMethods.CreateCompatibleDC(new HandleRef(null, src));
            if ((width > this.bufferSize.Width) || (height > this.bufferSize.Height))
            {
                int ulWidth = Math.Max(width, this.bufferSize.Width);
                int ulHeight = Math.Max(height, this.bufferSize.Height);
                this.busy = 2;
                this.DisposeBitmap();
                this.busy = 1;
                IntPtr zero = IntPtr.Zero;
                this.dib = this.CreateCompatibleDIB(src, IntPtr.Zero, ulWidth, ulHeight, ref zero);
                this.bufferSize = new Size(ulWidth, ulHeight);
            }
            this.oldBitmap = SafeNativeMethods.SelectObject(new HandleRef(this, this.compatDC), new HandleRef(this, this.dib));
            this.compatGraphics = Graphics.FromHdcInternal(this.compatDC);
            this.compatGraphics.TranslateTransform((float) -this.targetLoc.X, (float) -this.targetLoc.Y);
            this.virtualSize = new Size(width, height);
            return this.compatGraphics;
        }

        private IntPtr CreateCompatibleDIB(IntPtr hdc, IntPtr hpal, int ulWidth, int ulHeight, ref IntPtr ppvBits)
        {
            if (hdc == IntPtr.Zero)
            {
                throw new ArgumentNullException("hdc");
            }
            IntPtr zero = IntPtr.Zero;
            System.Drawing.NativeMethods.BITMAPINFO_FLAT pbmi = new System.Drawing.NativeMethods.BITMAPINFO_FLAT();
            switch (System.Drawing.UnsafeNativeMethods.GetObjectType(new HandleRef(null, hdc)))
            {
                case 3:
                case 4:
                case 10:
                case 12:
                    if (this.bFillBitmapInfo(hdc, hpal, ref pbmi))
                    {
                        pbmi.bmiHeader_biWidth = ulWidth;
                        pbmi.bmiHeader_biHeight = ulHeight;
                        if (pbmi.bmiHeader_biCompression == 0)
                        {
                            pbmi.bmiHeader_biSizeImage = 0;
                        }
                        else if (pbmi.bmiHeader_biBitCount == 0x10)
                        {
                            pbmi.bmiHeader_biSizeImage = (ulWidth * ulHeight) * 2;
                        }
                        else if (pbmi.bmiHeader_biBitCount == 0x20)
                        {
                            pbmi.bmiHeader_biSizeImage = (ulWidth * ulHeight) * 4;
                        }
                        else
                        {
                            pbmi.bmiHeader_biSizeImage = 0;
                        }
                        pbmi.bmiHeader_biClrUsed = 0;
                        pbmi.bmiHeader_biClrImportant = 0;
                        zero = SafeNativeMethods.CreateDIBSection(new HandleRef(null, hdc), ref pbmi, 0, ref ppvBits, IntPtr.Zero, 0);
                        Win32Exception exception = null;
                        if (zero == IntPtr.Zero)
                        {
                            exception = new Win32Exception(Marshal.GetLastWin32Error());
                        }
                        if (exception != null)
                        {
                            throw exception;
                        }
                    }
                    return zero;
            }
            throw new ArgumentException(System.Drawing.SR.GetString("DCTypeInvalid"));
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            int num = Interlocked.CompareExchange(ref this.busy, 2, 0);
            if (disposing)
            {
                if (num == 1)
                {
                    throw new InvalidOperationException(System.Drawing.SR.GetString("GraphicsBufferCurrentlyBusy"));
                }
                if (this.compatGraphics != null)
                {
                    this.compatGraphics.Dispose();
                    this.compatGraphics = null;
                }
            }
            this.DisposeDC();
            this.DisposeBitmap();
            if (this.buffer != null)
            {
                this.buffer.Dispose();
                this.buffer = null;
            }
            this.bufferSize = Size.Empty;
            this.virtualSize = Size.Empty;
            this.busy = 0;
        }

        private void DisposeBitmap()
        {
            if (this.dib != IntPtr.Zero)
            {
                SafeNativeMethods.DeleteObject(new HandleRef(this, this.dib));
                this.dib = IntPtr.Zero;
            }
        }

        private void DisposeDC()
        {
            if ((this.oldBitmap != IntPtr.Zero) && (this.compatDC != IntPtr.Zero))
            {
                SafeNativeMethods.SelectObject(new HandleRef(this, this.compatDC), new HandleRef(this, this.oldBitmap));
                this.oldBitmap = IntPtr.Zero;
            }
            if (this.compatDC != IntPtr.Zero)
            {
                System.Drawing.UnsafeNativeMethods.DeleteDC(new HandleRef(this, this.compatDC));
                this.compatDC = IntPtr.Zero;
            }
        }

        ~BufferedGraphicsContext()
        {
            this.Dispose(false);
        }

        public void Invalidate()
        {
            if (Interlocked.CompareExchange(ref this.busy, 2, 0) == 0)
            {
                this.Dispose();
                this.busy = 0;
            }
            else
            {
                this.invalidateWhenFree = true;
            }
        }

        internal void ReleaseBuffer(BufferedGraphics buffer)
        {
            this.buffer = null;
            if (this.invalidateWhenFree)
            {
                this.busy = 2;
                this.Dispose();
            }
            else
            {
                this.busy = 2;
                this.DisposeDC();
            }
            this.busy = 0;
        }

        private bool ShouldUseTempManager(Rectangle targetBounds)
        {
            return ((targetBounds.Width * targetBounds.Height) > (this.MaximumBuffer.Width * this.MaximumBuffer.Height));
        }

        internal static TraceSwitch DoubleBuffering
        {
            get
            {
                if (doubleBuffering == null)
                {
                    doubleBuffering = new TraceSwitch("DoubleBuffering", "Output information about double buffering");
                }
                return doubleBuffering;
            }
        }

        public Size MaximumBuffer
        {
            get
            {
                return this.maximumBuffer;
            }
            [UIPermission(SecurityAction.Demand, Window=UIPermissionWindow.AllWindows)]
            set
            {
                if ((value.Width <= 0) || (value.Height <= 0))
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("InvalidArgument", new object[] { "MaximumBuffer", value }));
                }
                if ((value.Width * value.Height) < (this.maximumBuffer.Width * this.maximumBuffer.Height))
                {
                    this.Invalidate();
                }
                this.maximumBuffer = value;
            }
        }
    }
}

