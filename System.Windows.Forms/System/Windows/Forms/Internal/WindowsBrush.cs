namespace System.Windows.Forms.Internal
{
    using System;
    using System.Drawing;

    internal abstract class WindowsBrush : MarshalByRefObject, ICloneable, IDisposable
    {
        private System.Drawing.Color color;
        private DeviceContext dc;
        private IntPtr nativeHandle;

        public WindowsBrush(DeviceContext dc)
        {
            this.color = System.Drawing.Color.White;
            this.dc = dc;
        }

        public WindowsBrush(DeviceContext dc, System.Drawing.Color color)
        {
            this.color = System.Drawing.Color.White;
            this.dc = dc;
            this.color = color;
        }

        public abstract object Clone();
        protected abstract void CreateBrush();
        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if ((this.dc != null) && (this.nativeHandle != IntPtr.Zero))
            {
                this.dc.DeleteObject(this.nativeHandle, GdiObjectType.Brush);
                this.nativeHandle = IntPtr.Zero;
            }
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        ~WindowsBrush()
        {
            this.Dispose(false);
        }

        public System.Drawing.Color Color
        {
            get
            {
                return this.color;
            }
        }

        protected DeviceContext DC
        {
            get
            {
                return this.dc;
            }
        }

        public IntPtr HBrush
        {
            get
            {
                return this.NativeHandle;
            }
        }

        protected IntPtr NativeHandle
        {
            get
            {
                if (this.nativeHandle == IntPtr.Zero)
                {
                    this.CreateBrush();
                }
                return this.nativeHandle;
            }
            set
            {
                this.nativeHandle = value;
            }
        }
    }
}

