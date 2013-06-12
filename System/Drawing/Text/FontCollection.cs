namespace System.Drawing.Text
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    public abstract class FontCollection : IDisposable
    {
        internal IntPtr nativeFontCollection = IntPtr.Zero;

        internal FontCollection()
        {
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        ~FontCollection()
        {
            this.Dispose(false);
        }

        public FontFamily[] Families
        {
            get
            {
                int numFound = 0;
                int status = SafeNativeMethods.Gdip.GdipGetFontCollectionFamilyCount(new HandleRef(this, this.nativeFontCollection), out numFound);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                IntPtr[] gpfamilies = new IntPtr[numFound];
                int num3 = 0;
                status = SafeNativeMethods.Gdip.GdipGetFontCollectionFamilyList(new HandleRef(this, this.nativeFontCollection), numFound, gpfamilies, out num3);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                FontFamily[] familyArray = new FontFamily[num3];
                for (int i = 0; i < num3; i++)
                {
                    IntPtr ptr;
                    SafeNativeMethods.Gdip.GdipCloneFontFamily(new HandleRef(null, gpfamilies[i]), out ptr);
                    familyArray[i] = new FontFamily(ptr);
                }
                return familyArray;
            }
        }
    }
}

