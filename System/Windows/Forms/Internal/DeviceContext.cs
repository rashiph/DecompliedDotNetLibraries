namespace System.Windows.Forms.Internal
{
    using System;
    using System.Collections;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class DeviceContext : MarshalByRefObject, IDeviceContext, IDisposable
    {
        private Stack contextStack;
        private System.Windows.Forms.Internal.DeviceContextType dcType;
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
        private WindowsFont selectedFont;

        public event EventHandler Disposing;

        private DeviceContext(IntPtr hWnd)
        {
            this.hWnd = (IntPtr) (-1);
            this.hWnd = hWnd;
            this.dcType = System.Windows.Forms.Internal.DeviceContextType.Display;
            DeviceContexts.AddDeviceContext(this);
        }

        private DeviceContext(IntPtr hDC, System.Windows.Forms.Internal.DeviceContextType dcType)
        {
            this.hWnd = (IntPtr) (-1);
            this.hDC = hDC;
            this.dcType = dcType;
            this.CacheInitialState();
            DeviceContexts.AddDeviceContext(this);
            if (dcType == System.Windows.Forms.Internal.DeviceContextType.Display)
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
            return new DeviceContext(IntUnsafeNativeMethods.CreateDC(driverName, deviceName, fileName, devMode), System.Windows.Forms.Internal.DeviceContextType.NamedDevice);
        }

        public static DeviceContext CreateIC(string driverName, string deviceName, string fileName, HandleRef devMode)
        {
            return new DeviceContext(IntUnsafeNativeMethods.CreateIC(driverName, deviceName, fileName, devMode), System.Windows.Forms.Internal.DeviceContextType.Information);
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
                this.DisposeFont(disposing);
                switch (this.dcType)
                {
                    case System.Windows.Forms.Internal.DeviceContextType.Unknown:
                    case System.Windows.Forms.Internal.DeviceContextType.NCWindow:
                        return;

                    case System.Windows.Forms.Internal.DeviceContextType.Display:
                        ((IDeviceContext) this).ReleaseHdc();
                        return;

                    case System.Windows.Forms.Internal.DeviceContextType.NamedDevice:
                    case System.Windows.Forms.Internal.DeviceContextType.Information:
                        IntUnsafeNativeMethods.DeleteHDC(new HandleRef(this, this.hDC));
                        this.hDC = IntPtr.Zero;
                        return;

                    case System.Windows.Forms.Internal.DeviceContextType.Memory:
                        IntUnsafeNativeMethods.DeleteDC(new HandleRef(this, this.hDC));
                        this.hDC = IntPtr.Zero;
                        return;
                }
            }
        }

        internal void DisposeFont(bool disposing)
        {
            if (disposing)
            {
                DeviceContexts.RemoveDeviceContext(this);
            }
            if ((this.selectedFont != null) && (this.selectedFont.Hfont != IntPtr.Zero))
            {
                if (IntUnsafeNativeMethods.GetCurrentObject(new HandleRef(this, this.hDC), 6) == this.selectedFont.Hfont)
                {
                    IntUnsafeNativeMethods.SelectObject(new HandleRef(this, this.Hdc), new HandleRef(null, this.hInitialFont));
                    IntPtr hInitialFont = this.hInitialFont;
                }
                this.selectedFont.Dispose(disposing);
                this.selectedFont = null;
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
            return new DeviceContext(IntUnsafeNativeMethods.CreateCompatibleDC(new HandleRef(null, hdc)), System.Windows.Forms.Internal.DeviceContextType.Memory);
        }

        public static DeviceContext FromHdc(IntPtr hdc)
        {
            return new DeviceContext(hdc, System.Windows.Forms.Internal.DeviceContextType.Unknown);
        }

        public static DeviceContext FromHwnd(IntPtr hwnd)
        {
            return new DeviceContext(hwnd);
        }

        public int GetDeviceCapabilities(DeviceCapabilities capabilityIndex)
        {
            return IntUnsafeNativeMethods.GetDeviceCaps(new HandleRef(this, this.Hdc), (int) capabilityIndex);
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

        public bool IsFontOnContextStack(WindowsFont wf)
        {
            if (this.contextStack != null)
            {
                foreach (GraphicsState state in this.contextStack)
                {
                    if (state.hFont == wf.Hfont)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void ResetFont()
        {
            MeasurementDCInfo.ResetIfIsMeasurementDC(this.Hdc);
            IntUnsafeNativeMethods.SelectObject(new HandleRef(this, this.Hdc), new HandleRef(null, this.hInitialFont));
            this.selectedFont = null;
            this.hCurrentFont = this.hInitialFont;
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
                if ((state.font != null) && state.font.IsAlive)
                {
                    this.selectedFont = state.font.Target as WindowsFont;
                }
                else
                {
                    WindowsFont selectedFont = this.selectedFont;
                    this.selectedFont = null;
                    if ((selectedFont != null) && MeasurementDCInfo.IsMeasurementDC(this))
                    {
                        selectedFont.Dispose();
                    }
                }
            }
            MeasurementDCInfo.ResetIfIsMeasurementDC(this.hDC);
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
                hFont = this.hCurrentFont,
                font = new WeakReference(this.selectedFont)
            };
            this.contextStack.Push(state);
            return num;
        }

        public IntPtr SelectFont(WindowsFont font)
        {
            if (font.Equals(this.Font))
            {
                return IntPtr.Zero;
            }
            IntPtr ptr = this.SelectObject(font.Hfont, GdiObjectType.Font);
            WindowsFont selectedFont = this.selectedFont;
            this.selectedFont = font;
            this.hCurrentFont = font.Hfont;
            if ((selectedFont != null) && MeasurementDCInfo.IsMeasurementDC(this))
            {
                selectedFont.Dispose();
            }
            if (MeasurementDCInfo.IsMeasurementDC(this))
            {
                if (ptr != IntPtr.Zero)
                {
                    MeasurementDCInfo.LastUsedFont = font;
                    return ptr;
                }
                MeasurementDCInfo.Reset();
            }
            return ptr;
        }

        public IntPtr SelectObject(IntPtr hObj, GdiObjectType type)
        {
            switch (type)
            {
                case GdiObjectType.Pen:
                    this.hCurrentPen = hObj;
                    break;

                case GdiObjectType.Brush:
                    this.hCurrentBrush = hObj;
                    break;

                case GdiObjectType.Bitmap:
                    this.hCurrentBmp = hObj;
                    break;
            }
            return IntUnsafeNativeMethods.SelectObject(new HandleRef(this, this.Hdc), new HandleRef(null, hObj));
        }

        public Color SetBackgroundColor(Color newColor)
        {
            return ColorTranslator.FromWin32(IntUnsafeNativeMethods.SetBkColor(new HandleRef(this, this.Hdc), ColorTranslator.ToWin32(newColor)));
        }

        public DeviceContextBackgroundMode SetBackgroundMode(DeviceContextBackgroundMode newMode)
        {
            return (DeviceContextBackgroundMode) IntUnsafeNativeMethods.SetBkMode(new HandleRef(this, this.Hdc), (int) newMode);
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

        public DeviceContextMapMode SetMapMode(DeviceContextMapMode newMode)
        {
            return (DeviceContextMapMode) IntUnsafeNativeMethods.SetMapMode(new HandleRef(this, this.Hdc), (int) newMode);
        }

        public DeviceContextBinaryRasterOperationFlags SetRasterOperation(DeviceContextBinaryRasterOperationFlags rasterOperation)
        {
            return (DeviceContextBinaryRasterOperationFlags) IntUnsafeNativeMethods.SetROP2(new HandleRef(this, this.Hdc), (int) rasterOperation);
        }

        public DeviceContextTextAlignment SetTextAlignment(DeviceContextTextAlignment newAligment)
        {
            return (DeviceContextTextAlignment) IntUnsafeNativeMethods.SetTextAlign(new HandleRef(this, this.Hdc), (int) newAligment);
        }

        public Color SetTextColor(Color newColor)
        {
            return ColorTranslator.FromWin32(IntUnsafeNativeMethods.SetTextColor(new HandleRef(this, this.Hdc), ColorTranslator.ToWin32(newColor)));
        }

        public Size SetViewportExtent(Size newExtent)
        {
            IntNativeMethods.SIZE size = new IntNativeMethods.SIZE();
            IntUnsafeNativeMethods.SetViewportExtEx(new HandleRef(this, this.Hdc), newExtent.Width, newExtent.Height, size);
            return size.ToSize();
        }

        public Point SetViewportOrigin(Point newOrigin)
        {
            IntNativeMethods.POINT point = new IntNativeMethods.POINT();
            IntUnsafeNativeMethods.SetViewportOrgEx(new HandleRef(this, this.Hdc), newOrigin.X, newOrigin.Y, point);
            return point.ToPoint();
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
            if ((this.hDC != IntPtr.Zero) && (this.dcType == System.Windows.Forms.Internal.DeviceContextType.Display))
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

        public WindowsFont ActiveFont
        {
            get
            {
                return this.selectedFont;
            }
        }

        public Color BackgroundColor
        {
            get
            {
                return ColorTranslator.FromWin32(IntUnsafeNativeMethods.GetBkColor(new HandleRef(this, this.Hdc)));
            }
        }

        public DeviceContextBackgroundMode BackgroundMode
        {
            get
            {
                return (DeviceContextBackgroundMode) IntUnsafeNativeMethods.GetBkMode(new HandleRef(this, this.Hdc));
            }
        }

        public DeviceContextBinaryRasterOperationFlags BinaryRasterOperation
        {
            get
            {
                return (DeviceContextBinaryRasterOperationFlags) IntUnsafeNativeMethods.GetROP2(new HandleRef(this, this.Hdc));
            }
        }

        public System.Windows.Forms.Internal.DeviceContextType DeviceContextType
        {
            get
            {
                return this.dcType;
            }
        }

        public Size Dpi
        {
            get
            {
                return new Size(this.GetDeviceCapabilities(DeviceCapabilities.LogicalPixelsX), this.GetDeviceCapabilities(DeviceCapabilities.LogicalPixelsY));
            }
        }

        public int DpiX
        {
            get
            {
                return this.GetDeviceCapabilities(DeviceCapabilities.LogicalPixelsX);
            }
        }

        public int DpiY
        {
            get
            {
                return this.GetDeviceCapabilities(DeviceCapabilities.LogicalPixelsY);
            }
        }

        public WindowsFont Font
        {
            get
            {
                if (MeasurementDCInfo.IsMeasurementDC(this))
                {
                    WindowsFont lastUsedFont = MeasurementDCInfo.LastUsedFont;
                    if ((lastUsedFont != null) && (lastUsedFont.Hfont != IntPtr.Zero))
                    {
                        return lastUsedFont;
                    }
                }
                return WindowsFont.FromHdc(this.Hdc);
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
                if ((this.hDC == IntPtr.Zero) && (this.dcType == System.Windows.Forms.Internal.DeviceContextType.Display))
                {
                    this.hDC = ((IDeviceContext) this).GetHdc();
                    this.CacheInitialState();
                }
                return this.hDC;
            }
        }

        public DeviceContextMapMode MapMode
        {
            get
            {
                return (DeviceContextMapMode) IntUnsafeNativeMethods.GetMapMode(new HandleRef(this, this.Hdc));
            }
        }

        public static DeviceContext ScreenDC
        {
            get
            {
                return FromHwnd(IntPtr.Zero);
            }
        }

        public DeviceContextTextAlignment TextAlignment
        {
            get
            {
                return (DeviceContextTextAlignment) IntUnsafeNativeMethods.GetTextAlign(new HandleRef(this, this.Hdc));
            }
        }

        public Color TextColor
        {
            get
            {
                return ColorTranslator.FromWin32(IntUnsafeNativeMethods.GetTextColor(new HandleRef(this, this.Hdc)));
            }
        }

        public Size ViewportExtent
        {
            get
            {
                IntNativeMethods.SIZE lpSize = new IntNativeMethods.SIZE();
                IntUnsafeNativeMethods.GetViewportExtEx(new HandleRef(this, this.Hdc), lpSize);
                return lpSize.ToSize();
            }
            set
            {
                this.SetViewportExtent(value);
            }
        }

        public Point ViewportOrigin
        {
            get
            {
                IntNativeMethods.POINT lpPoint = new IntNativeMethods.POINT();
                IntUnsafeNativeMethods.GetViewportOrgEx(new HandleRef(this, this.Hdc), lpPoint);
                return lpPoint.ToPoint();
            }
            set
            {
                this.SetViewportOrigin(value);
            }
        }

        internal class GraphicsState
        {
            internal WeakReference font;
            internal IntPtr hBitmap;
            internal IntPtr hBrush;
            internal IntPtr hFont;
            internal IntPtr hPen;
        }
    }
}

