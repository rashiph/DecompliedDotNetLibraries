namespace System.Drawing
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Drawing2D;
    using System.Drawing.Internal;
    using System.Runtime;
    using System.Runtime.InteropServices;

    public sealed class Pen : MarshalByRefObject, ISystemColorTracker, ICloneable, IDisposable
    {
        private System.Drawing.Color color;
        private bool immutable;
        private IntPtr nativePen;

        public Pen(System.Drawing.Brush brush) : this(brush, 1f)
        {
        }

        public Pen(System.Drawing.Color color) : this(color, (float) 1f)
        {
        }

        private Pen(IntPtr nativePen)
        {
            this.SetNativePen(nativePen);
        }

        public Pen(System.Drawing.Brush brush, float width)
        {
            IntPtr zero = IntPtr.Zero;
            if (brush == null)
            {
                throw new ArgumentNullException("brush");
            }
            int status = SafeNativeMethods.Gdip.GdipCreatePen2(new HandleRef(brush, brush.NativeBrush), width, 0, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            this.SetNativePen(zero);
        }

        internal Pen(System.Drawing.Color color, bool immutable) : this(color)
        {
            this.immutable = immutable;
        }

        public Pen(System.Drawing.Color color, float width)
        {
            this.color = color;
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreatePen1(color.ToArgb(), width, 0, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            this.SetNativePen(zero);
            if (this.color.IsSystemColor)
            {
                SystemColorTracker.Add(this);
            }
        }

        public object Clone()
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipClonePen(new HandleRef(this, this.NativePen), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return new Pen(zero);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                this.immutable = false;
            }
            else if (this.immutable)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("CantChangeImmutableObjects", new object[] { "Brush" }));
            }
            if (this.nativePen != IntPtr.Zero)
            {
                try
                {
                    SafeNativeMethods.Gdip.GdipDeletePen(new HandleRef(this, this.NativePen));
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
                    this.nativePen = IntPtr.Zero;
                }
            }
        }

        private void EnsureValidDashPattern()
        {
            int dashcount = 0;
            int status = SafeNativeMethods.Gdip.GdipGetPenDashCount(new HandleRef(this, this.NativePen), out dashcount);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            if (dashcount == 0)
            {
                this.DashPattern = new float[] { 1f };
            }
        }

        ~Pen()
        {
            this.Dispose(false);
        }

        private IntPtr GetNativeBrush()
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipGetPenBrushFill(new HandleRef(this, this.NativePen), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return zero;
        }

        private void InternalSetColor(System.Drawing.Color value)
        {
            int status = SafeNativeMethods.Gdip.GdipSetPenColor(new HandleRef(this, this.NativePen), this.color.ToArgb());
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            this.color = value;
        }

        public void MultiplyTransform(Matrix matrix)
        {
            this.MultiplyTransform(matrix, MatrixOrder.Prepend);
        }

        public void MultiplyTransform(Matrix matrix, MatrixOrder order)
        {
            int status = SafeNativeMethods.Gdip.GdipMultiplyPenTransform(new HandleRef(this, this.NativePen), new HandleRef(matrix, matrix.nativeMatrix), order);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void ResetTransform()
        {
            int status = SafeNativeMethods.Gdip.GdipResetPenTransform(new HandleRef(this, this.NativePen));
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
            int status = SafeNativeMethods.Gdip.GdipRotatePenTransform(new HandleRef(this, this.NativePen), angle, order);
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
            int status = SafeNativeMethods.Gdip.GdipScalePenTransform(new HandleRef(this, this.NativePen), sx, sy, order);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void SetLineCap(LineCap startCap, LineCap endCap, System.Drawing.Drawing2D.DashCap dashCap)
        {
            if (this.immutable)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("CantChangeImmutableObjects", new object[] { "Pen" }));
            }
            int status = SafeNativeMethods.Gdip.GdipSetPenLineCap197819(new HandleRef(this, this.NativePen), (int) startCap, (int) endCap, (int) dashCap);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        internal void SetNativePen(IntPtr nativePen)
        {
            if (nativePen == IntPtr.Zero)
            {
                throw new ArgumentNullException("nativePen");
            }
            this.nativePen = nativePen;
        }

        void ISystemColorTracker.OnSystemColorChanged()
        {
            if (this.NativePen != IntPtr.Zero)
            {
                this.InternalSetColor(this.color);
            }
        }

        public void TranslateTransform(float dx, float dy)
        {
            this.TranslateTransform(dx, dy, MatrixOrder.Prepend);
        }

        public void TranslateTransform(float dx, float dy, MatrixOrder order)
        {
            int status = SafeNativeMethods.Gdip.GdipTranslatePenTransform(new HandleRef(this, this.NativePen), dx, dy, order);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public PenAlignment Alignment
        {
            get
            {
                PenAlignment center = PenAlignment.Center;
                int status = SafeNativeMethods.Gdip.GdipGetPenMode(new HandleRef(this, this.NativePen), out center);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return center;
            }
            set
            {
                if (!System.Drawing.ClientUtils.IsEnumValid(value, (int) value, 0, 4))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(PenAlignment));
                }
                if (this.immutable)
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("CantChangeImmutableObjects", new object[] { "Pen" }));
                }
                int status = SafeNativeMethods.Gdip.GdipSetPenMode(new HandleRef(this, this.NativePen), value);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public System.Drawing.Brush Brush
        {
            get
            {
                switch (this.PenType)
                {
                    case System.Drawing.Drawing2D.PenType.SolidColor:
                        return new SolidBrush(this.GetNativeBrush());

                    case System.Drawing.Drawing2D.PenType.HatchFill:
                        return new HatchBrush(this.GetNativeBrush());

                    case System.Drawing.Drawing2D.PenType.TextureFill:
                        return new TextureBrush(this.GetNativeBrush());

                    case System.Drawing.Drawing2D.PenType.PathGradient:
                        return new PathGradientBrush(this.GetNativeBrush());

                    case System.Drawing.Drawing2D.PenType.LinearGradient:
                        return new LinearGradientBrush(this.GetNativeBrush());
                }
                return null;
            }
            set
            {
                if (this.immutable)
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("CantChangeImmutableObjects", new object[] { "Pen" }));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                int status = SafeNativeMethods.Gdip.GdipSetPenBrushFill(new HandleRef(this, this.NativePen), new HandleRef(value, value.NativeBrush));
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public System.Drawing.Color Color
        {
            get
            {
                if (this.color == System.Drawing.Color.Empty)
                {
                    int argb = 0;
                    int status = SafeNativeMethods.Gdip.GdipGetPenColor(new HandleRef(this, this.NativePen), out argb);
                    if (status != 0)
                    {
                        throw SafeNativeMethods.Gdip.StatusException(status);
                    }
                    this.color = System.Drawing.Color.FromArgb(argb);
                }
                return this.color;
            }
            set
            {
                if (this.immutable)
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("CantChangeImmutableObjects", new object[] { "Pen" }));
                }
                if (value != this.color)
                {
                    System.Drawing.Color color = this.color;
                    this.color = value;
                    this.InternalSetColor(value);
                    if (value.IsSystemColor && !color.IsSystemColor)
                    {
                        SystemColorTracker.Add(this);
                    }
                }
            }
        }

        public float[] CompoundArray
        {
            get
            {
                int count = 0;
                int status = SafeNativeMethods.Gdip.GdipGetPenCompoundCount(new HandleRef(this, this.NativePen), out count);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                float[] array = new float[count];
                status = SafeNativeMethods.Gdip.GdipGetPenCompoundArray(new HandleRef(this, this.NativePen), array, count);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return array;
            }
            set
            {
                if (this.immutable)
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("CantChangeImmutableObjects", new object[] { "Pen" }));
                }
                int status = SafeNativeMethods.Gdip.GdipSetPenCompoundArray(new HandleRef(this, this.NativePen), value, value.Length);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public CustomLineCap CustomEndCap
        {
            get
            {
                IntPtr zero = IntPtr.Zero;
                int status = SafeNativeMethods.Gdip.GdipGetPenCustomEndCap(new HandleRef(this, this.NativePen), out zero);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return CustomLineCap.CreateCustomLineCapObject(zero);
            }
            set
            {
                if (this.immutable)
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("CantChangeImmutableObjects", new object[] { "Pen" }));
                }
                int status = SafeNativeMethods.Gdip.GdipSetPenCustomEndCap(new HandleRef(this, this.NativePen), new HandleRef(value, (value == null) ? IntPtr.Zero : ((IntPtr) value.nativeCap)));
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public CustomLineCap CustomStartCap
        {
            get
            {
                IntPtr zero = IntPtr.Zero;
                int status = SafeNativeMethods.Gdip.GdipGetPenCustomStartCap(new HandleRef(this, this.NativePen), out zero);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return CustomLineCap.CreateCustomLineCapObject(zero);
            }
            set
            {
                if (this.immutable)
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("CantChangeImmutableObjects", new object[] { "Pen" }));
                }
                int status = SafeNativeMethods.Gdip.GdipSetPenCustomStartCap(new HandleRef(this, this.NativePen), new HandleRef(value, (value == null) ? IntPtr.Zero : ((IntPtr) value.nativeCap)));
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public System.Drawing.Drawing2D.DashCap DashCap
        {
            get
            {
                int dashCap = 0;
                int status = SafeNativeMethods.Gdip.GdipGetPenDashCap197819(new HandleRef(this, this.NativePen), out dashCap);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return (System.Drawing.Drawing2D.DashCap) dashCap;
            }
            set
            {
                int[] enumValues = new int[3];
                enumValues[1] = 2;
                enumValues[2] = 3;
                if (!System.Drawing.ClientUtils.IsEnumValid_NotSequential(value, (int) value, enumValues))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Drawing.Drawing2D.DashCap));
                }
                if (this.immutable)
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("CantChangeImmutableObjects", new object[] { "Pen" }));
                }
                int status = SafeNativeMethods.Gdip.GdipSetPenDashCap197819(new HandleRef(this, this.NativePen), (int) value);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public float DashOffset
        {
            get
            {
                float[] dashoffset = new float[1];
                int status = SafeNativeMethods.Gdip.GdipGetPenDashOffset(new HandleRef(this, this.NativePen), dashoffset);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return dashoffset[0];
            }
            set
            {
                if (this.immutable)
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("CantChangeImmutableObjects", new object[] { "Pen" }));
                }
                int status = SafeNativeMethods.Gdip.GdipSetPenDashOffset(new HandleRef(this, this.NativePen), value);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public float[] DashPattern
        {
            get
            {
                float[] numArray;
                int dashcount = 0;
                int status = SafeNativeMethods.Gdip.GdipGetPenDashCount(new HandleRef(this, this.NativePen), out dashcount);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                int count = dashcount;
                IntPtr memorydash = Marshal.AllocHGlobal((int) (4 * count));
                status = SafeNativeMethods.Gdip.GdipGetPenDashArray(new HandleRef(this, this.NativePen), memorydash, count);
                try
                {
                    if (status != 0)
                    {
                        throw SafeNativeMethods.Gdip.StatusException(status);
                    }
                    numArray = new float[count];
                    Marshal.Copy(memorydash, numArray, 0, count);
                }
                finally
                {
                    Marshal.FreeHGlobal(memorydash);
                }
                return numArray;
            }
            set
            {
                if (this.immutable)
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("CantChangeImmutableObjects", new object[] { "Pen" }));
                }
                if ((value == null) || (value.Length == 0))
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("InvalidDashPattern"));
                }
                int length = value.Length;
                IntPtr destination = Marshal.AllocHGlobal((int) (4 * length));
                try
                {
                    Marshal.Copy(value, 0, destination, length);
                    int status = SafeNativeMethods.Gdip.GdipSetPenDashArray(new HandleRef(this, this.NativePen), new HandleRef(destination, destination), length);
                    if (status != 0)
                    {
                        throw SafeNativeMethods.Gdip.StatusException(status);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(destination);
                }
            }
        }

        public System.Drawing.Drawing2D.DashStyle DashStyle
        {
            get
            {
                int dashstyle = 0;
                int status = SafeNativeMethods.Gdip.GdipGetPenDashStyle(new HandleRef(this, this.NativePen), out dashstyle);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return (System.Drawing.Drawing2D.DashStyle) dashstyle;
            }
            set
            {
                if (!System.Drawing.ClientUtils.IsEnumValid(value, (int) value, 0, 5))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Drawing.Drawing2D.DashStyle));
                }
                if (this.immutable)
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("CantChangeImmutableObjects", new object[] { "Pen" }));
                }
                int status = SafeNativeMethods.Gdip.GdipSetPenDashStyle(new HandleRef(this, this.NativePen), (int) value);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                if (value == System.Drawing.Drawing2D.DashStyle.Custom)
                {
                    this.EnsureValidDashPattern();
                }
            }
        }

        public LineCap EndCap
        {
            get
            {
                int endCap = 0;
                int status = SafeNativeMethods.Gdip.GdipGetPenEndCap(new HandleRef(this, this.NativePen), out endCap);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return (LineCap) endCap;
            }
            set
            {
                switch (value)
                {
                    case LineCap.Flat:
                    case LineCap.Square:
                    case LineCap.Round:
                    case LineCap.Triangle:
                    case LineCap.NoAnchor:
                    case LineCap.SquareAnchor:
                    case LineCap.RoundAnchor:
                    case LineCap.DiamondAnchor:
                    case LineCap.ArrowAnchor:
                    case LineCap.AnchorMask:
                    case LineCap.Custom:
                    {
                        if (this.immutable)
                        {
                            throw new ArgumentException(System.Drawing.SR.GetString("CantChangeImmutableObjects", new object[] { "Pen" }));
                        }
                        int status = SafeNativeMethods.Gdip.GdipSetPenEndCap(new HandleRef(this, this.NativePen), (int) value);
                        if (status != 0)
                        {
                            throw SafeNativeMethods.Gdip.StatusException(status);
                        }
                        return;
                    }
                }
                throw new InvalidEnumArgumentException("value", (int) value, typeof(LineCap));
            }
        }

        public System.Drawing.Drawing2D.LineJoin LineJoin
        {
            get
            {
                int lineJoin = 0;
                int status = SafeNativeMethods.Gdip.GdipGetPenLineJoin(new HandleRef(this, this.NativePen), out lineJoin);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return (System.Drawing.Drawing2D.LineJoin) lineJoin;
            }
            set
            {
                if (!System.Drawing.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Drawing.Drawing2D.LineJoin));
                }
                if (this.immutable)
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("CantChangeImmutableObjects", new object[] { "Pen" }));
                }
                int status = SafeNativeMethods.Gdip.GdipSetPenLineJoin(new HandleRef(this, this.NativePen), (int) value);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public float MiterLimit
        {
            get
            {
                float[] miterLimit = new float[1];
                int status = SafeNativeMethods.Gdip.GdipGetPenMiterLimit(new HandleRef(this, this.NativePen), miterLimit);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return miterLimit[0];
            }
            set
            {
                if (this.immutable)
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("CantChangeImmutableObjects", new object[] { "Pen" }));
                }
                int status = SafeNativeMethods.Gdip.GdipSetPenMiterLimit(new HandleRef(this, this.NativePen), value);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        internal IntPtr NativePen
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.nativePen;
            }
        }

        public System.Drawing.Drawing2D.PenType PenType
        {
            get
            {
                int pentype = -1;
                int status = SafeNativeMethods.Gdip.GdipGetPenFillType(new HandleRef(this, this.NativePen), out pentype);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return (System.Drawing.Drawing2D.PenType) pentype;
            }
        }

        public LineCap StartCap
        {
            get
            {
                int startCap = 0;
                int status = SafeNativeMethods.Gdip.GdipGetPenStartCap(new HandleRef(this, this.NativePen), out startCap);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return (LineCap) startCap;
            }
            set
            {
                switch (value)
                {
                    case LineCap.Flat:
                    case LineCap.Square:
                    case LineCap.Round:
                    case LineCap.Triangle:
                    case LineCap.NoAnchor:
                    case LineCap.SquareAnchor:
                    case LineCap.RoundAnchor:
                    case LineCap.DiamondAnchor:
                    case LineCap.ArrowAnchor:
                    case LineCap.AnchorMask:
                    case LineCap.Custom:
                    {
                        if (this.immutable)
                        {
                            throw new ArgumentException(System.Drawing.SR.GetString("CantChangeImmutableObjects", new object[] { "Pen" }));
                        }
                        int status = SafeNativeMethods.Gdip.GdipSetPenStartCap(new HandleRef(this, this.NativePen), (int) value);
                        if (status != 0)
                        {
                            throw SafeNativeMethods.Gdip.StatusException(status);
                        }
                        return;
                    }
                }
                throw new InvalidEnumArgumentException("value", (int) value, typeof(LineCap));
            }
        }

        public Matrix Transform
        {
            get
            {
                Matrix wrapper = new Matrix();
                int status = SafeNativeMethods.Gdip.GdipGetPenTransform(new HandleRef(this, this.NativePen), new HandleRef(wrapper, wrapper.nativeMatrix));
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return wrapper;
            }
            set
            {
                if (this.immutable)
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("CantChangeImmutableObjects", new object[] { "Pen" }));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                int status = SafeNativeMethods.Gdip.GdipSetPenTransform(new HandleRef(this, this.NativePen), new HandleRef(value, value.nativeMatrix));
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public float Width
        {
            get
            {
                float[] width = new float[1];
                int status = SafeNativeMethods.Gdip.GdipGetPenWidth(new HandleRef(this, this.NativePen), width);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return width[0];
            }
            set
            {
                if (this.immutable)
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("CantChangeImmutableObjects", new object[] { "Pen" }));
                }
                int status = SafeNativeMethods.Gdip.GdipSetPenWidth(new HandleRef(this, this.NativePen), value);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }
    }
}

