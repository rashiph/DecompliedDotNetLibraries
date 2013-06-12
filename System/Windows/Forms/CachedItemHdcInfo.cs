namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    internal class CachedItemHdcInfo : IDisposable
    {
        private Size cachedHDCSize = Size.Empty;
        private HandleRef cachedItemBitmap = System.Windows.Forms.NativeMethods.NullHandleRef;
        private HandleRef cachedItemHDC = System.Windows.Forms.NativeMethods.NullHandleRef;

        internal CachedItemHdcInfo()
        {
        }

        private void DeleteCachedItemHDC()
        {
            if (this.cachedItemHDC.Handle != IntPtr.Zero)
            {
                if (this.cachedItemBitmap.Handle != IntPtr.Zero)
                {
                    System.Windows.Forms.SafeNativeMethods.DeleteObject(this.cachedItemBitmap);
                    this.cachedItemBitmap = System.Windows.Forms.NativeMethods.NullHandleRef;
                }
                System.Windows.Forms.UnsafeNativeMethods.DeleteCompatibleDC(this.cachedItemHDC);
            }
            this.cachedItemHDC = System.Windows.Forms.NativeMethods.NullHandleRef;
            this.cachedItemBitmap = System.Windows.Forms.NativeMethods.NullHandleRef;
            this.cachedHDCSize = Size.Empty;
        }

        public void Dispose()
        {
            this.DeleteCachedItemHDC();
            GC.SuppressFinalize(this);
        }

        ~CachedItemHdcInfo()
        {
            this.Dispose();
        }

        public HandleRef GetCachedItemDC(HandleRef toolStripHDC, Size bitmapSize)
        {
            if ((this.cachedHDCSize.Width < bitmapSize.Width) || (this.cachedHDCSize.Height < bitmapSize.Height))
            {
                if (this.cachedItemHDC.Handle == IntPtr.Zero)
                {
                    IntPtr ptr = System.Windows.Forms.UnsafeNativeMethods.CreateCompatibleDC(toolStripHDC);
                    this.cachedItemHDC = new HandleRef(this, ptr);
                }
                this.cachedItemBitmap = new HandleRef(this, System.Windows.Forms.SafeNativeMethods.CreateCompatibleBitmap(toolStripHDC, bitmapSize.Width, bitmapSize.Height));
                IntPtr handle = System.Windows.Forms.SafeNativeMethods.SelectObject(this.cachedItemHDC, this.cachedItemBitmap);
                if (handle != IntPtr.Zero)
                {
                    System.Windows.Forms.SafeNativeMethods.ExternalDeleteObject(new HandleRef(null, handle));
                    handle = IntPtr.Zero;
                }
                this.cachedHDCSize = bitmapSize;
            }
            return this.cachedItemHDC;
        }
    }
}

