namespace System.Drawing.Imaging
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public sealed class ImageAttributes : ICloneable, IDisposable
    {
        internal IntPtr nativeImageAttributes;
        internal void SetNativeImageAttributes(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                throw new ArgumentNullException("handle");
            }
            this.nativeImageAttributes = handle;
        }

        public ImageAttributes()
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateImageAttributes(out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            this.SetNativeImageAttributes(zero);
        }

        internal ImageAttributes(IntPtr newNativeImageAttributes)
        {
            this.SetNativeImageAttributes(newNativeImageAttributes);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.nativeImageAttributes != IntPtr.Zero)
            {
                try
                {
                    SafeNativeMethods.Gdip.GdipDisposeImageAttributes(new HandleRef(this, this.nativeImageAttributes));
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
                    this.nativeImageAttributes = IntPtr.Zero;
                }
            }
        }

        ~ImageAttributes()
        {
            this.Dispose(false);
        }

        public object Clone()
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCloneImageAttributes(new HandleRef(this, this.nativeImageAttributes), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return new ImageAttributes(zero);
        }

        public void SetColorMatrix(ColorMatrix newColorMatrix)
        {
            this.SetColorMatrix(newColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Default);
        }

        public void SetColorMatrix(ColorMatrix newColorMatrix, ColorMatrixFlag flags)
        {
            this.SetColorMatrix(newColorMatrix, flags, ColorAdjustType.Default);
        }

        public void SetColorMatrix(ColorMatrix newColorMatrix, ColorMatrixFlag mode, ColorAdjustType type)
        {
            int status = SafeNativeMethods.Gdip.GdipSetImageAttributesColorMatrix(new HandleRef(this, this.nativeImageAttributes), type, true, newColorMatrix, null, mode);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void ClearColorMatrix()
        {
            this.ClearColorMatrix(ColorAdjustType.Default);
        }

        public void ClearColorMatrix(ColorAdjustType type)
        {
            int status = SafeNativeMethods.Gdip.GdipSetImageAttributesColorMatrix(new HandleRef(this, this.nativeImageAttributes), type, false, null, null, ColorMatrixFlag.Default);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void SetColorMatrices(ColorMatrix newColorMatrix, ColorMatrix grayMatrix)
        {
            this.SetColorMatrices(newColorMatrix, grayMatrix, ColorMatrixFlag.Default, ColorAdjustType.Default);
        }

        public void SetColorMatrices(ColorMatrix newColorMatrix, ColorMatrix grayMatrix, ColorMatrixFlag flags)
        {
            this.SetColorMatrices(newColorMatrix, grayMatrix, flags, ColorAdjustType.Default);
        }

        public void SetColorMatrices(ColorMatrix newColorMatrix, ColorMatrix grayMatrix, ColorMatrixFlag mode, ColorAdjustType type)
        {
            int status = SafeNativeMethods.Gdip.GdipSetImageAttributesColorMatrix(new HandleRef(this, this.nativeImageAttributes), type, true, newColorMatrix, grayMatrix, mode);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void SetThreshold(float threshold)
        {
            this.SetThreshold(threshold, ColorAdjustType.Default);
        }

        public void SetThreshold(float threshold, ColorAdjustType type)
        {
            int status = SafeNativeMethods.Gdip.GdipSetImageAttributesThreshold(new HandleRef(this, this.nativeImageAttributes), type, true, threshold);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void ClearThreshold()
        {
            this.ClearThreshold(ColorAdjustType.Default);
        }

        public void ClearThreshold(ColorAdjustType type)
        {
            int status = SafeNativeMethods.Gdip.GdipSetImageAttributesThreshold(new HandleRef(this, this.nativeImageAttributes), type, false, 0f);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void SetGamma(float gamma)
        {
            this.SetGamma(gamma, ColorAdjustType.Default);
        }

        public void SetGamma(float gamma, ColorAdjustType type)
        {
            int status = SafeNativeMethods.Gdip.GdipSetImageAttributesGamma(new HandleRef(this, this.nativeImageAttributes), type, true, gamma);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void ClearGamma()
        {
            this.ClearGamma(ColorAdjustType.Default);
        }

        public void ClearGamma(ColorAdjustType type)
        {
            int status = SafeNativeMethods.Gdip.GdipSetImageAttributesGamma(new HandleRef(this, this.nativeImageAttributes), type, false, 0f);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void SetNoOp()
        {
            this.SetNoOp(ColorAdjustType.Default);
        }

        public void SetNoOp(ColorAdjustType type)
        {
            int status = SafeNativeMethods.Gdip.GdipSetImageAttributesNoOp(new HandleRef(this, this.nativeImageAttributes), type, true);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void ClearNoOp()
        {
            this.ClearNoOp(ColorAdjustType.Default);
        }

        public void ClearNoOp(ColorAdjustType type)
        {
            int status = SafeNativeMethods.Gdip.GdipSetImageAttributesNoOp(new HandleRef(this, this.nativeImageAttributes), type, false);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void SetColorKey(Color colorLow, Color colorHigh)
        {
            this.SetColorKey(colorLow, colorHigh, ColorAdjustType.Default);
        }

        public void SetColorKey(Color colorLow, Color colorHigh, ColorAdjustType type)
        {
            int num = colorLow.ToArgb();
            int num2 = colorHigh.ToArgb();
            int status = SafeNativeMethods.Gdip.GdipSetImageAttributesColorKeys(new HandleRef(this, this.nativeImageAttributes), type, true, num, num2);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void ClearColorKey()
        {
            this.ClearColorKey(ColorAdjustType.Default);
        }

        public void ClearColorKey(ColorAdjustType type)
        {
            int colorLow = 0;
            int status = SafeNativeMethods.Gdip.GdipSetImageAttributesColorKeys(new HandleRef(this, this.nativeImageAttributes), type, false, colorLow, colorLow);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void SetOutputChannel(ColorChannelFlag flags)
        {
            this.SetOutputChannel(flags, ColorAdjustType.Default);
        }

        public void SetOutputChannel(ColorChannelFlag flags, ColorAdjustType type)
        {
            int status = SafeNativeMethods.Gdip.GdipSetImageAttributesOutputChannel(new HandleRef(this, this.nativeImageAttributes), type, true, flags);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void ClearOutputChannel()
        {
            this.ClearOutputChannel(ColorAdjustType.Default);
        }

        public void ClearOutputChannel(ColorAdjustType type)
        {
            int status = SafeNativeMethods.Gdip.GdipSetImageAttributesOutputChannel(new HandleRef(this, this.nativeImageAttributes), type, false, ColorChannelFlag.ColorChannelLast);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void SetOutputChannelColorProfile(string colorProfileFilename)
        {
            this.SetOutputChannelColorProfile(colorProfileFilename, ColorAdjustType.Default);
        }

        public void SetOutputChannelColorProfile(string colorProfileFilename, ColorAdjustType type)
        {
            IntSecurity.DemandReadFileIO(colorProfileFilename);
            int status = SafeNativeMethods.Gdip.GdipSetImageAttributesOutputChannelColorProfile(new HandleRef(this, this.nativeImageAttributes), type, true, colorProfileFilename);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void ClearOutputChannelColorProfile()
        {
            this.ClearOutputChannel(ColorAdjustType.Default);
        }

        public void ClearOutputChannelColorProfile(ColorAdjustType type)
        {
            int status = SafeNativeMethods.Gdip.GdipSetImageAttributesOutputChannel(new HandleRef(this, this.nativeImageAttributes), type, false, ColorChannelFlag.ColorChannelLast);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void SetRemapTable(ColorMap[] map)
        {
            this.SetRemapTable(map, ColorAdjustType.Default);
        }

        public void SetRemapTable(ColorMap[] map, ColorAdjustType type)
        {
            int index = 0;
            int length = map.Length;
            int num3 = 4;
            IntPtr handle = Marshal.AllocHGlobal((int) ((length * num3) * 2));
            try
            {
                for (index = 0; index < length; index++)
                {
                    Marshal.StructureToPtr(map[index].OldColor.ToArgb(), (IntPtr) (((long) handle) + ((index * num3) * 2)), false);
                    Marshal.StructureToPtr(map[index].NewColor.ToArgb(), (IntPtr) ((((long) handle) + ((index * num3) * 2)) + num3), false);
                }
                int status = SafeNativeMethods.Gdip.GdipSetImageAttributesRemapTable(new HandleRef(this, this.nativeImageAttributes), type, true, length, new HandleRef(null, handle));
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public void ClearRemapTable()
        {
            this.ClearRemapTable(ColorAdjustType.Default);
        }

        public void ClearRemapTable(ColorAdjustType type)
        {
            int status = SafeNativeMethods.Gdip.GdipSetImageAttributesRemapTable(new HandleRef(this, this.nativeImageAttributes), type, false, 0, System.Drawing.NativeMethods.NullHandleRef);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void SetBrushRemapTable(ColorMap[] map)
        {
            this.SetRemapTable(map, ColorAdjustType.Brush);
        }

        public void ClearBrushRemapTable()
        {
            this.ClearRemapTable(ColorAdjustType.Brush);
        }

        public void SetWrapMode(WrapMode mode)
        {
            this.SetWrapMode(mode, new Color(), false);
        }

        public void SetWrapMode(WrapMode mode, Color color)
        {
            this.SetWrapMode(mode, color, false);
        }

        public void SetWrapMode(WrapMode mode, Color color, bool clamp)
        {
            int status = SafeNativeMethods.Gdip.GdipSetImageAttributesWrapMode(new HandleRef(this, this.nativeImageAttributes), (int) mode, color.ToArgb(), clamp);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void GetAdjustedPalette(ColorPalette palette, ColorAdjustType type)
        {
            IntPtr handle = palette.ConvertToMemory();
            try
            {
                int status = SafeNativeMethods.Gdip.GdipGetImageAttributesAdjustedPalette(new HandleRef(this, this.nativeImageAttributes), new HandleRef(null, handle), type);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                palette.ConvertFromMemory(handle);
            }
            finally
            {
                if (handle != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(handle);
                }
            }
        }
    }
}

