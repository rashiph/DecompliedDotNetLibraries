namespace System.Drawing.Drawing2D
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Internal;
    using System.Runtime.InteropServices;

    public sealed class LinearGradientBrush : Brush
    {
        private bool interpolationColorsWasSet;

        internal LinearGradientBrush(IntPtr nativeBrush)
        {
            base.SetNativeBrushInternal(nativeBrush);
        }

        public LinearGradientBrush(Point point1, Point point2, Color color1, Color color2)
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateLineBrushI(new GPPOINT(point1), new GPPOINT(point2), color1.ToArgb(), color2.ToArgb(), 0, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeBrushInternal(zero);
        }

        public LinearGradientBrush(PointF point1, PointF point2, Color color1, Color color2)
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateLineBrush(new GPPOINTF(point1), new GPPOINTF(point2), color1.ToArgb(), color2.ToArgb(), 0, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeBrushInternal(zero);
        }

        public LinearGradientBrush(System.Drawing.Rectangle rect, Color color1, Color color2, LinearGradientMode linearGradientMode)
        {
            if (!System.Drawing.ClientUtils.IsEnumValid(linearGradientMode, (int) linearGradientMode, 0, 3))
            {
                throw new InvalidEnumArgumentException("linearGradientMode", (int) linearGradientMode, typeof(LinearGradientMode));
            }
            if ((rect.Width == 0) || (rect.Height == 0))
            {
                throw new ArgumentException(System.Drawing.SR.GetString("GdiplusInvalidRectangle", new object[] { rect.ToString() }));
            }
            IntPtr zero = IntPtr.Zero;
            GPRECT gprect = new GPRECT(rect);
            int status = SafeNativeMethods.Gdip.GdipCreateLineBrushFromRectI(ref gprect, color1.ToArgb(), color2.ToArgb(), (int) linearGradientMode, 0, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeBrushInternal(zero);
        }

        public LinearGradientBrush(System.Drawing.Rectangle rect, Color color1, Color color2, float angle) : this(rect, color1, color2, angle, false)
        {
        }

        public LinearGradientBrush(RectangleF rect, Color color1, Color color2, LinearGradientMode linearGradientMode)
        {
            if (!System.Drawing.ClientUtils.IsEnumValid(linearGradientMode, (int) linearGradientMode, 0, 3))
            {
                throw new InvalidEnumArgumentException("linearGradientMode", (int) linearGradientMode, typeof(LinearGradientMode));
            }
            if ((rect.Width == 0.0) || (rect.Height == 0.0))
            {
                throw new ArgumentException(System.Drawing.SR.GetString("GdiplusInvalidRectangle", new object[] { rect.ToString() }));
            }
            IntPtr zero = IntPtr.Zero;
            GPRECTF gprectf = new GPRECTF(rect);
            int status = SafeNativeMethods.Gdip.GdipCreateLineBrushFromRect(ref gprectf, color1.ToArgb(), color2.ToArgb(), (int) linearGradientMode, 0, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeBrushInternal(zero);
        }

        public LinearGradientBrush(RectangleF rect, Color color1, Color color2, float angle) : this(rect, color1, color2, angle, false)
        {
        }

        public LinearGradientBrush(System.Drawing.Rectangle rect, Color color1, Color color2, float angle, bool isAngleScaleable)
        {
            IntPtr zero = IntPtr.Zero;
            if ((rect.Width == 0) || (rect.Height == 0))
            {
                throw new ArgumentException(System.Drawing.SR.GetString("GdiplusInvalidRectangle", new object[] { rect.ToString() }));
            }
            GPRECT gprect = new GPRECT(rect);
            int status = SafeNativeMethods.Gdip.GdipCreateLineBrushFromRectWithAngleI(ref gprect, color1.ToArgb(), color2.ToArgb(), angle, isAngleScaleable, 0, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeBrushInternal(zero);
        }

        public LinearGradientBrush(RectangleF rect, Color color1, Color color2, float angle, bool isAngleScaleable)
        {
            IntPtr zero = IntPtr.Zero;
            if ((rect.Width == 0.0) || (rect.Height == 0.0))
            {
                throw new ArgumentException(System.Drawing.SR.GetString("GdiplusInvalidRectangle", new object[] { rect.ToString() }));
            }
            GPRECTF gprectf = new GPRECTF(rect);
            int status = SafeNativeMethods.Gdip.GdipCreateLineBrushFromRectWithAngle(ref gprectf, color1.ToArgb(), color2.ToArgb(), angle, isAngleScaleable, 0, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeBrushInternal(zero);
        }

        private System.Drawing.Drawing2D.Blend _GetBlend()
        {
            System.Drawing.Drawing2D.Blend blend;
            if (this.interpolationColorsWasSet)
            {
                return null;
            }
            int count = 0;
            int status = SafeNativeMethods.Gdip.GdipGetLineBlendCount(new HandleRef(this, base.NativeBrush), out count);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            if (count <= 0)
            {
                return null;
            }
            int num3 = count;
            IntPtr zero = IntPtr.Zero;
            IntPtr positions = IntPtr.Zero;
            try
            {
                int cb = 4 * num3;
                zero = Marshal.AllocHGlobal(cb);
                positions = Marshal.AllocHGlobal(cb);
                status = SafeNativeMethods.Gdip.GdipGetLineBlend(new HandleRef(this, base.NativeBrush), zero, positions, num3);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                blend = new System.Drawing.Drawing2D.Blend(num3);
                Marshal.Copy(zero, blend.Factors, 0, num3);
                Marshal.Copy(positions, blend.Positions, 0, num3);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
                if (positions != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(positions);
                }
            }
            return blend;
        }

        private ColorBlend _GetInterpolationColors()
        {
            ColorBlend blend;
            if (!this.interpolationColorsWasSet)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InterpolationColorsCommon", new object[] { System.Drawing.SR.GetString("InterpolationColorsColorBlendNotSet"), "" }));
            }
            int count = 0;
            int status = SafeNativeMethods.Gdip.GdipGetLinePresetBlendCount(new HandleRef(this, base.NativeBrush), out count);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            int num3 = count;
            IntPtr zero = IntPtr.Zero;
            IntPtr positions = IntPtr.Zero;
            try
            {
                int cb = 4 * num3;
                zero = Marshal.AllocHGlobal(cb);
                positions = Marshal.AllocHGlobal(cb);
                status = SafeNativeMethods.Gdip.GdipGetLinePresetBlend(new HandleRef(this, base.NativeBrush), zero, positions, num3);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                blend = new ColorBlend(num3);
                int[] destination = new int[num3];
                Marshal.Copy(zero, destination, 0, num3);
                Marshal.Copy(positions, blend.Positions, 0, num3);
                blend.Colors = new Color[destination.Length];
                for (int i = 0; i < destination.Length; i++)
                {
                    blend.Colors[i] = Color.FromArgb(destination[i]);
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
                if (positions != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(positions);
                }
            }
            return blend;
        }

        private Color[] _GetLinearColors()
        {
            int[] colors = new int[2];
            int status = SafeNativeMethods.Gdip.GdipGetLineColors(new HandleRef(this, base.NativeBrush), colors);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return new Color[] { Color.FromArgb(colors[0]), Color.FromArgb(colors[1]) };
        }

        private RectangleF _GetRectangle()
        {
            GPRECTF gprectf = new GPRECTF();
            int status = SafeNativeMethods.Gdip.GdipGetLineRect(new HandleRef(this, base.NativeBrush), ref gprectf);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return gprectf.ToRectangleF();
        }

        private Matrix _GetTransform()
        {
            Matrix wrapper = new Matrix();
            int status = SafeNativeMethods.Gdip.GdipGetLineTransform(new HandleRef(this, base.NativeBrush), new HandleRef(wrapper, wrapper.nativeMatrix));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return wrapper;
        }

        private System.Drawing.Drawing2D.WrapMode _GetWrapMode()
        {
            int wrapMode = 0;
            int status = SafeNativeMethods.Gdip.GdipGetLineWrapMode(new HandleRef(this, base.NativeBrush), out wrapMode);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return (System.Drawing.Drawing2D.WrapMode) wrapMode;
        }

        private void _SetBlend(System.Drawing.Drawing2D.Blend blend)
        {
            int length = blend.Factors.Length;
            IntPtr zero = IntPtr.Zero;
            IntPtr destination = IntPtr.Zero;
            try
            {
                int cb = 4 * length;
                zero = Marshal.AllocHGlobal(cb);
                destination = Marshal.AllocHGlobal(cb);
                Marshal.Copy(blend.Factors, 0, zero, length);
                Marshal.Copy(blend.Positions, 0, destination, length);
                int status = SafeNativeMethods.Gdip.GdipSetLineBlend(new HandleRef(this, base.NativeBrush), new HandleRef(null, zero), new HandleRef(null, destination), length);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
                if (destination != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(destination);
                }
            }
        }

        private void _SetInterpolationColors(ColorBlend blend)
        {
            this.interpolationColorsWasSet = true;
            if (blend == null)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InterpolationColorsCommon", new object[] { System.Drawing.SR.GetString("InterpolationColorsInvalidColorBlendObject"), "" }));
            }
            if (blend.Colors.Length < 2)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InterpolationColorsCommon", new object[] { System.Drawing.SR.GetString("InterpolationColorsInvalidColorBlendObject"), System.Drawing.SR.GetString("InterpolationColorsLength") }));
            }
            if (blend.Colors.Length != blend.Positions.Length)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InterpolationColorsCommon", new object[] { System.Drawing.SR.GetString("InterpolationColorsInvalidColorBlendObject"), System.Drawing.SR.GetString("InterpolationColorsLengthsDiffer") }));
            }
            if (blend.Positions[0] != 0f)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InterpolationColorsCommon", new object[] { System.Drawing.SR.GetString("InterpolationColorsInvalidColorBlendObject"), System.Drawing.SR.GetString("InterpolationColorsInvalidStartPosition") }));
            }
            if (blend.Positions[blend.Positions.Length - 1] != 1f)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InterpolationColorsCommon", new object[] { System.Drawing.SR.GetString("InterpolationColorsInvalidColorBlendObject"), System.Drawing.SR.GetString("InterpolationColorsInvalidEndPosition") }));
            }
            int length = blend.Colors.Length;
            IntPtr zero = IntPtr.Zero;
            IntPtr destination = IntPtr.Zero;
            try
            {
                int cb = 4 * length;
                zero = Marshal.AllocHGlobal(cb);
                destination = Marshal.AllocHGlobal(cb);
                int[] source = new int[length];
                for (int i = 0; i < length; i++)
                {
                    source[i] = blend.Colors[i].ToArgb();
                }
                Marshal.Copy(source, 0, zero, length);
                Marshal.Copy(blend.Positions, 0, destination, length);
                int status = SafeNativeMethods.Gdip.GdipSetLinePresetBlend(new HandleRef(this, base.NativeBrush), new HandleRef(null, zero), new HandleRef(null, destination), length);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
                if (destination != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(destination);
                }
            }
        }

        private void _SetLinearColors(Color color1, Color color2)
        {
            int status = SafeNativeMethods.Gdip.GdipSetLineColors(new HandleRef(this, base.NativeBrush), color1.ToArgb(), color2.ToArgb());
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        private void _SetTransform(Matrix matrix)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }
            int status = SafeNativeMethods.Gdip.GdipSetLineTransform(new HandleRef(this, base.NativeBrush), new HandleRef(matrix, matrix.nativeMatrix));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        private void _SetWrapMode(System.Drawing.Drawing2D.WrapMode wrapMode)
        {
            int status = SafeNativeMethods.Gdip.GdipSetLineWrapMode(new HandleRef(this, base.NativeBrush), (int) wrapMode);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public override object Clone()
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCloneBrush(new HandleRef(this, base.NativeBrush), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return new LinearGradientBrush(zero);
        }

        public void MultiplyTransform(Matrix matrix)
        {
            this.MultiplyTransform(matrix, MatrixOrder.Prepend);
        }

        public void MultiplyTransform(Matrix matrix, MatrixOrder order)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }
            int status = SafeNativeMethods.Gdip.GdipMultiplyLineTransform(new HandleRef(this, base.NativeBrush), new HandleRef(matrix, matrix.nativeMatrix), order);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void ResetTransform()
        {
            int status = SafeNativeMethods.Gdip.GdipResetLineTransform(new HandleRef(this, base.NativeBrush));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void RotateTransform(float angle)
        {
            this.RotateTransform(angle, MatrixOrder.Prepend);
        }

        public void RotateTransform(float angle, MatrixOrder order)
        {
            int status = SafeNativeMethods.Gdip.GdipRotateLineTransform(new HandleRef(this, base.NativeBrush), angle, order);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void ScaleTransform(float sx, float sy)
        {
            this.ScaleTransform(sx, sy, MatrixOrder.Prepend);
        }

        public void ScaleTransform(float sx, float sy, MatrixOrder order)
        {
            int status = SafeNativeMethods.Gdip.GdipScaleLineTransform(new HandleRef(this, base.NativeBrush), sx, sy, order);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void SetBlendTriangularShape(float focus)
        {
            this.SetBlendTriangularShape(focus, 1f);
        }

        public void SetBlendTriangularShape(float focus, float scale)
        {
            int status = SafeNativeMethods.Gdip.GdipSetLineLinearBlend(new HandleRef(this, base.NativeBrush), focus, scale);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void SetSigmaBellShape(float focus)
        {
            this.SetSigmaBellShape(focus, 1f);
        }

        public void SetSigmaBellShape(float focus, float scale)
        {
            int status = SafeNativeMethods.Gdip.GdipSetLineSigmaBlend(new HandleRef(this, base.NativeBrush), focus, scale);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void TranslateTransform(float dx, float dy)
        {
            this.TranslateTransform(dx, dy, MatrixOrder.Prepend);
        }

        public void TranslateTransform(float dx, float dy, MatrixOrder order)
        {
            int status = SafeNativeMethods.Gdip.GdipTranslateLineTransform(new HandleRef(this, base.NativeBrush), dx, dy, order);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public System.Drawing.Drawing2D.Blend Blend
        {
            get
            {
                return this._GetBlend();
            }
            set
            {
                this._SetBlend(value);
            }
        }

        public bool GammaCorrection
        {
            get
            {
                bool flag;
                int status = SafeNativeMethods.Gdip.GdipGetLineGammaCorrection(new HandleRef(this, base.NativeBrush), out flag);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return flag;
            }
            set
            {
                int status = SafeNativeMethods.Gdip.GdipSetLineGammaCorrection(new HandleRef(this, base.NativeBrush), value);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public ColorBlend InterpolationColors
        {
            get
            {
                return this._GetInterpolationColors();
            }
            set
            {
                this._SetInterpolationColors(value);
            }
        }

        public Color[] LinearColors
        {
            get
            {
                return this._GetLinearColors();
            }
            set
            {
                this._SetLinearColors(value[0], value[1]);
            }
        }

        public RectangleF Rectangle
        {
            get
            {
                return this._GetRectangle();
            }
        }

        public Matrix Transform
        {
            get
            {
                return this._GetTransform();
            }
            set
            {
                this._SetTransform(value);
            }
        }

        public System.Drawing.Drawing2D.WrapMode WrapMode
        {
            get
            {
                return this._GetWrapMode();
            }
            set
            {
                if (!System.Drawing.ClientUtils.IsEnumValid(value, (int) value, 0, 4))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Drawing.Drawing2D.WrapMode));
                }
                this._SetWrapMode(value);
            }
        }
    }
}

