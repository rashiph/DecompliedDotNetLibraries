namespace System.Drawing.Drawing2D
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    public sealed class AdjustableArrowCap : CustomLineCap
    {
        internal AdjustableArrowCap(IntPtr nativeCap) : base(nativeCap)
        {
        }

        public AdjustableArrowCap(float width, float height) : this(width, height, true)
        {
        }

        public AdjustableArrowCap(float width, float height, bool isFilled)
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateAdjustableArrowCap(height, width, isFilled, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeLineCap(zero);
        }

        private float _GetHeight()
        {
            float num;
            int status = SafeNativeMethods.Gdip.GdipGetAdjustableArrowCapHeight(new HandleRef(this, (IntPtr) base.nativeCap), out num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return num;
        }

        private float _GetMiddleInset()
        {
            float num;
            int status = SafeNativeMethods.Gdip.GdipGetAdjustableArrowCapMiddleInset(new HandleRef(this, (IntPtr) base.nativeCap), out num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return num;
        }

        private float _GetWidth()
        {
            float num;
            int status = SafeNativeMethods.Gdip.GdipGetAdjustableArrowCapWidth(new HandleRef(this, (IntPtr) base.nativeCap), out num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return num;
        }

        private bool _IsFilled()
        {
            bool fillState = false;
            int status = SafeNativeMethods.Gdip.GdipGetAdjustableArrowCapFillState(new HandleRef(this, (IntPtr) base.nativeCap), out fillState);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return fillState;
        }

        private void _SetFillState(bool isFilled)
        {
            int status = SafeNativeMethods.Gdip.GdipSetAdjustableArrowCapFillState(new HandleRef(this, (IntPtr) base.nativeCap), isFilled);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        private void _SetHeight(float height)
        {
            int status = SafeNativeMethods.Gdip.GdipSetAdjustableArrowCapHeight(new HandleRef(this, (IntPtr) base.nativeCap), height);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        private void _SetMiddleInset(float middleInset)
        {
            int status = SafeNativeMethods.Gdip.GdipSetAdjustableArrowCapMiddleInset(new HandleRef(this, (IntPtr) base.nativeCap), middleInset);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        private void _SetWidth(float width)
        {
            int status = SafeNativeMethods.Gdip.GdipSetAdjustableArrowCapWidth(new HandleRef(this, (IntPtr) base.nativeCap), width);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public bool Filled
        {
            get
            {
                return this._IsFilled();
            }
            set
            {
                this._SetFillState(value);
            }
        }

        public float Height
        {
            get
            {
                return this._GetHeight();
            }
            set
            {
                this._SetHeight(value);
            }
        }

        public float MiddleInset
        {
            get
            {
                return this._GetMiddleInset();
            }
            set
            {
                this._SetMiddleInset(value);
            }
        }

        public float Width
        {
            get
            {
                return this._GetWidth();
            }
            set
            {
                this._SetWidth(value);
            }
        }
    }
}

