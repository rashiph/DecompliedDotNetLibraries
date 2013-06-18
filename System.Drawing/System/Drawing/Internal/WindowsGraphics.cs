namespace System.Drawing.Internal
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime;

    internal sealed class WindowsGraphics : MarshalByRefObject, IDeviceContext, IDisposable
    {
        private System.Drawing.Internal.DeviceContext dc;
        private bool disposeDc;
        private Graphics graphics;

        public WindowsGraphics(System.Drawing.Internal.DeviceContext dc)
        {
            this.dc = dc;
            this.dc.SaveHdc();
        }

        public static WindowsGraphics CreateMeasurementWindowsGraphics()
        {
            return new WindowsGraphics(System.Drawing.Internal.DeviceContext.FromCompatibleDC(IntPtr.Zero)) { disposeDc = true };
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
                    if (System.Drawing.ClientUtils.IsSecurityOrCriticalException(exception))
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
            return new WindowsGraphics(System.Drawing.Internal.DeviceContext.FromHdc(hDc)) { disposeDc = true };
        }

        public static WindowsGraphics FromHwnd(IntPtr hWnd)
        {
            return new WindowsGraphics(System.Drawing.Internal.DeviceContext.FromHwnd(hWnd)) { disposeDc = true };
        }

        public IntPtr GetHdc()
        {
            return this.dc.Hdc;
        }

        public void ReleaseHdc()
        {
            this.dc.Dispose();
        }

        public System.Drawing.Internal.DeviceContext DeviceContext
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.dc;
            }
        }
    }
}

