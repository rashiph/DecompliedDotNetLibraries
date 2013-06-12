namespace System.Drawing.Drawing2D
{
    using System;
    using System.Drawing;
    using System.Drawing.Internal;
    using System.Runtime.InteropServices;

    public sealed class Matrix : MarshalByRefObject, IDisposable
    {
        internal IntPtr nativeMatrix;

        public Matrix()
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateMatrix(out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            this.nativeMatrix = zero;
        }

        internal Matrix(IntPtr nativeMatrix)
        {
            this.SetNativeMatrix(nativeMatrix);
        }

        public Matrix(Rectangle rect, Point[] plgpts)
        {
            if (plgpts == null)
            {
                throw new ArgumentNullException("plgpts");
            }
            if (plgpts.Length != 3)
            {
                throw SafeNativeMethods.Gdip.StatusException(2);
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(plgpts);
            try
            {
                IntPtr zero = IntPtr.Zero;
                GPRECT gprect = new GPRECT(rect);
                int status = SafeNativeMethods.Gdip.GdipCreateMatrix3I(ref gprect, new HandleRef(null, handle), out zero);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                this.nativeMatrix = zero;
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public Matrix(RectangleF rect, PointF[] plgpts)
        {
            if (plgpts == null)
            {
                throw new ArgumentNullException("plgpts");
            }
            if (plgpts.Length != 3)
            {
                throw SafeNativeMethods.Gdip.StatusException(2);
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(plgpts);
            try
            {
                IntPtr zero = IntPtr.Zero;
                GPRECTF gprectf = new GPRECTF(rect);
                int status = SafeNativeMethods.Gdip.GdipCreateMatrix3(ref gprectf, new HandleRef(null, handle), out zero);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                this.nativeMatrix = zero;
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public Matrix(float m11, float m12, float m21, float m22, float dx, float dy)
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateMatrix2(m11, m12, m21, m22, dx, dy, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            this.nativeMatrix = zero;
        }

        public Matrix Clone()
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCloneMatrix(new HandleRef(this, this.nativeMatrix), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return new Matrix(zero);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.nativeMatrix != IntPtr.Zero)
            {
                SafeNativeMethods.Gdip.GdipDeleteMatrix(new HandleRef(this, this.nativeMatrix));
                this.nativeMatrix = IntPtr.Zero;
            }
        }

        public override bool Equals(object obj)
        {
            int num;
            Matrix wrapper = obj as Matrix;
            if (wrapper == null)
            {
                return false;
            }
            int status = SafeNativeMethods.Gdip.GdipIsMatrixEqual(new HandleRef(this, this.nativeMatrix), new HandleRef(wrapper, wrapper.nativeMatrix), out num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return (num != 0);
        }

        ~Matrix()
        {
            this.Dispose(false);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public void Invert()
        {
            int status = SafeNativeMethods.Gdip.GdipInvertMatrix(new HandleRef(this, this.nativeMatrix));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Multiply(Matrix matrix)
        {
            this.Multiply(matrix, MatrixOrder.Prepend);
        }

        public void Multiply(Matrix matrix, MatrixOrder order)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }
            int status = SafeNativeMethods.Gdip.GdipMultiplyMatrix(new HandleRef(this, this.nativeMatrix), new HandleRef(matrix, matrix.nativeMatrix), order);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Reset()
        {
            int status = SafeNativeMethods.Gdip.GdipSetMatrixElements(new HandleRef(this, this.nativeMatrix), 1f, 0f, 0f, 1f, 0f, 0f);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Rotate(float angle)
        {
            this.Rotate(angle, MatrixOrder.Prepend);
        }

        public void Rotate(float angle, MatrixOrder order)
        {
            int status = SafeNativeMethods.Gdip.GdipRotateMatrix(new HandleRef(this, this.nativeMatrix), angle, order);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void RotateAt(float angle, PointF point)
        {
            this.RotateAt(angle, point, MatrixOrder.Prepend);
        }

        public void RotateAt(float angle, PointF point, MatrixOrder order)
        {
            int num;
            if (order == MatrixOrder.Prepend)
            {
                num = SafeNativeMethods.Gdip.GdipTranslateMatrix(new HandleRef(this, this.nativeMatrix), point.X, point.Y, order) | SafeNativeMethods.Gdip.GdipRotateMatrix(new HandleRef(this, this.nativeMatrix), angle, order);
                num |= SafeNativeMethods.Gdip.GdipTranslateMatrix(new HandleRef(this, this.nativeMatrix), -point.X, -point.Y, order);
            }
            else
            {
                num = SafeNativeMethods.Gdip.GdipTranslateMatrix(new HandleRef(this, this.nativeMatrix), -point.X, -point.Y, order) | SafeNativeMethods.Gdip.GdipRotateMatrix(new HandleRef(this, this.nativeMatrix), angle, order);
                num |= SafeNativeMethods.Gdip.GdipTranslateMatrix(new HandleRef(this, this.nativeMatrix), point.X, point.Y, order);
            }
            if (num != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(num);
            }
        }

        public void Scale(float scaleX, float scaleY)
        {
            this.Scale(scaleX, scaleY, MatrixOrder.Prepend);
        }

        public void Scale(float scaleX, float scaleY, MatrixOrder order)
        {
            int status = SafeNativeMethods.Gdip.GdipScaleMatrix(new HandleRef(this, this.nativeMatrix), scaleX, scaleY, order);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        internal void SetNativeMatrix(IntPtr nativeMatrix)
        {
            this.nativeMatrix = nativeMatrix;
        }

        public void Shear(float shearX, float shearY)
        {
            int status = SafeNativeMethods.Gdip.GdipShearMatrix(new HandleRef(this, this.nativeMatrix), shearX, shearY, MatrixOrder.Prepend);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Shear(float shearX, float shearY, MatrixOrder order)
        {
            int status = SafeNativeMethods.Gdip.GdipShearMatrix(new HandleRef(this, this.nativeMatrix), shearX, shearY, order);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void TransformPoints(Point[] pts)
        {
            if (pts == null)
            {
                throw new ArgumentNullException("pts");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(pts);
            try
            {
                int status = SafeNativeMethods.Gdip.GdipTransformMatrixPointsI(new HandleRef(this, this.nativeMatrix), new HandleRef(null, handle), pts.Length);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                Point[] pointArray = SafeNativeMethods.Gdip.ConvertGPPOINTArray(handle, pts.Length);
                for (int i = 0; i < pts.Length; i++)
                {
                    pts[i] = pointArray[i];
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public void TransformPoints(PointF[] pts)
        {
            if (pts == null)
            {
                throw new ArgumentNullException("pts");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(pts);
            try
            {
                int status = SafeNativeMethods.Gdip.GdipTransformMatrixPoints(new HandleRef(this, this.nativeMatrix), new HandleRef(null, handle), pts.Length);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                PointF[] tfArray = SafeNativeMethods.Gdip.ConvertGPPOINTFArrayF(handle, pts.Length);
                for (int i = 0; i < pts.Length; i++)
                {
                    pts[i] = tfArray[i];
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public void TransformVectors(Point[] pts)
        {
            if (pts == null)
            {
                throw new ArgumentNullException("pts");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(pts);
            try
            {
                int status = SafeNativeMethods.Gdip.GdipVectorTransformMatrixPointsI(new HandleRef(this, this.nativeMatrix), new HandleRef(null, handle), pts.Length);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                Point[] pointArray = SafeNativeMethods.Gdip.ConvertGPPOINTArray(handle, pts.Length);
                for (int i = 0; i < pts.Length; i++)
                {
                    pts[i] = pointArray[i];
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public void TransformVectors(PointF[] pts)
        {
            if (pts == null)
            {
                throw new ArgumentNullException("pts");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(pts);
            try
            {
                int status = SafeNativeMethods.Gdip.GdipVectorTransformMatrixPoints(new HandleRef(this, this.nativeMatrix), new HandleRef(null, handle), pts.Length);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                PointF[] tfArray = SafeNativeMethods.Gdip.ConvertGPPOINTFArrayF(handle, pts.Length);
                for (int i = 0; i < pts.Length; i++)
                {
                    pts[i] = tfArray[i];
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public void Translate(float offsetX, float offsetY)
        {
            this.Translate(offsetX, offsetY, MatrixOrder.Prepend);
        }

        public void Translate(float offsetX, float offsetY, MatrixOrder order)
        {
            int status = SafeNativeMethods.Gdip.GdipTranslateMatrix(new HandleRef(this, this.nativeMatrix), offsetX, offsetY, order);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void VectorTransformPoints(Point[] pts)
        {
            this.TransformVectors(pts);
        }

        public float[] Elements
        {
            get
            {
                float[] numArray;
                IntPtr m = Marshal.AllocHGlobal(0x30);
                try
                {
                    int status = SafeNativeMethods.Gdip.GdipGetMatrixElements(new HandleRef(this, this.nativeMatrix), m);
                    if (status != 0)
                    {
                        throw SafeNativeMethods.Gdip.StatusException(status);
                    }
                    numArray = new float[6];
                    Marshal.Copy(m, numArray, 0, 6);
                }
                finally
                {
                    Marshal.FreeHGlobal(m);
                }
                return numArray;
            }
        }

        public bool IsIdentity
        {
            get
            {
                int num;
                int status = SafeNativeMethods.Gdip.GdipIsMatrixIdentity(new HandleRef(this, this.nativeMatrix), out num);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return (num != 0);
            }
        }

        public bool IsInvertible
        {
            get
            {
                int num;
                int status = SafeNativeMethods.Gdip.GdipIsMatrixInvertible(new HandleRef(this, this.nativeMatrix), out num);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return (num != 0);
            }
        }

        public float OffsetX
        {
            get
            {
                return this.Elements[4];
            }
        }

        public float OffsetY
        {
            get
            {
                return this.Elements[5];
            }
        }
    }
}

