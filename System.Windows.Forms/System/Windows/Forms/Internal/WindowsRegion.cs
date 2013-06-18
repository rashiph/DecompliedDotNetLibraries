namespace System.Windows.Forms.Internal
{
    using System;
    using System.Drawing;
    using System.Internal;
    using System.Runtime.InteropServices;

    internal sealed class WindowsRegion : MarshalByRefObject, ICloneable, IDisposable
    {
        private IntPtr nativeHandle;
        private bool ownHandle;

        private WindowsRegion()
        {
        }

        public WindowsRegion(Rectangle rect)
        {
            this.CreateRegion(rect);
        }

        public WindowsRegion(int x, int y, int width, int height)
        {
            this.CreateRegion(new Rectangle(x, y, width, height));
        }

        public object Clone()
        {
            if (!this.IsInfinite)
            {
                return new WindowsRegion(this.ToRectangle());
            }
            return new WindowsRegion();
        }

        public IntNativeMethods.RegionFlags CombineRegion(WindowsRegion region1, WindowsRegion region2, RegionCombineMode mode)
        {
            return IntUnsafeNativeMethods.CombineRgn(new HandleRef(this, this.HRegion), new HandleRef(region1, region1.HRegion), new HandleRef(region2, region2.HRegion), mode);
        }

        private void CreateRegion(Rectangle rect)
        {
            this.nativeHandle = IntSafeNativeMethods.CreateRectRgn(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
            this.ownHandle = true;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            if (this.nativeHandle != IntPtr.Zero)
            {
                if (this.ownHandle)
                {
                    IntUnsafeNativeMethods.DeleteObject(new HandleRef(this, this.nativeHandle));
                }
                this.nativeHandle = IntPtr.Zero;
                if (disposing)
                {
                    GC.SuppressFinalize(this);
                }
            }
        }

        ~WindowsRegion()
        {
            this.Dispose(false);
        }

        public static WindowsRegion FromHregion(IntPtr hRegion, bool takeOwnership)
        {
            WindowsRegion region = new WindowsRegion();
            if (hRegion != IntPtr.Zero)
            {
                region.nativeHandle = hRegion;
                if (takeOwnership)
                {
                    region.ownHandle = true;
                    System.Internal.HandleCollector.Add(hRegion, IntSafeNativeMethods.CommonHandles.GDI);
                }
            }
            return region;
        }

        public static WindowsRegion FromRegion(Region region, Graphics g)
        {
            if (region.IsInfinite(g))
            {
                return new WindowsRegion();
            }
            return FromHregion(region.GetHrgn(g), true);
        }

        public Rectangle ToRectangle()
        {
            if (this.IsInfinite)
            {
                return new Rectangle(-2147483647, -2147483647, 0x7fffffff, 0x7fffffff);
            }
            IntNativeMethods.RECT clipRect = new IntNativeMethods.RECT();
            IntUnsafeNativeMethods.GetRgnBox(new HandleRef(this, this.nativeHandle), ref clipRect);
            return new Rectangle(new Point(clipRect.left, clipRect.top), clipRect.Size);
        }

        public IntPtr HRegion
        {
            get
            {
                return this.nativeHandle;
            }
        }

        public bool IsInfinite
        {
            get
            {
                return (this.nativeHandle == IntPtr.Zero);
            }
        }
    }
}

