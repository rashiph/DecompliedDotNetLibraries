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
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    [Serializable, Editor("System.Drawing.Design.IconEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), TypeConverter(typeof(IconConverter))]
    public sealed class Icon : MarshalByRefObject, ISerializable, ICloneable, IDisposable
    {
        private int bestBitDepth;
        private int bestImageOffset;
        private static int bitDepth;
        private IntPtr handle;
        private byte[] iconData;
        private System.Drawing.Size iconSize;
        private bool ownHandle;

        private Icon()
        {
            this.iconSize = System.Drawing.Size.Empty;
            this.handle = IntPtr.Zero;
            this.ownHandle = true;
        }

        internal Icon(IntPtr handle) : this(handle, false)
        {
        }

        public Icon(Stream stream) : this(stream, 0, 0)
        {
        }

        public Icon(string fileName) : this(fileName, 0, 0)
        {
        }

        public Icon(Icon original, System.Drawing.Size size) : this(original, size.Width, size.Height)
        {
        }

        internal Icon(IntPtr handle, bool takeOwnership)
        {
            this.iconSize = System.Drawing.Size.Empty;
            this.handle = IntPtr.Zero;
            this.ownHandle = true;
            if (handle == IntPtr.Zero)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InvalidGDIHandle", new object[] { typeof(Icon).Name }));
            }
            this.handle = handle;
            this.ownHandle = takeOwnership;
        }

        public Icon(Stream stream, System.Drawing.Size size) : this(stream, size.Width, size.Height)
        {
        }

        private Icon(SerializationInfo info, StreamingContext context)
        {
            this.iconSize = System.Drawing.Size.Empty;
            this.handle = IntPtr.Zero;
            this.ownHandle = true;
            this.iconData = (byte[]) info.GetValue("IconData", typeof(byte[]));
            this.iconSize = (System.Drawing.Size) info.GetValue("IconSize", typeof(System.Drawing.Size));
            if (this.iconSize.IsEmpty)
            {
                this.Initialize(0, 0);
            }
            else
            {
                this.Initialize(this.iconSize.Width, this.iconSize.Height);
            }
        }

        public Icon(string fileName, System.Drawing.Size size) : this(fileName, size.Width, size.Height)
        {
        }

        public Icon(Type type, string resource) : this()
        {
            Stream manifestResourceStream = type.Module.Assembly.GetManifestResourceStream(type, resource);
            if (manifestResourceStream == null)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("ResourceNotFound", new object[] { type, resource }));
            }
            this.iconData = new byte[(int) manifestResourceStream.Length];
            manifestResourceStream.Read(this.iconData, 0, this.iconData.Length);
            this.Initialize(0, 0);
        }

        public Icon(Icon original, int width, int height) : this()
        {
            if (original == null)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InvalidArgument", new object[] { "original", "null" }));
            }
            this.iconData = original.iconData;
            if (this.iconData == null)
            {
                this.iconSize = original.Size;
                this.handle = SafeNativeMethods.CopyImage(new HandleRef(original, original.Handle), 1, this.iconSize.Width, this.iconSize.Height, 0);
            }
            else
            {
                this.Initialize(width, height);
            }
        }

        public Icon(Stream stream, int width, int height) : this()
        {
            if (stream == null)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InvalidArgument", new object[] { "stream", "null" }));
            }
            this.iconData = new byte[(int) stream.Length];
            stream.Read(this.iconData, 0, this.iconData.Length);
            this.Initialize(width, height);
        }

        public Icon(string fileName, int width, int height) : this()
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                this.iconData = new byte[(int) stream.Length];
                stream.Read(this.iconData, 0, this.iconData.Length);
            }
            this.Initialize(width, height);
        }

        private static unsafe bool BitmapHasAlpha(BitmapData bmpData)
        {
            for (int i = 0; i < bmpData.Height; i++)
            {
                for (int j = 3; j < Math.Abs(bmpData.Stride); j += 4)
                {
                    byte* numPtr = (byte*) ((bmpData.Scan0.ToPointer() + (i * bmpData.Stride)) + j);
                    if (numPtr[0] != 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public object Clone()
        {
            return new Icon(this, this.Size.Width, this.Size.Height);
        }

        private void CopyBitmapData(BitmapData sourceData, BitmapData targetData)
        {
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < Math.Min(sourceData.Height, targetData.Height); i++)
            {
                IntPtr ptr;
                IntPtr ptr2;
                if (IntPtr.Size == 4)
                {
                    ptr = new IntPtr(sourceData.Scan0.ToInt32() + num);
                    ptr2 = new IntPtr(targetData.Scan0.ToInt32() + num2);
                }
                else
                {
                    ptr = new IntPtr(sourceData.Scan0.ToInt64() + num);
                    ptr2 = new IntPtr(targetData.Scan0.ToInt64() + num2);
                }
                System.Drawing.UnsafeNativeMethods.CopyMemory(new HandleRef(this, ptr2), new HandleRef(this, ptr), Math.Abs(targetData.Stride));
                num += sourceData.Stride;
                num2 += targetData.Stride;
            }
        }

        internal void DestroyHandle()
        {
            if (this.ownHandle)
            {
                SafeNativeMethods.DestroyIcon(new HandleRef(this, this.handle));
                this.handle = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.handle != IntPtr.Zero)
            {
                this.DestroyHandle();
            }
        }

        internal void Draw(Graphics graphics, Rectangle targetRect)
        {
            Rectangle rectangle = targetRect;
            rectangle.X += (int) graphics.Transform.OffsetX;
            rectangle.Y += (int) graphics.Transform.OffsetY;
            WindowsGraphics graphics2 = WindowsGraphics.FromGraphics(graphics, ApplyGraphicsProperties.Clipping);
            IntPtr hdc = graphics2.GetHdc();
            try
            {
                this.DrawIcon(hdc, Rectangle.Empty, rectangle, true);
            }
            finally
            {
                graphics2.Dispose();
            }
        }

        internal void Draw(Graphics graphics, int x, int y)
        {
            System.Drawing.Size size = this.Size;
            this.Draw(graphics, new Rectangle(x, y, size.Width, size.Height));
        }

        private void DrawIcon(IntPtr dc, Rectangle imageRect, Rectangle targetRect, bool stretch)
        {
            int width;
            int height;
            int num9;
            int num10;
            int num11;
            int num12;
            int x = 0;
            int y = 0;
            int num5 = 0;
            int num6 = 0;
            int num7 = 0;
            int num8 = 0;
            System.Drawing.Size size = this.Size;
            if (!imageRect.IsEmpty)
            {
                x = imageRect.X;
                y = imageRect.Y;
                width = imageRect.Width;
                height = imageRect.Height;
            }
            else
            {
                width = size.Width;
                height = size.Height;
            }
            if (!targetRect.IsEmpty)
            {
                num5 = targetRect.X;
                num6 = targetRect.Y;
                num7 = targetRect.Width;
                num8 = targetRect.Height;
            }
            else
            {
                num7 = size.Width;
                num8 = size.Height;
            }
            if (stretch)
            {
                num9 = (size.Width * num7) / width;
                num10 = (size.Height * num8) / height;
                num11 = num7;
                num12 = num8;
            }
            else
            {
                num9 = size.Width;
                num10 = size.Height;
                num11 = (num7 < width) ? num7 : width;
                num12 = (num8 < height) ? num8 : height;
            }
            IntPtr hRgn = SafeNativeMethods.SaveClipRgn(dc);
            try
            {
                SafeNativeMethods.IntersectClipRect(new HandleRef(this, dc), num5, num6, num5 + num11, num6 + num12);
                SafeNativeMethods.DrawIconEx(new HandleRef(null, dc), num5 - x, num6 - y, new HandleRef(this, this.handle), num9, num10, 0, System.Drawing.NativeMethods.NullHandleRef, 3);
            }
            finally
            {
                SafeNativeMethods.RestoreClipRgn(dc, hRgn);
            }
        }

        internal void DrawUnstretched(Graphics graphics, Rectangle targetRect)
        {
            Rectangle rectangle = targetRect;
            rectangle.X += (int) graphics.Transform.OffsetX;
            rectangle.Y += (int) graphics.Transform.OffsetY;
            WindowsGraphics graphics2 = WindowsGraphics.FromGraphics(graphics, ApplyGraphicsProperties.Clipping);
            IntPtr hdc = graphics2.GetHdc();
            try
            {
                this.DrawIcon(hdc, Rectangle.Empty, rectangle, false);
            }
            finally
            {
                graphics2.Dispose();
            }
        }

        public static Icon ExtractAssociatedIcon(string filePath)
        {
            return ExtractAssociatedIcon(filePath, 0);
        }

        private static Icon ExtractAssociatedIcon(string filePath, int index)
        {
            Uri uri;
            if (filePath == null)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InvalidArgument", new object[] { "filePath", "null" }));
            }
            try
            {
                uri = new Uri(filePath);
            }
            catch (UriFormatException)
            {
                filePath = Path.GetFullPath(filePath);
                uri = new Uri(filePath);
            }
            if (uri.IsUnc)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InvalidArgument", new object[] { "filePath", filePath }));
            }
            if (uri.IsFile)
            {
                if (!File.Exists(filePath))
                {
                    System.Drawing.IntSecurity.DemandReadFileIO(filePath);
                    throw new FileNotFoundException(filePath);
                }
                Icon icon = new Icon();
                StringBuilder iconPath = new StringBuilder(260);
                iconPath.Append(filePath);
                IntPtr handle = SafeNativeMethods.ExtractAssociatedIcon(System.Drawing.NativeMethods.NullHandleRef, iconPath, ref index);
                if (handle != IntPtr.Zero)
                {
                    System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
                    return new Icon(handle, true);
                }
            }
            return null;
        }

        ~Icon()
        {
            this.Dispose(false);
        }

        public static Icon FromHandle(IntPtr handle)
        {
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            return new Icon(handle);
        }

        private unsafe int GetInt(byte* pb)
        {
            int num = 0;
            if ((((byte) pb) & 3) != 0)
            {
                num = pb[0];
                pb++;
                num |= pb[0] << 8;
                pb++;
                num |= pb[0] << 0x10;
                pb++;
                return (num | (pb[0] << 0x18));
            }
            return *(((int*) pb));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        private unsafe short GetShort(byte* pb)
        {
            int num = 0;
            if ((((byte) pb) & 1) != 0)
            {
                num = pb[0];
                pb++;
                num |= pb[0] << 8;
            }
            else
            {
                num = *((short*) pb);
            }
            return (short) num;
        }

        private unsafe void Initialize(int width, int height)
        {
            if ((this.iconData == null) || (this.handle != IntPtr.Zero))
            {
                throw new InvalidOperationException(System.Drawing.SR.GetString("IllegalState", new object[] { base.GetType().Name }));
            }
            if (this.iconData.Length < Marshal.SizeOf(typeof(SafeNativeMethods.ICONDIR)))
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InvalidPictureType", new object[] { "picture", "Icon" }));
            }
            if (width == 0)
            {
                width = System.Drawing.UnsafeNativeMethods.GetSystemMetrics(11);
            }
            if (height == 0)
            {
                height = System.Drawing.UnsafeNativeMethods.GetSystemMetrics(12);
            }
            if (bitDepth == 0)
            {
                IntPtr dC = System.Drawing.UnsafeNativeMethods.GetDC(System.Drawing.NativeMethods.NullHandleRef);
                bitDepth = System.Drawing.UnsafeNativeMethods.GetDeviceCaps(new HandleRef(null, dC), 12);
                bitDepth *= System.Drawing.UnsafeNativeMethods.GetDeviceCaps(new HandleRef(null, dC), 14);
                System.Drawing.UnsafeNativeMethods.ReleaseDC(System.Drawing.NativeMethods.NullHandleRef, new HandleRef(null, dC));
                if (bitDepth == 8)
                {
                    bitDepth = 4;
                }
            }
            fixed (byte* numRef = this.iconData)
            {
                short @short = this.GetShort(numRef);
                short num2 = this.GetShort(numRef + 2);
                short num3 = this.GetShort(numRef + 4);
                if (((@short != 0) || (num2 != 1)) || (num3 == 0))
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("InvalidPictureType", new object[] { "picture", "Icon" }));
                }
                byte bWidth = 0;
                byte bHeight = 0;
                int length = 0;
                byte* numPtr = numRef + 6;
                int num7 = Marshal.SizeOf(typeof(SafeNativeMethods.ICONDIRENTRY));
                if ((num7 * num3) >= this.iconData.Length)
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("InvalidPictureType", new object[] { "picture", "Icon" }));
                }
                for (int i = 0; i < num3; i++)
                {
                    SafeNativeMethods.ICONDIRENTRY icondirentry;
                    icondirentry.bWidth = numPtr[0];
                    icondirentry.bHeight = numPtr[1];
                    icondirentry.bColorCount = numPtr[2];
                    icondirentry.bReserved = numPtr[3];
                    icondirentry.wPlanes = this.GetShort(numPtr + 4);
                    icondirentry.wBitCount = this.GetShort(numPtr + 6);
                    icondirentry.dwBytesInRes = this.GetInt(numPtr + 8);
                    icondirentry.dwImageOffset = this.GetInt(numPtr + 12);
                    bool flag = false;
                    int wBitCount = 0;
                    if (icondirentry.bColorCount != 0)
                    {
                        wBitCount = 4;
                        if (icondirentry.bColorCount < 0x10)
                        {
                            wBitCount = 1;
                        }
                    }
                    else
                    {
                        wBitCount = icondirentry.wBitCount;
                    }
                    if (wBitCount == 0)
                    {
                        wBitCount = 8;
                    }
                    if (length == 0)
                    {
                        flag = true;
                    }
                    else
                    {
                        int num10 = Math.Abs((int) (bWidth - width)) + Math.Abs((int) (bHeight - height));
                        int introduced25 = Math.Abs((int) (icondirentry.bWidth - width));
                        int num11 = introduced25 + Math.Abs((int) (icondirentry.bHeight - height));
                        if ((num11 < num10) || ((num11 == num10) && (((wBitCount <= bitDepth) && (wBitCount > this.bestBitDepth)) || ((this.bestBitDepth > bitDepth) && (wBitCount < this.bestBitDepth)))))
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        bWidth = icondirentry.bWidth;
                        bHeight = icondirentry.bHeight;
                        this.bestImageOffset = icondirentry.dwImageOffset;
                        length = icondirentry.dwBytesInRes;
                        this.bestBitDepth = wBitCount;
                    }
                    numPtr += num7;
                }
                if ((this.bestImageOffset < 0) || ((this.bestImageOffset + length) > this.iconData.Length))
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("InvalidPictureType", new object[] { "picture", "Icon" }));
                }
                if ((this.bestImageOffset % IntPtr.Size) != 0)
                {
                    byte[] destinationArray = new byte[length];
                    Array.Copy(this.iconData, this.bestImageOffset, destinationArray, 0, length);
                    fixed (byte* numRef2 = destinationArray)
                    {
                        this.handle = SafeNativeMethods.CreateIconFromResourceEx(numRef2, length, true, 0x30000, 0, 0, 0);
                    }
                }
                else
                {
                    this.handle = SafeNativeMethods.CreateIconFromResourceEx(numRef + this.bestImageOffset, length, true, 0x30000, 0, 0, 0);
                }
                if (this.handle == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }
            }
        }

        public void Save(Stream outputStream)
        {
            if (this.iconData != null)
            {
                outputStream.Write(this.iconData, 0, this.iconData.Length);
            }
            else
            {
                SafeNativeMethods.PICTDESC pictdesc = SafeNativeMethods.PICTDESC.CreateIconPICTDESC(this.Handle);
                Guid gUID = typeof(SafeNativeMethods.IPicture).GUID;
                SafeNativeMethods.IPicture o = SafeNativeMethods.OleCreatePictureIndirect(pictdesc, ref gUID, false);
                if (o != null)
                {
                    try
                    {
                        int num;
                        o.SaveAsFile(new System.Drawing.UnsafeNativeMethods.ComStreamFromDataStream(outputStream), -1, out num);
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(o);
                    }
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
        {
            if (this.iconData != null)
            {
                si.AddValue("IconData", this.iconData, typeof(byte[]));
            }
            else
            {
                MemoryStream outputStream = new MemoryStream();
                this.Save(outputStream);
                si.AddValue("IconData", outputStream.ToArray(), typeof(byte[]));
            }
            si.AddValue("IconSize", this.iconSize, typeof(System.Drawing.Size));
        }

        public unsafe Bitmap ToBitmap()
        {
            Bitmap image = null;
            if ((this.iconData != null) && (this.bestBitDepth == 0x20))
            {
                image = new Bitmap(this.Size.Width, this.Size.Height, PixelFormat.Format32bppArgb);
                BitmapData bitmapdata = image.LockBits(new Rectangle(0, 0, this.Size.Width, this.Size.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                try
                {
                    uint* numPtr = (uint*) bitmapdata.Scan0.ToPointer();
                    int num = this.bestImageOffset + Marshal.SizeOf(typeof(SafeNativeMethods.BITMAPINFOHEADER));
                    int length = this.Size.Width * 4;
                    int width = this.Size.Width;
                    for (int i = (this.Size.Height - 1) * 4; i >= 0; i -= 4)
                    {
                        Marshal.Copy(this.iconData, num + (i * width), (IntPtr) numPtr, length);
                        numPtr += width;
                    }
                }
                finally
                {
                    image.UnlockBits(bitmapdata);
                }
            }
            else if ((this.bestBitDepth == 0) || (this.bestBitDepth == 0x20))
            {
                SafeNativeMethods.ICONINFO info = new SafeNativeMethods.ICONINFO();
                SafeNativeMethods.GetIconInfo(new HandleRef(this, this.handle), info);
                SafeNativeMethods.BITMAP bm = new SafeNativeMethods.BITMAP();
                try
                {
                    if (info.hbmColor != IntPtr.Zero)
                    {
                        SafeNativeMethods.GetObject(new HandleRef(null, info.hbmColor), Marshal.SizeOf(typeof(SafeNativeMethods.BITMAP)), bm);
                        if (bm.bmBitsPixel == 0x20)
                        {
                            Bitmap bitmap3 = null;
                            BitmapData bmpData = null;
                            BitmapData targetData = null;
                            System.Drawing.IntSecurity.ObjectFromWin32Handle.Assert();
                            try
                            {
                                bitmap3 = Image.FromHbitmap(info.hbmColor);
                                bmpData = bitmap3.LockBits(new Rectangle(0, 0, bitmap3.Width, bitmap3.Height), ImageLockMode.ReadOnly, bitmap3.PixelFormat);
                                if (BitmapHasAlpha(bmpData))
                                {
                                    image = new Bitmap(bmpData.Width, bmpData.Height, PixelFormat.Format32bppArgb);
                                    targetData = image.LockBits(new Rectangle(0, 0, bmpData.Width, bmpData.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                                    this.CopyBitmapData(bmpData, targetData);
                                }
                            }
                            finally
                            {
                                CodeAccessPermission.RevertAssert();
                                if ((bitmap3 != null) && (bmpData != null))
                                {
                                    bitmap3.UnlockBits(bmpData);
                                }
                                if ((image != null) && (targetData != null))
                                {
                                    image.UnlockBits(targetData);
                                }
                            }
                            bitmap3.Dispose();
                        }
                    }
                }
                finally
                {
                    if (info.hbmColor != IntPtr.Zero)
                    {
                        SafeNativeMethods.IntDeleteObject(new HandleRef(null, info.hbmColor));
                    }
                    if (info.hbmMask != IntPtr.Zero)
                    {
                        SafeNativeMethods.IntDeleteObject(new HandleRef(null, info.hbmMask));
                    }
                }
            }
            if (image == null)
            {
                System.Drawing.Size size = this.Size;
                image = new Bitmap(size.Width, size.Height);
                using (Graphics graphics = null)
                {
                    graphics = Graphics.FromImage(image);
                    System.Drawing.IntSecurity.ObjectFromWin32Handle.Assert();
                    try
                    {
                        using (Bitmap bitmap4 = Bitmap.FromHicon(this.Handle))
                        {
                            graphics.DrawImage(bitmap4, new Rectangle(0, 0, size.Width, size.Height));
                        }
                    }
                    catch (ArgumentException)
                    {
                        this.Draw(graphics, new Rectangle(0, 0, size.Width, size.Height));
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
                Color transparentColor = Color.FromArgb(13, 11, 12);
                image.MakeTransparent(transparentColor);
            }
            return image;
        }

        public override string ToString()
        {
            return System.Drawing.SR.GetString("toStringIcon");
        }

        [Browsable(false)]
        public IntPtr Handle
        {
            get
            {
                if (this.handle == IntPtr.Zero)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                return this.handle;
            }
        }

        [Browsable(false)]
        public int Height
        {
            get
            {
                return this.Size.Height;
            }
        }

        public System.Drawing.Size Size
        {
            get
            {
                if (this.iconSize.IsEmpty)
                {
                    SafeNativeMethods.ICONINFO info = new SafeNativeMethods.ICONINFO();
                    SafeNativeMethods.GetIconInfo(new HandleRef(this, this.Handle), info);
                    SafeNativeMethods.BITMAP bm = new SafeNativeMethods.BITMAP();
                    if (info.hbmColor != IntPtr.Zero)
                    {
                        SafeNativeMethods.GetObject(new HandleRef(null, info.hbmColor), Marshal.SizeOf(typeof(SafeNativeMethods.BITMAP)), bm);
                        SafeNativeMethods.IntDeleteObject(new HandleRef(null, info.hbmColor));
                        this.iconSize = new System.Drawing.Size(bm.bmWidth, bm.bmHeight);
                    }
                    else if (info.hbmMask != IntPtr.Zero)
                    {
                        SafeNativeMethods.GetObject(new HandleRef(null, info.hbmMask), Marshal.SizeOf(typeof(SafeNativeMethods.BITMAP)), bm);
                        this.iconSize = new System.Drawing.Size(bm.bmWidth, bm.bmHeight / 2);
                    }
                    if (info.hbmMask != IntPtr.Zero)
                    {
                        SafeNativeMethods.IntDeleteObject(new HandleRef(null, info.hbmMask));
                    }
                }
                return this.iconSize;
            }
        }

        [Browsable(false)]
        public int Width
        {
            get
            {
                return this.Size.Width;
            }
        }
    }
}

