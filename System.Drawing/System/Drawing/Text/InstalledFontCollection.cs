namespace System.Drawing.Text
{
    using System;
    using System.Drawing;

    public sealed class InstalledFontCollection : FontCollection
    {
        public InstalledFontCollection()
        {
            base.nativeFontCollection = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipNewInstalledFontCollection(out this.nativeFontCollection);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }
    }
}

