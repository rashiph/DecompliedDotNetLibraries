namespace System.Drawing
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.InteropServices;

    public abstract class Brush : MarshalByRefObject, ICloneable, IDisposable
    {
        private IntPtr nativeBrush;

        protected Brush()
        {
        }

        public abstract object Clone();
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.nativeBrush != IntPtr.Zero)
            {
                try
                {
                    SafeNativeMethods.Gdip.GdipDeleteBrush(new HandleRef(this, this.nativeBrush));
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
                    this.nativeBrush = IntPtr.Zero;
                }
            }
        }

        ~Brush()
        {
            this.Dispose(false);
        }

        protected internal void SetNativeBrush(IntPtr brush)
        {
            System.Drawing.IntSecurity.UnmanagedCode.Demand();
            this.SetNativeBrushInternal(brush);
        }

        internal void SetNativeBrushInternal(IntPtr brush)
        {
            this.nativeBrush = brush;
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        internal IntPtr NativeBrush
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.nativeBrush;
            }
        }
    }
}

