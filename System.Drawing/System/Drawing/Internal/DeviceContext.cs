namespace System.Drawing.Internal
{
    using System;
    using System.Collections;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class DeviceContext : MarshalByRefObject, IDeviceContext, IDisposable
    {
        private Stack contextStack;
        private System.Drawing.Internal.DeviceContextType dcType;
        private bool disposed;
        private IntPtr hCurrentBmp;
        private IntPtr hCurrentBrush;
        private IntPtr hCurrentFont;
        private IntPtr hCurrentPen;
        private IntPtr hDC;
        private IntPtr hInitialBmp;
        private IntPtr hInitialBrush;
        private IntPtr hInitialFont;
        private IntPtr hInitialPen;
        private IntPtr hWnd;

        public event EventHandler Disposing;

        private DeviceContext(IntPtr hWnd)
        {
            this.hWnd = (IntPtr) (-1);
            this.hWnd = hWnd;
            this.dcType = System.Drawing.Internal.DeviceContextType.Display;
            DeviceContexts.AddDeviceContext(this);
        }

        private DeviceContext(IntPtr hDC, System.Drawing.Internal.DeviceContextType dcType)
        {
            this.hWnd = (IntPtr) (-1);
            this.hDC = hDC;
            this.dcType = dcType;
            this.CacheInitialState();
            DeviceContexts.AddDeviceContext(this);
            if (dcType == System.Drawing.Internal.DeviceContextType.Display)
            {
                this.hWnd = IntUnsafeNativeMethods.WindowFromDC(new HandleRef(this, this.hDC));
            }
        }

        private void CacheInitialState()
        {
            this.hCurrentPen = this.hInitialPen = IntUnsafeNativeMethods.GetCurrentObject(new HandleRef(this, this.hDC), 1);
            this.hCurrentBrush = this.hInitialBrush = IntUnsafeNativeMethods.GetCurrentObject(new HandleRef(this, this.hDC), 2);
            this.hCurrentBmp = this.hInitialBmp = IntUnsafeNativeMethods.GetCurrentObject(new HandleRef(this, this.hDC), 7);
            this.hCurrentFont = this.hInitialFont = IntUnsafeNativeMethods.GetCurrentObject(new HandleRef(this, this.hDC), 6);
        }

        public static DeviceContext CreateDC(string driverName, string deviceName, string fileName, HandleRef devMode)
        {
            return new DeviceContext(IntUnsafeNativeMethods.CreateDC(driverName, deviceName, fileName, devMode), System.Drawing.Internal.DeviceContextType.NamedDevice);
        }

        public static DeviceContext CreateIC(string driverName, string deviceName, string fileName, HandleRef devMode)
        {
            return new DeviceContext(IntUnsafeNativeMethods.CreateIC(driverName, deviceName, fileName, devMode), System.Drawing.Internal.DeviceContextType.Information);
        }

        public void DeleteObject(IntPtr handle, GdiObjectType type)
        {
            IntPtr zero = IntPtr.Zero;
            switch (type)
            {
                case GdiObjectType.Pen:
                    if (handle == this.hCurrentPen)
                    {
                        IntUnsafeNativeMethods.SelectObject(new HandleRef(this, this.Hdc), new HandleRef(this, this.hInitialPen));
                        this.hCurrentPen = IntPtr.Zero;
                    }
                    zero = handle;
                    break;

                case GdiObjectType.Brush:
                    if (handle == this.hCurrentBrush)
                    {
                        IntUnsafeNativeMethods.SelectObject(new HandleRef(this, this.Hdc), new HandleRef(this, this.hInitialBrush));
                        this.hCurrentBrush = IntPtr.Zero;
                    }
                    zero = handle;
                    break;

                case GdiObjectType.Bitmap:
                    if (handle == this.hCurrentBmp)
                    {
                        IntUnsafeNativeMethods.SelectObject(new HandleRef(this, this.Hdc), new HandleRef(this, this.hInitialBmp));
                        this.hCurrentBmp = IntPtr.Zero;
                    }
                    zero = handle;
                    break;
            }
            IntUnsafeNativeMethods.DeleteObject(new HandleRef(this, zero));
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (this.Disposing != null)
                {
                    this.Disposing(this, EventArgs.Empty);
                }
                this.disposed = true;
                switch (this.dcType)
                {
                    case System.Drawing.Internal.DeviceContextType.Unknown:
                    case System.Drawing.Internal.DeviceContextType.NCWindow:
                        return;

                    case System.Drawing.Internal.DeviceContextType.Display:
                        ((IDeviceContext) this).ReleaseHdc();
                        return;

                    case System.Drawing.Internal.DeviceContextType.NamedDevice:
                    case System.Drawing.Internal.DeviceContextType.Information:
                        IntUnsafeNativeMethods.DeleteHDC(new HandleRef(this, this.hDC));
                        this.hDC = IntPtr.Zero;
                        return;

                    case System.Drawing.Internal.DeviceContextType.Memory:
                        IntUnsafeNativeMethods.DeleteDC(new HandleRef(this, this.hDC));
                        this.hDC = IntPtr.Zero;
                        return;
                }
            }
        }

        public override bool Equals(object obj)
        {
            DeviceContext context = obj as DeviceContext;
            if (context == this)
            {
                return true;
            }
            if (context == null)
            {
                return false;
            }
            return (context.Hdc == this.Hdc);
        }

        ~DeviceContext()
        {
            this.Dispose(false);
        }

        public static DeviceContext FromCompatibleDC(IntPtr hdc)
        {
            return new DeviceContext(IntUnsafeNativeMethods.CreateCompatibleDC(new HandleRef(null, hdc)), System.Drawing.Internal.DeviceContextType.Memory);
        }

        public static DeviceContext FromHdc(IntPtr hdc)
        {
            return new DeviceContext(hdc, System.Drawing.Internal.DeviceContextType.Unknown);
        }

        public static DeviceContext FromHwnd(IntPtr hwnd)
        {
            return new DeviceContext(hwnd);
        }

        public override int GetHashCode()
        {
            return this.Hdc.GetHashCode();
        }

        public void IntersectClip(WindowsRegion wr)
        {
            if (wr.HRegion != IntPtr.Zero)
            {
                using (WindowsRegion region = new WindowsRegion(0, 0, 0, 0))
                {
                    if (IntUnsafeNativeMethods.GetClipRgn(new HandleRef(this, this.Hdc), new HandleRef(region, region.HRegion)) == 1)
                    {
                        wr.CombineRegion(region, wr, RegionCombineMode.AND);
                    }
                    this.SetClip(wr);
                }
            }
        }

        public void RestoreHdc()
        {
            IntUnsafeNativeMethods.RestoreDC(new HandleRef(this, this.hDC), -1);
            if (this.contextStack != null)
            {
                GraphicsState state = (GraphicsState) this.contextStack.Pop();
                this.hCurrentBmp = state.hBitmap;
                this.hCurrentBrush = state.hBrush;
                this.hCurrentPen = state.hPen;
                this.hCurrentFont = state.hFont;
            }
        }

        public int SaveHdc()
        {
            HandleRef hDC = new HandleRef(this, this.Hdc);
            int num = IntUnsafeNativeMethods.SaveDC(hDC);
            if (this.contextStack == null)
            {
                this.contextStack = new Stack();
            }
            GraphicsState state = new GraphicsState {
                hBitmap = this.hCurrentBmp,
                hBrush = this.hCurrentBrush,
                hPen = this.hCurrentPen,
                hFont = this.hCurrentFont
            };
            this.contextStack.Push(state);
            return num;
        }

        public void SetClip(WindowsRegion region)
        {
            HandleRef hDC = new HandleRef(this, this.Hdc);
            HandleRef hRgn = new HandleRef(region, region.HRegion);
            IntUnsafeNativeMethods.SelectClipRgn(hDC, hRgn);
        }

        public DeviceContextGraphicsMode SetGraphicsMode(DeviceContextGraphicsMode newMode)
        {
            return (DeviceContextGraphicsMode) IntUnsafeNativeMethods.SetGraphicsMode(new HandleRef(this, this.Hdc), (int) newMode);
        }

        IntPtr IDeviceContext.GetHdc()
        {
            if (this.hDC == IntPtr.Zero)
            {
                this.hDC = IntUnsafeNativeMethods.GetDC(new HandleRef(this, this.hWnd));
            }
            return this.hDC;
        }

        void IDeviceContext.ReleaseHdc()
        {
            if ((this.hDC != IntPtr.Zero) && (this.dcType == System.Drawing.Internal.DeviceContextType.Display))
            {
                IntUnsafeNativeMethods.ReleaseDC(new HandleRef(this, this.hWnd), new HandleRef(this, this.hDC));
                this.hDC = IntPtr.Zero;
            }
        }

        public void TranslateTransform(int dx, int dy)
        {
            IntNativeMethods.POINT point = new IntNativeMethods.POINT();
            IntUnsafeNativeMethods.OffsetViewportOrgEx(new HandleRef(this, this.Hdc), dx, dy, point);
        }

        public System.Drawing.Internal.DeviceContextType DeviceContextType
        {
            get
            {
                return this.dcType;
            }
        }

        public DeviceContextGraphicsMode GraphicsMode
        {
            get
            {
                return (DeviceContextGraphicsMode) IntUnsafeNativeMethods.GetGraphicsMode(new HandleRef(this, this.Hdc));
            }
        }

        public IntPtr Hdc
        {
            get
            {
                if ((this.hDC == IntPtr.Zero) && (this.dcType == System.Drawing.Internal.DeviceContextType.Display))
                {
                    this.hDC = ((IDeviceContext) this).GetHdc();
                    this.CacheInitialState();
                }
                return this.hDC;
            }
        }

        internal class GraphicsState
        {
            internal IntPtr hBitmap;
            internal IntPtr hBrush;
            internal IntPtr hFont;
            internal IntPtr hPen;
        }
    }
}

