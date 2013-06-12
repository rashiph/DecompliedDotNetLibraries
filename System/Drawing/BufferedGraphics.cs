namespace System.Drawing
{
    using System;
    using System.Runtime.InteropServices;

    public sealed class BufferedGraphics : IDisposable
    {
        private System.Drawing.Graphics bufferedGraphicsSurface;
        private BufferedGraphicsContext context;
        private bool disposeContext;
        private static int rop = 0xcc0020;
        private IntPtr targetDC;
        private System.Drawing.Graphics targetGraphics;
        private Point targetLoc;
        private Size virtualSize;

        internal BufferedGraphics(System.Drawing.Graphics bufferedGraphicsSurface, BufferedGraphicsContext context, System.Drawing.Graphics targetGraphics, IntPtr targetDC, Point targetLoc, Size virtualSize)
        {
            this.context = context;
            this.bufferedGraphicsSurface = bufferedGraphicsSurface;
            this.targetDC = targetDC;
            this.targetGraphics = targetGraphics;
            this.targetLoc = targetLoc;
            this.virtualSize = virtualSize;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.context != null)
                {
                    this.context.ReleaseBuffer(this);
                    if (this.DisposeContext)
                    {
                        this.context.Dispose();
                        this.context = null;
                    }
                }
                if (this.bufferedGraphicsSurface != null)
                {
                    this.bufferedGraphicsSurface.Dispose();
                    this.bufferedGraphicsSurface = null;
                }
            }
        }

        ~BufferedGraphics()
        {
            this.Dispose(false);
        }

        public void Render()
        {
            if (this.targetGraphics != null)
            {
                this.Render(this.targetGraphics);
            }
            else
            {
                this.RenderInternal(new HandleRef(this.Graphics, this.targetDC), this);
            }
        }

        public void Render(System.Drawing.Graphics target)
        {
            if (target != null)
            {
                IntPtr hdc = target.GetHdc();
                try
                {
                    this.RenderInternal(new HandleRef(target, hdc), this);
                }
                finally
                {
                    target.ReleaseHdcInternal(hdc);
                }
            }
        }

        public void Render(IntPtr targetDC)
        {
            IntSecurity.UnmanagedCode.Demand();
            this.RenderInternal(new HandleRef(null, targetDC), this);
        }

        private void RenderInternal(HandleRef refTargetDC, BufferedGraphics buffer)
        {
            IntPtr hdc = buffer.Graphics.GetHdc();
            try
            {
                SafeNativeMethods.BitBlt(refTargetDC, this.targetLoc.X, this.targetLoc.Y, this.virtualSize.Width, this.virtualSize.Height, new HandleRef(buffer.Graphics, hdc), 0, 0, rop);
            }
            finally
            {
                buffer.Graphics.ReleaseHdcInternal(hdc);
            }
        }

        internal bool DisposeContext
        {
            get
            {
                return this.disposeContext;
            }
            set
            {
                this.disposeContext = value;
            }
        }

        public System.Drawing.Graphics Graphics
        {
            get
            {
                return this.bufferedGraphicsSurface;
            }
        }
    }
}

