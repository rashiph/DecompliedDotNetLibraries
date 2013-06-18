namespace System.Drawing
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Drawing.Imaging;
    using System.Drawing.Internal;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable, Editor("System.Drawing.Design.BitmapEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), ComVisible(true)]
    public sealed class Bitmap : Image
    {
        private static Color defaultTransparentColor = Color.LightGray;

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        private Bitmap()
        {
        }

        public Bitmap(Image original) : this(original, original.Width, original.Height)
        {
        }

        public Bitmap(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InvalidArgument", new object[] { "stream", "null" }));
            }
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateBitmapFromStream(new GPStream(stream), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            status = SafeNativeMethods.Gdip.GdipImageForceValidation(new HandleRef(null, zero));
            if (status != 0)
            {
                SafeNativeMethods.Gdip.GdipDisposeImage(new HandleRef(null, zero));
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeImage(zero);
            Image.EnsureSave(this, null, stream);
        }

        public Bitmap(string filename)
        {
            System.Drawing.IntSecurity.DemandReadFileIO(filename);
            filename = Path.GetFullPath(filename);
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateBitmapFromFile(filename, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            status = SafeNativeMethods.Gdip.GdipImageForceValidation(new HandleRef(null, zero));
            if (status != 0)
            {
                SafeNativeMethods.Gdip.GdipDisposeImage(new HandleRef(null, zero));
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeImage(zero);
            Image.EnsureSave(this, filename, null);
        }

        public Bitmap(Image original, Size newSize) : this(original, (newSize != 0) ? newSize.Width : 0, (newSize != 0) ? newSize.Height : 0)
        {
        }

        public Bitmap(int width, int height) : this(width, height, PixelFormat.Format32bppArgb)
        {
        }

        public Bitmap(Stream stream, bool useIcm)
        {
            int num;
            if (stream == null)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InvalidArgument", new object[] { "stream", "null" }));
            }
            IntPtr zero = IntPtr.Zero;
            if (useIcm)
            {
                num = SafeNativeMethods.Gdip.GdipCreateBitmapFromStreamICM(new GPStream(stream), out zero);
            }
            else
            {
                num = SafeNativeMethods.Gdip.GdipCreateBitmapFromStream(new GPStream(stream), out zero);
            }
            if (num != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(num);
            }
            num = SafeNativeMethods.Gdip.GdipImageForceValidation(new HandleRef(null, zero));
            if (num != 0)
            {
                SafeNativeMethods.Gdip.GdipDisposeImage(new HandleRef(null, zero));
                throw SafeNativeMethods.Gdip.StatusException(num);
            }
            base.SetNativeImage(zero);
            Image.EnsureSave(this, null, stream);
        }

        private Bitmap(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public Bitmap(string filename, bool useIcm)
        {
            int num;
            System.Drawing.IntSecurity.DemandReadFileIO(filename);
            filename = Path.GetFullPath(filename);
            IntPtr zero = IntPtr.Zero;
            if (useIcm)
            {
                num = SafeNativeMethods.Gdip.GdipCreateBitmapFromFileICM(filename, out zero);
            }
            else
            {
                num = SafeNativeMethods.Gdip.GdipCreateBitmapFromFile(filename, out zero);
            }
            if (num != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(num);
            }
            num = SafeNativeMethods.Gdip.GdipImageForceValidation(new HandleRef(null, zero));
            if (num != 0)
            {
                SafeNativeMethods.Gdip.GdipDisposeImage(new HandleRef(null, zero));
                throw SafeNativeMethods.Gdip.StatusException(num);
            }
            base.SetNativeImage(zero);
            Image.EnsureSave(this, filename, null);
        }

        public Bitmap(Type type, string resource)
        {
            Stream manifestResourceStream = type.Module.Assembly.GetManifestResourceStream(type, resource);
            if (manifestResourceStream == null)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("ResourceNotFound", new object[] { type, resource }));
            }
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateBitmapFromStream(new GPStream(manifestResourceStream), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            status = SafeNativeMethods.Gdip.GdipImageForceValidation(new HandleRef(null, zero));
            if (status != 0)
            {
                SafeNativeMethods.Gdip.GdipDisposeImage(new HandleRef(null, zero));
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeImage(zero);
            Image.EnsureSave(this, null, manifestResourceStream);
        }

        public Bitmap(Image original, int width, int height) : this(width, height)
        {
            using (Graphics graphics = null)
            {
                graphics = Graphics.FromImage(this);
                graphics.Clear(Color.Transparent);
                graphics.DrawImage(original, 0, 0, width, height);
            }
        }

        public Bitmap(int width, int height, Graphics g)
        {
            if (g == null)
            {
                throw new ArgumentNullException(System.Drawing.SR.GetString("InvalidArgument", new object[] { "g", "null" }));
            }
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateBitmapFromGraphics(width, height, new HandleRef(g, g.NativeGraphics), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeImage(zero);
        }

        public Bitmap(int width, int height, PixelFormat format)
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateBitmapFromScan0(width, height, 0, (int) format, System.Drawing.NativeMethods.NullHandleRef, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeImage(zero);
        }

        public Bitmap(int width, int height, int stride, PixelFormat format, IntPtr scan0)
        {
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateBitmapFromScan0(width, height, stride, (int) format, new HandleRef(null, scan0), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeImage(zero);
        }

        public Bitmap Clone(Rectangle rect, PixelFormat format)
        {
            if ((rect.Width == 0) || (rect.Height == 0))
            {
                throw new ArgumentException(System.Drawing.SR.GetString("GdiplusInvalidRectangle", new object[] { rect.ToString() }));
            }
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCloneBitmapAreaI(rect.X, rect.Y, rect.Width, rect.Height, (int) format, new HandleRef(this, base.nativeImage), out zero);
            if ((status != 0) || (zero == IntPtr.Zero))
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return FromGDIplus(zero);
        }

        public Bitmap Clone(RectangleF rect, PixelFormat format)
        {
            if ((rect.Width == 0f) || (rect.Height == 0f))
            {
                throw new ArgumentException(System.Drawing.SR.GetString("GdiplusInvalidRectangle", new object[] { rect.ToString() }));
            }
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCloneBitmapArea(rect.X, rect.Y, rect.Width, rect.Height, (int) format, new HandleRef(this, base.nativeImage), out zero);
            if ((status != 0) || (zero == IntPtr.Zero))
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return FromGDIplus(zero);
        }

        internal static Bitmap FromGDIplus(IntPtr handle)
        {
            Bitmap bitmap = new Bitmap();
            bitmap.SetNativeImage(handle);
            return bitmap;
        }

        public static Bitmap FromHicon(IntPtr hicon)
        {
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateBitmapFromHICON(new HandleRef(null, hicon), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return FromGDIplus(zero);
        }

        public static Bitmap FromResource(IntPtr hinstance, string bitmapName)
        {
            IntPtr ptr;
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            IntPtr handle = Marshal.StringToHGlobalUni(bitmapName);
            int status = SafeNativeMethods.Gdip.GdipCreateBitmapFromResource(new HandleRef(null, hinstance), new HandleRef(null, handle), out ptr);
            Marshal.FreeHGlobal(handle);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return FromGDIplus(ptr);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public IntPtr GetHbitmap()
        {
            return this.GetHbitmap(Color.LightGray);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public IntPtr GetHbitmap(Color background)
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateHBITMAPFromBitmap(new HandleRef(this, base.nativeImage), out zero, ColorTranslator.ToWin32(background));
            if ((status == 2) && ((base.Width >= 0x7fff) || (base.Height >= 0x7fff)))
            {
                throw new ArgumentException(System.Drawing.SR.GetString("GdiplusInvalidSize"));
            }
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return zero;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public IntPtr GetHicon()
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateHICONFromBitmap(new HandleRef(this, base.nativeImage), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return zero;
        }

        public Color GetPixel(int x, int y)
        {
            int argb = 0;
            if ((x < 0) || (x >= base.Width))
            {
                throw new ArgumentOutOfRangeException("x", System.Drawing.SR.GetString("ValidRangeX"));
            }
            if ((y < 0) || (y >= base.Height))
            {
                throw new ArgumentOutOfRangeException("y", System.Drawing.SR.GetString("ValidRangeY"));
            }
            int status = SafeNativeMethods.Gdip.GdipBitmapGetPixel(new HandleRef(this, base.nativeImage), x, y, out argb);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return Color.FromArgb(argb);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public BitmapData LockBits(Rectangle rect, ImageLockMode flags, PixelFormat format)
        {
            BitmapData bitmapData = new BitmapData();
            return this.LockBits(rect, flags, format, bitmapData);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public BitmapData LockBits(Rectangle rect, ImageLockMode flags, PixelFormat format, BitmapData bitmapData)
        {
            GPRECT gprect = new GPRECT(rect);
            int status = SafeNativeMethods.Gdip.GdipBitmapLockBits(new HandleRef(this, base.nativeImage), ref gprect, flags, format, bitmapData);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return bitmapData;
        }

        public void MakeTransparent()
        {
            Color defaultTransparentColor = Bitmap.defaultTransparentColor;
            if ((base.Height > 0) && (base.Width > 0))
            {
                defaultTransparentColor = this.GetPixel(0, base.Size.Height - 1);
            }
            if (defaultTransparentColor.A >= 0xff)
            {
                this.MakeTransparent(defaultTransparentColor);
            }
        }

        public void MakeTransparent(Color transparentColor)
        {
            if (base.RawFormat.Guid == ImageFormat.Icon.Guid)
            {
                throw new InvalidOperationException(System.Drawing.SR.GetString("CantMakeIconTransparent"));
            }
            Size size = base.Size;
            Bitmap image = null;
            Graphics graphics = null;
            try
            {
                image = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
                using (graphics = Graphics.FromImage(image))
                {
                    graphics.Clear(Color.Transparent);
                    Rectangle destRect = new Rectangle(0, 0, size.Width, size.Height);
                    using (ImageAttributes attributes = null)
                    {
                        attributes = new ImageAttributes();
                        attributes.SetColorKey(transparentColor, transparentColor);
                        graphics.DrawImage(this, destRect, 0, 0, size.Width, size.Height, GraphicsUnit.Pixel, attributes, null, IntPtr.Zero);
                    }
                }
                IntPtr nativeImage = base.nativeImage;
                base.nativeImage = image.nativeImage;
                image.nativeImage = nativeImage;
            }
            finally
            {
                if (image != null)
                {
                    image.Dispose();
                }
            }
        }

        public void SetPixel(int x, int y, Color color)
        {
            if ((base.PixelFormat & PixelFormat.Indexed) != PixelFormat.Undefined)
            {
                throw new InvalidOperationException(System.Drawing.SR.GetString("GdiplusCannotSetPixelFromIndexedPixelFormat"));
            }
            if ((x < 0) || (x >= base.Width))
            {
                throw new ArgumentOutOfRangeException("x", System.Drawing.SR.GetString("ValidRangeX"));
            }
            if ((y < 0) || (y >= base.Height))
            {
                throw new ArgumentOutOfRangeException("y", System.Drawing.SR.GetString("ValidRangeY"));
            }
            int status = SafeNativeMethods.Gdip.GdipBitmapSetPixel(new HandleRef(this, base.nativeImage), x, y, color.ToArgb());
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void SetResolution(float xDpi, float yDpi)
        {
            int status = SafeNativeMethods.Gdip.GdipBitmapSetResolution(new HandleRef(this, base.nativeImage), xDpi, yDpi);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public void UnlockBits(BitmapData bitmapdata)
        {
            int status = SafeNativeMethods.Gdip.GdipBitmapUnlockBits(new HandleRef(this, base.nativeImage), bitmapdata);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }
    }
}

