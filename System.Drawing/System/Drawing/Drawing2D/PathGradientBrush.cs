namespace System.Drawing.Drawing2D
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Internal;
    using System.Runtime.InteropServices;

    public sealed class PathGradientBrush : Brush
    {
        public PathGradientBrush(Point[] points) : this(points, System.Drawing.Drawing2D.WrapMode.Clamp)
        {
        }

        public PathGradientBrush(PointF[] points) : this(points, System.Drawing.Drawing2D.WrapMode.Clamp)
        {
        }

        public PathGradientBrush(GraphicsPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreatePathGradientFromPath(new HandleRef(path, path.nativePath), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeBrushInternal(zero);
        }

        internal PathGradientBrush(IntPtr nativeBrush)
        {
            base.SetNativeBrushInternal(nativeBrush);
        }

        public PathGradientBrush(Point[] points, System.Drawing.Drawing2D.WrapMode wrapMode)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            if (!System.Drawing.ClientUtils.IsEnumValid(wrapMode, (int) wrapMode, 0, 4))
            {
                throw new InvalidEnumArgumentException("wrapMode", (int) wrapMode, typeof(System.Drawing.Drawing2D.WrapMode));
            }
            IntPtr zero = IntPtr.Zero;
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            try
            {
                int status = SafeNativeMethods.Gdip.GdipCreatePathGradientI(new HandleRef(null, handle), points.Length, (int) wrapMode, out zero);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                base.SetNativeBrushInternal(zero);
            }
            finally
            {
                if (handle != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(handle);
                }
            }
        }

        public PathGradientBrush(PointF[] points, System.Drawing.Drawing2D.WrapMode wrapMode)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            if (!System.Drawing.ClientUtils.IsEnumValid(wrapMode, (int) wrapMode, 0, 4))
            {
                throw new InvalidEnumArgumentException("wrapMode", (int) wrapMode, typeof(System.Drawing.Drawing2D.WrapMode));
            }
            IntPtr zero = IntPtr.Zero;
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            try
            {
                int status = SafeNativeMethods.Gdip.GdipCreatePathGradient(new HandleRef(null, handle), points.Length, (int) wrapMode, out zero);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                base.SetNativeBrushInternal(zero);
            }
            finally
            {
                if (handle != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(handle);
                }
            }
        }

        private System.Drawing.Drawing2D.Blend _GetBlend()
        {
            System.Drawing.Drawing2D.Blend blend;
            int count = 0;
            int status = SafeNativeMethods.Gdip.GdipGetPathGradientBlendCount(new HandleRef(this, base.NativeBrush), out count);
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
                status = SafeNativeMethods.Gdip.GdipGetPathGradientBlend(new HandleRef(this, base.NativeBrush), zero, positions, num3);
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
            int count = 0;
            int status = SafeNativeMethods.Gdip.GdipGetPathGradientPresetBlendCount(new HandleRef(this, base.NativeBrush), out count);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            if (count == 0)
            {
                return new ColorBlend();
            }
            int num3 = count;
            IntPtr zero = IntPtr.Zero;
            IntPtr positions = IntPtr.Zero;
            try
            {
                int cb = 4 * num3;
                zero = Marshal.AllocHGlobal(cb);
                positions = Marshal.AllocHGlobal(cb);
                status = SafeNativeMethods.Gdip.GdipGetPathGradientPresetBlend(new HandleRef(this, base.NativeBrush), zero, positions, num3);
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

        private RectangleF _GetRectangle()
        {
            GPRECTF gprectf = new GPRECTF();
            int status = SafeNativeMethods.Gdip.GdipGetPathGradientRect(new HandleRef(this, base.NativeBrush), ref gprectf);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return gprectf.ToRectangleF();
        }

        private Color[] _GetSurroundColors()
        {
            int num;
            int status = SafeNativeMethods.Gdip.GdipGetPathGradientSurroundColorCount(new HandleRef(this, base.NativeBrush), out num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            int[] color = new int[num];
            status = SafeNativeMethods.Gdip.GdipGetPathGradientSurroundColorsWithCount(new HandleRef(this, base.NativeBrush), color, ref num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            Color[] colorArray = new Color[num];
            for (int i = 0; i < num; i++)
            {
                colorArray[i] = Color.FromArgb(color[i]);
            }
            return colorArray;
        }

        private Matrix _GetTransform()
        {
            Matrix wrapper = new Matrix();
            int status = SafeNativeMethods.Gdip.GdipGetPathGradientTransform(new HandleRef(this, base.NativeBrush), new HandleRef(wrapper, wrapper.nativeMatrix));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return wrapper;
        }

        private System.Drawing.Drawing2D.WrapMode _GetWrapMode()
        {
            int wrapmode = 0;
            int status = SafeNativeMethods.Gdip.GdipGetPathGradientWrapMode(new HandleRef(this, base.NativeBrush), out wrapmode);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return (System.Drawing.Drawing2D.WrapMode) wrapmode;
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
                int status = SafeNativeMethods.Gdip.GdipSetPathGradientBlend(new HandleRef(this, base.NativeBrush), new HandleRef(null, zero), new HandleRef(null, destination), length);
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
                int status = SafeNativeMethods.Gdip.GdipSetPathGradientPresetBlend(new HandleRef(this, base.NativeBrush), new HandleRef(null, zero), new HandleRef(null, destination), length);
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

        private void _SetSurroundColors(Color[] colors)
        {
            int length;
            int status = SafeNativeMethods.Gdip.GdipGetPathGradientSurroundColorCount(new HandleRef(this, base.NativeBrush), out length);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            if ((colors.Length > length) || (length <= 0))
            {
                throw SafeNativeMethods.Gdip.StatusException(2);
            }
            length = colors.Length;
            int[] argb = new int[length];
            for (int i = 0; i < colors.Length; i++)
            {
                argb[i] = colors[i].ToArgb();
            }
            status = SafeNativeMethods.Gdip.GdipSetPathGradientSurroundColorsWithCount(new HandleRef(this, base.NativeBrush), argb, ref length);
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
            int status = SafeNativeMethods.Gdip.GdipSetPathGradientTransform(new HandleRef(this, base.NativeBrush), new HandleRef(matrix, matrix.nativeMatrix));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        private void _SetWrapMode(System.Drawing.Drawing2D.WrapMode wrapMode)
        {
            int status = SafeNativeMethods.Gdip.GdipSetPathGradientWrapMode(new HandleRef(this, base.NativeBrush), (int) wrapMode);
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
            return new PathGradientBrush(zero);
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
            int status = SafeNativeMethods.Gdip.GdipMultiplyPathGradientTransform(new HandleRef(this, base.NativeBrush), new HandleRef(matrix, matrix.nativeMatrix), order);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void ResetTransform()
        {
            int status = SafeNativeMethods.Gdip.GdipResetPathGradientTransform(new HandleRef(this, base.NativeBrush));
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
            int status = SafeNativeMethods.Gdip.GdipRotatePathGradientTransform(new HandleRef(this, base.NativeBrush), angle, order);
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
            int status = SafeNativeMethods.Gdip.GdipScalePathGradientTransform(new HandleRef(this, base.NativeBrush), sx, sy, order);
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
            int status = SafeNativeMethods.Gdip.GdipSetPathGradientLinearBlend(new HandleRef(this, base.NativeBrush), focus, scale);
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
            int status = SafeNativeMethods.Gdip.GdipSetPathGradientSigmaBlend(new HandleRef(this, base.NativeBrush), focus, scale);
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
            int status = SafeNativeMethods.Gdip.GdipTranslatePathGradientTransform(new HandleRef(this, base.NativeBrush), dx, dy, order);
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

        public Color CenterColor
        {
            get
            {
                int num;
                int status = SafeNativeMethods.Gdip.GdipGetPathGradientCenterColor(new HandleRef(this, base.NativeBrush), out num);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return Color.FromArgb(num);
            }
            set
            {
                int status = SafeNativeMethods.Gdip.GdipSetPathGradientCenterColor(new HandleRef(this, base.NativeBrush), value.ToArgb());
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public PointF CenterPoint
        {
            get
            {
                GPPOINTF point = new GPPOINTF();
                int status = SafeNativeMethods.Gdip.GdipGetPathGradientCenterPoint(new HandleRef(this, base.NativeBrush), point);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return point.ToPoint();
            }
            set
            {
                int status = SafeNativeMethods.Gdip.GdipSetPathGradientCenterPoint(new HandleRef(this, base.NativeBrush), new GPPOINTF(value));
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public PointF FocusScales
        {
            get
            {
                float[] xScale = new float[1];
                float[] yScale = new float[1];
                int status = SafeNativeMethods.Gdip.GdipGetPathGradientFocusScales(new HandleRef(this, base.NativeBrush), xScale, yScale);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return new PointF(xScale[0], yScale[0]);
            }
            set
            {
                int status = SafeNativeMethods.Gdip.GdipSetPathGradientFocusScales(new HandleRef(this, base.NativeBrush), value.X, value.Y);
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

        public RectangleF Rectangle
        {
            get
            {
                return this._GetRectangle();
            }
        }

        public Color[] SurroundColors
        {
            get
            {
                return this._GetSurroundColors();
            }
            set
            {
                this._SetSurroundColors(value);
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

