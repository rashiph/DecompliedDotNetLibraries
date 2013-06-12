namespace System.Drawing.Text
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    public sealed class PrivateFontCollection : FontCollection
    {
        public PrivateFontCollection()
        {
            base.nativeFontCollection = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipNewPrivateFontCollection(out this.nativeFontCollection);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void AddFontFile(string filename)
        {
            IntSecurity.DemandReadFileIO(filename);
            int status = SafeNativeMethods.Gdip.GdipPrivateAddFontFile(new HandleRef(this, base.nativeFontCollection), filename);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            SafeNativeMethods.AddFontFile(filename);
        }

        public void AddMemoryFont(IntPtr memory, int length)
        {
            IntSecurity.ObjectFromWin32Handle.Demand();
            int status = SafeNativeMethods.Gdip.GdipPrivateAddMemoryFont(new HandleRef(this, base.nativeFontCollection), new HandleRef(null, memory), length);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (base.nativeFontCollection != IntPtr.Zero)
            {
                try
                {
                    SafeNativeMethods.Gdip.GdipDeletePrivateFontCollection(out this.nativeFontCollection);
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
                    base.nativeFontCollection = IntPtr.Zero;
                }
            }
            base.Dispose(disposing);
        }
    }
}

