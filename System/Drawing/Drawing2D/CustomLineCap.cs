namespace System.Drawing.Drawing2D
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    public class CustomLineCap : MarshalByRefObject, ICloneable, IDisposable
    {
        private bool disposed;
        internal SafeCustomLineCapHandle nativeCap;

        internal CustomLineCap()
        {
        }

        internal CustomLineCap(IntPtr nativeLineCap)
        {
            this.SetNativeLineCap(nativeLineCap);
        }

        public CustomLineCap(GraphicsPath fillPath, GraphicsPath strokePath) : this(fillPath, strokePath, LineCap.Flat)
        {
        }

        public CustomLineCap(GraphicsPath fillPath, GraphicsPath strokePath, LineCap baseCap) : this(fillPath, strokePath, baseCap, 0f)
        {
        }

        public CustomLineCap(GraphicsPath fillPath, GraphicsPath strokePath, LineCap baseCap, float baseInset)
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateCustomLineCap(new HandleRef(fillPath, (fillPath == null) ? IntPtr.Zero : fillPath.nativePath), new HandleRef(strokePath, (strokePath == null) ? IntPtr.Zero : strokePath.nativePath), baseCap, baseInset, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            this.SetNativeLineCap(zero);
        }

        private LineCap _GetBaseCap()
        {
            LineCap cap;
            int status = SafeNativeMethods.Gdip.GdipGetCustomLineCapBaseCap(new HandleRef(this, (IntPtr) this.nativeCap), out cap);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return cap;
        }

        private float _GetBaseInset()
        {
            float num;
            int status = SafeNativeMethods.Gdip.GdipGetCustomLineCapBaseInset(new HandleRef(this, (IntPtr) this.nativeCap), out num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return num;
        }

        private LineJoin _GetStrokeJoin()
        {
            LineJoin join;
            int status = SafeNativeMethods.Gdip.GdipGetCustomLineCapStrokeJoin(new HandleRef(this, (IntPtr) this.nativeCap), out join);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return join;
        }

        private float _GetWidthScale()
        {
            float num;
            int status = SafeNativeMethods.Gdip.GdipGetCustomLineCapWidthScale(new HandleRef(this, (IntPtr) this.nativeCap), out num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return num;
        }

        private void _SetBaseCap(LineCap baseCap)
        {
            int status = SafeNativeMethods.Gdip.GdipSetCustomLineCapBaseCap(new HandleRef(this, (IntPtr) this.nativeCap), baseCap);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        private void _SetBaseInset(float inset)
        {
            int status = SafeNativeMethods.Gdip.GdipSetCustomLineCapBaseInset(new HandleRef(this, (IntPtr) this.nativeCap), inset);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        private void _SetStrokeJoin(LineJoin lineJoin)
        {
            int status = SafeNativeMethods.Gdip.GdipSetCustomLineCapStrokeJoin(new HandleRef(this, (IntPtr) this.nativeCap), lineJoin);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        private void _SetWidthScale(float widthScale)
        {
            int status = SafeNativeMethods.Gdip.GdipSetCustomLineCapWidthScale(new HandleRef(this, (IntPtr) this.nativeCap), widthScale);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public object Clone()
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCloneCustomLineCap(new HandleRef(this, (IntPtr) this.nativeCap), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return CreateCustomLineCapObject(zero);
        }

        internal static CustomLineCap CreateCustomLineCapObject(IntPtr cap)
        {
            CustomLineCapType capType = CustomLineCapType.Default;
            int status = SafeNativeMethods.Gdip.GdipGetCustomLineCapType(new HandleRef(null, cap), out capType);
            if (status != 0)
            {
                SafeNativeMethods.Gdip.GdipDeleteCustomLineCap(new HandleRef(null, cap));
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            switch (capType)
            {
                case CustomLineCapType.Default:
                    return new CustomLineCap(cap);

                case CustomLineCapType.AdjustableArrowCap:
                    return new AdjustableArrowCap(cap);
            }
            SafeNativeMethods.Gdip.GdipDeleteCustomLineCap(new HandleRef(null, cap));
            throw SafeNativeMethods.Gdip.StatusException(6);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing && (this.nativeCap != null))
                {
                    this.nativeCap.Dispose();
                }
                this.disposed = true;
            }
        }

        ~CustomLineCap()
        {
            this.Dispose(false);
        }

        public void GetStrokeCaps(out LineCap startCap, out LineCap endCap)
        {
            int status = SafeNativeMethods.Gdip.GdipGetCustomLineCapStrokeCaps(new HandleRef(this, (IntPtr) this.nativeCap), out startCap, out endCap);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        internal void SetNativeLineCap(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                throw new ArgumentNullException("handle");
            }
            this.nativeCap = new SafeCustomLineCapHandle(handle);
        }

        public void SetStrokeCaps(LineCap startCap, LineCap endCap)
        {
            int status = SafeNativeMethods.Gdip.GdipSetCustomLineCapStrokeCaps(new HandleRef(this, (IntPtr) this.nativeCap), startCap, endCap);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public LineCap BaseCap
        {
            get
            {
                return this._GetBaseCap();
            }
            set
            {
                this._SetBaseCap(value);
            }
        }

        public float BaseInset
        {
            get
            {
                return this._GetBaseInset();
            }
            set
            {
                this._SetBaseInset(value);
            }
        }

        public LineJoin StrokeJoin
        {
            get
            {
                return this._GetStrokeJoin();
            }
            set
            {
                this._SetStrokeJoin(value);
            }
        }

        public float WidthScale
        {
            get
            {
                return this._GetWidthScale();
            }
            set
            {
                this._SetWidthScale(value);
            }
        }
    }
}

