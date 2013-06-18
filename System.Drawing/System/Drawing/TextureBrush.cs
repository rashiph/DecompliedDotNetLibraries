namespace System.Drawing
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;

    public sealed class TextureBrush : Brush
    {
        public TextureBrush(System.Drawing.Image bitmap) : this(bitmap, System.Drawing.Drawing2D.WrapMode.Tile)
        {
        }

        internal TextureBrush(IntPtr nativeBrush)
        {
            base.SetNativeBrushInternal(nativeBrush);
        }

        public TextureBrush(System.Drawing.Image image, System.Drawing.Drawing2D.WrapMode wrapMode)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            if (!System.Drawing.ClientUtils.IsEnumValid(wrapMode, (int) wrapMode, 0, 4))
            {
                throw new InvalidEnumArgumentException("wrapMode", (int) wrapMode, typeof(System.Drawing.Drawing2D.WrapMode));
            }
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateTexture(new HandleRef(image, image.nativeImage), (int) wrapMode, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeBrushInternal(zero);
        }

        public TextureBrush(System.Drawing.Image image, Rectangle dstRect) : this(image, dstRect, null)
        {
        }

        public TextureBrush(System.Drawing.Image image, RectangleF dstRect) : this(image, dstRect, null)
        {
        }

        public TextureBrush(System.Drawing.Image image, System.Drawing.Drawing2D.WrapMode wrapMode, Rectangle dstRect)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            if (!System.Drawing.ClientUtils.IsEnumValid(wrapMode, (int) wrapMode, 0, 4))
            {
                throw new InvalidEnumArgumentException("wrapMode", (int) wrapMode, typeof(System.Drawing.Drawing2D.WrapMode));
            }
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateTexture2I(new HandleRef(image, image.nativeImage), (int) wrapMode, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeBrushInternal(zero);
        }

        public TextureBrush(System.Drawing.Image image, System.Drawing.Drawing2D.WrapMode wrapMode, RectangleF dstRect)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            if (!System.Drawing.ClientUtils.IsEnumValid(wrapMode, (int) wrapMode, 0, 4))
            {
                throw new InvalidEnumArgumentException("wrapMode", (int) wrapMode, typeof(System.Drawing.Drawing2D.WrapMode));
            }
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateTexture2(new HandleRef(image, image.nativeImage), (int) wrapMode, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeBrushInternal(zero);
        }

        public TextureBrush(System.Drawing.Image image, Rectangle dstRect, ImageAttributes imageAttr)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateTextureIAI(new HandleRef(image, image.nativeImage), new HandleRef(imageAttr, (imageAttr == null) ? IntPtr.Zero : imageAttr.nativeImageAttributes), dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeBrushInternal(zero);
        }

        public TextureBrush(System.Drawing.Image image, RectangleF dstRect, ImageAttributes imageAttr)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateTextureIA(new HandleRef(image, image.nativeImage), new HandleRef(imageAttr, (imageAttr == null) ? IntPtr.Zero : imageAttr.nativeImageAttributes), dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeBrushInternal(zero);
        }

        private Matrix _GetTransform()
        {
            Matrix wrapper = new Matrix();
            int status = SafeNativeMethods.Gdip.GdipGetTextureTransform(new HandleRef(this, base.NativeBrush), new HandleRef(wrapper, wrapper.nativeMatrix));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return wrapper;
        }

        private System.Drawing.Drawing2D.WrapMode _GetWrapMode()
        {
            int wrapMode = 0;
            int status = SafeNativeMethods.Gdip.GdipGetTextureWrapMode(new HandleRef(this, base.NativeBrush), out wrapMode);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return (System.Drawing.Drawing2D.WrapMode) wrapMode;
        }

        private void _SetTransform(Matrix matrix)
        {
            int status = SafeNativeMethods.Gdip.GdipSetTextureTransform(new HandleRef(this, base.NativeBrush), new HandleRef(matrix, matrix.nativeMatrix));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        private void _SetWrapMode(System.Drawing.Drawing2D.WrapMode wrapMode)
        {
            int status = SafeNativeMethods.Gdip.GdipSetTextureWrapMode(new HandleRef(this, base.NativeBrush), (int) wrapMode);
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
            return new TextureBrush(zero);
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
            int status = SafeNativeMethods.Gdip.GdipMultiplyTextureTransform(new HandleRef(this, base.NativeBrush), new HandleRef(matrix, matrix.nativeMatrix), order);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void ResetTransform()
        {
            int status = SafeNativeMethods.Gdip.GdipResetTextureTransform(new HandleRef(this, base.NativeBrush));
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
            int status = SafeNativeMethods.Gdip.GdipRotateTextureTransform(new HandleRef(this, base.NativeBrush), angle, order);
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
            int status = SafeNativeMethods.Gdip.GdipScaleTextureTransform(new HandleRef(this, base.NativeBrush), sx, sy, order);
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
            int status = SafeNativeMethods.Gdip.GdipTranslateTextureTransform(new HandleRef(this, base.NativeBrush), dx, dy, order);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                IntPtr ptr;
                int status = SafeNativeMethods.Gdip.GdipGetTextureImage(new HandleRef(this, base.NativeBrush), out ptr);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return System.Drawing.Image.CreateImageObject(ptr);
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
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
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

