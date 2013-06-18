namespace System.Drawing
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Drawing.Imaging;
    using System.Drawing.Internal;
    using System.IO;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable, ImmutableObject(true), Editor("System.Drawing.Design.ImageEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), TypeConverter(typeof(ImageConverter)), ComVisible(true)]
    public abstract class Image : MarshalByRefObject, ISerializable, ICloneable, IDisposable
    {
        internal IntPtr nativeImage;
        private byte[] rawData;
        private object userData;

        internal Image()
        {
        }

        internal Image(IntPtr nativeImage)
        {
            this.SetNativeImage(nativeImage);
        }

        internal Image(SerializationInfo info, StreamingContext context)
        {
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            if (enumerator != null)
            {
                while (enumerator.MoveNext())
                {
                    if (string.Equals(enumerator.Name, "Data", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            byte[] buffer = (byte[]) enumerator.Value;
                            if (buffer != null)
                            {
                                this.InitializeFromStream(new MemoryStream(buffer));
                            }
                        }
                        catch (ExternalException)
                        {
                        }
                        catch (ArgumentException)
                        {
                        }
                        catch (OutOfMemoryException)
                        {
                        }
                        catch (InvalidOperationException)
                        {
                        }
                        catch (NotImplementedException)
                        {
                        }
                        catch (FileNotFoundException)
                        {
                        }
                    }
                }
            }
        }

        private ColorPalette _GetColorPalette()
        {
            int size = -1;
            int status = SafeNativeMethods.Gdip.GdipGetImagePaletteSize(new HandleRef(this, this.nativeImage), out size);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            ColorPalette palette = new ColorPalette(size);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            status = SafeNativeMethods.Gdip.GdipGetImagePalette(new HandleRef(this, this.nativeImage), ptr, size);
            try
            {
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                palette.ConvertFromMemory(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return palette;
        }

        private SizeF _GetPhysicalDimension()
        {
            float num;
            float num2;
            int status = SafeNativeMethods.Gdip.GdipGetImageDimension(new HandleRef(this, this.nativeImage), out num, out num2);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return new SizeF(num, num2);
        }

        private void _SetColorPalette(ColorPalette palette)
        {
            IntPtr ptr = palette.ConvertToMemory();
            int status = SafeNativeMethods.Gdip.GdipSetImagePalette(new HandleRef(this, this.nativeImage), ptr);
            if (ptr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(ptr);
            }
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public object Clone()
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCloneImage(new HandleRef(this, this.nativeImage), out zero);
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
            return CreateImageObject(zero);
        }

        internal static Image CreateImageObject(IntPtr nativeImage)
        {
            int type = -1;
            int status = SafeNativeMethods.Gdip.GdipGetImageType(new HandleRef(null, nativeImage), out type);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            switch (((ImageTypeEnum) type))
            {
                case ImageTypeEnum.Bitmap:
                    return Bitmap.FromGDIplus(nativeImage);

                case ImageTypeEnum.Metafile:
                    return Metafile.FromGDIplus(nativeImage);
            }
            throw new ArgumentException(System.Drawing.SR.GetString("InvalidImage"));
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.nativeImage != IntPtr.Zero)
            {
                try
                {
                    SafeNativeMethods.Gdip.GdipDisposeImage(new HandleRef(this, this.nativeImage));
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
                    this.nativeImage = IntPtr.Zero;
                }
            }
        }

        internal static void EnsureSave(Image image, string filename, Stream dataStream)
        {
            if (image.RawFormat.Equals(ImageFormat.Gif))
            {
                bool flag = false;
                foreach (Guid guid in image.FrameDimensionsList)
                {
                    FrameDimension dimension = new FrameDimension(guid);
                    if (dimension.Equals(FrameDimension.Time))
                    {
                        flag = image.GetFrameCount(FrameDimension.Time) > 1;
                        break;
                    }
                }
                if (flag)
                {
                    try
                    {
                        Stream stream = null;
                        long position = 0L;
                        if (dataStream != null)
                        {
                            position = dataStream.Position;
                            dataStream.Position = 0L;
                        }
                        try
                        {
                            if (dataStream == null)
                            {
                                stream = dataStream = File.OpenRead(filename);
                            }
                            image.rawData = new byte[(int) dataStream.Length];
                            dataStream.Read(image.rawData, 0, (int) dataStream.Length);
                        }
                        finally
                        {
                            if (stream != null)
                            {
                                stream.Close();
                            }
                            else
                            {
                                dataStream.Position = position;
                            }
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                    catch (DirectoryNotFoundException)
                    {
                    }
                    catch (IOException)
                    {
                    }
                    catch (NotSupportedException)
                    {
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
        }

        ~Image()
        {
            this.Dispose(false);
        }

        public static Image FromFile(string filename)
        {
            return FromFile(filename, false);
        }

        public static Image FromFile(string filename, bool useEmbeddedColorManagement)
        {
            int num;
            if (!File.Exists(filename))
            {
                System.Drawing.IntSecurity.DemandReadFileIO(filename);
                throw new FileNotFoundException(filename);
            }
            filename = Path.GetFullPath(filename);
            IntPtr zero = IntPtr.Zero;
            if (useEmbeddedColorManagement)
            {
                num = SafeNativeMethods.Gdip.GdipLoadImageFromFileICM(filename, out zero);
            }
            else
            {
                num = SafeNativeMethods.Gdip.GdipLoadImageFromFile(filename, out zero);
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
            Image image = CreateImageObject(zero);
            EnsureSave(image, filename, null);
            return image;
        }

        public static Bitmap FromHbitmap(IntPtr hbitmap)
        {
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            return FromHbitmap(hbitmap, IntPtr.Zero);
        }

        public static Bitmap FromHbitmap(IntPtr hbitmap, IntPtr hpalette)
        {
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateBitmapFromHBITMAP(new HandleRef(null, hbitmap), new HandleRef(null, hpalette), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return Bitmap.FromGDIplus(zero);
        }

        public static Image FromStream(Stream stream)
        {
            return FromStream(stream, false);
        }

        public static Image FromStream(Stream stream, bool useEmbeddedColorManagement)
        {
            return FromStream(stream, useEmbeddedColorManagement, true);
        }

        public static Image FromStream(Stream stream, bool useEmbeddedColorManagement, bool validateImageData)
        {
            int num;
            if (!validateImageData)
            {
                System.Drawing.IntSecurity.UnmanagedCode.Demand();
            }
            if (stream == null)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InvalidArgument", new object[] { "stream", "null" }));
            }
            IntPtr zero = IntPtr.Zero;
            if (useEmbeddedColorManagement)
            {
                num = SafeNativeMethods.Gdip.GdipLoadImageFromStreamICM(new GPStream(stream), out zero);
            }
            else
            {
                num = SafeNativeMethods.Gdip.GdipLoadImageFromStream(new GPStream(stream), out zero);
            }
            if (num != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(num);
            }
            if (validateImageData)
            {
                num = SafeNativeMethods.Gdip.GdipImageForceValidation(new HandleRef(null, zero));
                if (num != 0)
                {
                    SafeNativeMethods.Gdip.GdipDisposeImage(new HandleRef(null, zero));
                    throw SafeNativeMethods.Gdip.StatusException(num);
                }
            }
            Image image = CreateImageObject(zero);
            EnsureSave(image, null, stream);
            return image;
        }

        public RectangleF GetBounds(ref GraphicsUnit pageUnit)
        {
            GPRECTF gprectf = new GPRECTF();
            int status = SafeNativeMethods.Gdip.GdipGetImageBounds(new HandleRef(this, this.nativeImage), ref gprectf, out pageUnit);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return gprectf.ToRectangleF();
        }

        public EncoderParameters GetEncoderParameterList(Guid encoder)
        {
            EncoderParameters parameters;
            int num;
            int status = SafeNativeMethods.Gdip.GdipGetEncoderParameterListSize(new HandleRef(this, this.nativeImage), ref encoder, out num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            if (num <= 0)
            {
                return null;
            }
            IntPtr buffer = Marshal.AllocHGlobal(num);
            status = SafeNativeMethods.Gdip.GdipGetEncoderParameterList(new HandleRef(this, this.nativeImage), ref encoder, num, buffer);
            try
            {
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                parameters = EncoderParameters.ConvertFromMemory(buffer);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
            return parameters;
        }

        public int GetFrameCount(FrameDimension dimension)
        {
            int[] count = new int[1];
            Guid dimensionID = dimension.Guid;
            int status = SafeNativeMethods.Gdip.GdipImageGetFrameCount(new HandleRef(this, this.nativeImage), ref dimensionID, count);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return count[0];
        }

        public static int GetPixelFormatSize(System.Drawing.Imaging.PixelFormat pixfmt)
        {
            return ((((int) pixfmt) >> 8) & 0xff);
        }

        public PropertyItem GetPropertyItem(int propid)
        {
            PropertyItem item;
            int num;
            int status = SafeNativeMethods.Gdip.GdipGetPropertyItemSize(new HandleRef(this, this.nativeImage), propid, out num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            if (num == 0)
            {
                return null;
            }
            IntPtr buffer = Marshal.AllocHGlobal(num);
            if (buffer == IntPtr.Zero)
            {
                throw SafeNativeMethods.Gdip.StatusException(3);
            }
            status = SafeNativeMethods.Gdip.GdipGetPropertyItem(new HandleRef(this, this.nativeImage), propid, num, buffer);
            try
            {
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                item = PropertyItemInternal.ConvertFromMemory(buffer, 1)[0];
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
            return item;
        }

        public Image GetThumbnailImage(int thumbWidth, int thumbHeight, GetThumbnailImageAbort callback, IntPtr callbackData)
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipGetImageThumbnail(new HandleRef(this, this.nativeImage), thumbWidth, thumbHeight, out zero, callback, callbackData);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return CreateImageObject(zero);
        }

        private void InitializeFromStream(Stream stream)
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipLoadImageFromStream(new GPStream(stream), out zero);
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
            this.nativeImage = zero;
            int type = -1;
            status = SafeNativeMethods.Gdip.GdipGetImageType(new HandleRef(this, this.nativeImage), out type);
            EnsureSave(this, null, stream);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public static bool IsAlphaPixelFormat(System.Drawing.Imaging.PixelFormat pixfmt)
        {
            return ((pixfmt & System.Drawing.Imaging.PixelFormat.Alpha) != System.Drawing.Imaging.PixelFormat.Undefined);
        }

        public static bool IsCanonicalPixelFormat(System.Drawing.Imaging.PixelFormat pixfmt)
        {
            return ((pixfmt & System.Drawing.Imaging.PixelFormat.Canonical) != System.Drawing.Imaging.PixelFormat.Undefined);
        }

        public static bool IsExtendedPixelFormat(System.Drawing.Imaging.PixelFormat pixfmt)
        {
            return ((pixfmt & System.Drawing.Imaging.PixelFormat.Extended) != System.Drawing.Imaging.PixelFormat.Undefined);
        }

        public void RemovePropertyItem(int propid)
        {
            int status = SafeNativeMethods.Gdip.GdipRemovePropertyItem(new HandleRef(this, this.nativeImage), propid);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void RotateFlip(RotateFlipType rotateFlipType)
        {
            int status = SafeNativeMethods.Gdip.GdipImageRotateFlip(new HandleRef(this, this.nativeImage), (int) rotateFlipType);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        internal void Save(MemoryStream stream)
        {
            ImageFormat rawFormat = this.RawFormat;
            if (rawFormat == ImageFormat.Jpeg)
            {
                rawFormat = ImageFormat.Png;
            }
            ImageCodecInfo encoder = rawFormat.FindEncoder();
            if (encoder == null)
            {
                encoder = ImageFormat.Png.FindEncoder();
            }
            this.Save(stream, encoder, null);
        }

        public void Save(string filename)
        {
            this.Save(filename, this.RawFormat);
        }

        public void Save(Stream stream, ImageFormat format)
        {
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }
            ImageCodecInfo encoder = format.FindEncoder();
            this.Save(stream, encoder, null);
        }

        public void Save(string filename, ImageFormat format)
        {
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }
            ImageCodecInfo encoder = format.FindEncoder();
            if (encoder == null)
            {
                encoder = ImageFormat.Png.FindEncoder();
            }
            this.Save(filename, encoder, null);
        }

        public void Save(Stream stream, ImageCodecInfo encoder, EncoderParameters encoderParams)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (encoder == null)
            {
                throw new ArgumentNullException("encoder");
            }
            IntPtr zero = IntPtr.Zero;
            if (encoderParams != null)
            {
                this.rawData = null;
                zero = encoderParams.ConvertToMemory();
            }
            int status = 0;
            try
            {
                Guid clsid = encoder.Clsid;
                bool flag = false;
                if (this.rawData != null)
                {
                    ImageCodecInfo info = this.RawFormat.FindEncoder();
                    if ((info != null) && (info.Clsid == clsid))
                    {
                        stream.Write(this.rawData, 0, this.rawData.Length);
                        flag = true;
                    }
                }
                if (!flag)
                {
                    status = SafeNativeMethods.Gdip.GdipSaveImageToStream(new HandleRef(this, this.nativeImage), new System.Drawing.UnsafeNativeMethods.ComStreamFromDataStream(stream), ref clsid, new HandleRef(encoderParams, zero));
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
            }
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Save(string filename, ImageCodecInfo encoder, EncoderParameters encoderParams)
        {
            if (filename == null)
            {
                throw new ArgumentNullException("filename");
            }
            if (encoder == null)
            {
                throw new ArgumentNullException("encoder");
            }
            System.Drawing.IntSecurity.DemandWriteFileIO(filename);
            IntPtr zero = IntPtr.Zero;
            if (encoderParams != null)
            {
                this.rawData = null;
                zero = encoderParams.ConvertToMemory();
            }
            int status = 0;
            try
            {
                Guid clsid = encoder.Clsid;
                bool flag = false;
                if (this.rawData != null)
                {
                    ImageCodecInfo info = this.RawFormat.FindEncoder();
                    if ((info != null) && (info.Clsid == clsid))
                    {
                        using (FileStream stream = File.OpenWrite(filename))
                        {
                            stream.Write(this.rawData, 0, this.rawData.Length);
                            flag = true;
                        }
                    }
                }
                if (!flag)
                {
                    status = SafeNativeMethods.Gdip.GdipSaveImageToFile(new HandleRef(this, this.nativeImage), filename, ref clsid, new HandleRef(encoderParams, zero));
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
            }
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void SaveAdd(EncoderParameters encoderParams)
        {
            IntPtr zero = IntPtr.Zero;
            if (encoderParams != null)
            {
                zero = encoderParams.ConvertToMemory();
            }
            this.rawData = null;
            int status = SafeNativeMethods.Gdip.GdipSaveAdd(new HandleRef(this, this.nativeImage), new HandleRef(encoderParams, zero));
            if (zero != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(zero);
            }
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void SaveAdd(Image image, EncoderParameters encoderParams)
        {
            IntPtr zero = IntPtr.Zero;
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            if (encoderParams != null)
            {
                zero = encoderParams.ConvertToMemory();
            }
            this.rawData = null;
            int status = SafeNativeMethods.Gdip.GdipSaveAddImage(new HandleRef(this, this.nativeImage), new HandleRef(image, image.nativeImage), new HandleRef(encoderParams, zero));
            if (zero != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(zero);
            }
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public int SelectActiveFrame(FrameDimension dimension, int frameIndex)
        {
            int[] numArray = new int[1];
            Guid dimensionID = dimension.Guid;
            int status = SafeNativeMethods.Gdip.GdipImageSelectActiveFrame(new HandleRef(this, this.nativeImage), ref dimensionID, frameIndex);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return numArray[0];
        }

        internal void SetNativeImage(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("NativeHandle0"), "handle");
            }
            this.nativeImage = handle;
        }

        public void SetPropertyItem(PropertyItem propitem)
        {
            PropertyItemInternal internal2 = PropertyItemInternal.ConvertFromPropertyItem(propitem);
            using (internal2)
            {
                int status = SafeNativeMethods.Gdip.GdipSetPropertyItem(new HandleRef(this, this.nativeImage), internal2);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                this.Save(stream);
                si.AddValue("Data", stream.ToArray(), typeof(byte[]));
            }
        }

        [Browsable(false)]
        public int Flags
        {
            get
            {
                int num;
                int status = SafeNativeMethods.Gdip.GdipGetImageFlags(new HandleRef(this, this.nativeImage), out num);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return num;
            }
        }

        [Browsable(false)]
        public Guid[] FrameDimensionsList
        {
            get
            {
                int num;
                int status = SafeNativeMethods.Gdip.GdipImageGetFrameDimensionsCount(new HandleRef(this, this.nativeImage), out num);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                if (num <= 0)
                {
                    return new Guid[0];
                }
                int num3 = Marshal.SizeOf(typeof(Guid));
                IntPtr buffer = Marshal.AllocHGlobal((int) (num3 * num));
                if (buffer == IntPtr.Zero)
                {
                    throw SafeNativeMethods.Gdip.StatusException(3);
                }
                status = SafeNativeMethods.Gdip.GdipImageGetFrameDimensionsList(new HandleRef(this, this.nativeImage), buffer, num);
                if (status != 0)
                {
                    Marshal.FreeHGlobal(buffer);
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                Guid[] guidArray = new Guid[num];
                try
                {
                    for (int i = 0; i < num; i++)
                    {
                        guidArray[i] = (Guid) System.Drawing.UnsafeNativeMethods.PtrToStructure((IntPtr) (((long) buffer) + (num3 * i)), typeof(Guid));
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
                return guidArray;
            }
        }

        [Browsable(false), DefaultValue(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Height
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                int num;
                int status = SafeNativeMethods.Gdip.GdipGetImageHeight(new HandleRef(this, this.nativeImage), out num);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return num;
            }
        }

        public float HorizontalResolution
        {
            get
            {
                float num;
                int status = SafeNativeMethods.Gdip.GdipGetImageHorizontalResolution(new HandleRef(this, this.nativeImage), out num);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return num;
            }
        }

        [Browsable(false)]
        public ColorPalette Palette
        {
            get
            {
                return this._GetColorPalette();
            }
            set
            {
                this._SetColorPalette(value);
            }
        }

        public SizeF PhysicalDimension
        {
            get
            {
                return this._GetPhysicalDimension();
            }
        }

        public System.Drawing.Imaging.PixelFormat PixelFormat
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                int num;
                if (SafeNativeMethods.Gdip.GdipGetImagePixelFormat(new HandleRef(this, this.nativeImage), out num) != 0)
                {
                    return System.Drawing.Imaging.PixelFormat.Undefined;
                }
                return (System.Drawing.Imaging.PixelFormat) num;
            }
        }

        [Browsable(false)]
        public int[] PropertyIdList
        {
            get
            {
                int num;
                int status = SafeNativeMethods.Gdip.GdipGetPropertyCount(new HandleRef(this, this.nativeImage), out num);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                int[] list = new int[num];
                if (num != 0)
                {
                    status = SafeNativeMethods.Gdip.GdipGetPropertyIdList(new HandleRef(this, this.nativeImage), num, list);
                    if (status != 0)
                    {
                        throw SafeNativeMethods.Gdip.StatusException(status);
                    }
                }
                return list;
            }
        }

        [Browsable(false)]
        public PropertyItem[] PropertyItems
        {
            get
            {
                int num;
                int num2;
                int status = SafeNativeMethods.Gdip.GdipGetPropertyCount(new HandleRef(this, this.nativeImage), out num2);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                status = SafeNativeMethods.Gdip.GdipGetPropertySize(new HandleRef(this, this.nativeImage), out num, ref num2);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                if ((num == 0) || (num2 == 0))
                {
                    return new PropertyItem[0];
                }
                IntPtr buffer = Marshal.AllocHGlobal(num);
                status = SafeNativeMethods.Gdip.GdipGetAllPropertyItems(new HandleRef(this, this.nativeImage), num, num2, buffer);
                PropertyItem[] itemArray = null;
                try
                {
                    if (status != 0)
                    {
                        throw SafeNativeMethods.Gdip.StatusException(status);
                    }
                    itemArray = PropertyItemInternal.ConvertFromMemory(buffer, num2);
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
                return itemArray;
            }
        }

        public ImageFormat RawFormat
        {
            get
            {
                Guid format = new Guid();
                int status = SafeNativeMethods.Gdip.GdipGetImageRawFormat(new HandleRef(this, this.nativeImage), ref format);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return new ImageFormat(format);
            }
        }

        public System.Drawing.Size Size
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return new System.Drawing.Size(this.Width, this.Height);
            }
        }

        [Localizable(false), TypeConverter(typeof(StringConverter)), Bindable(true), DefaultValue((string) null)]
        public object Tag
        {
            get
            {
                return this.userData;
            }
            set
            {
                this.userData = value;
            }
        }

        public float VerticalResolution
        {
            get
            {
                float num;
                int status = SafeNativeMethods.Gdip.GdipGetImageVerticalResolution(new HandleRef(this, this.nativeImage), out num);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return num;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), DefaultValue(false)]
        public int Width
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                int num;
                int status = SafeNativeMethods.Gdip.GdipGetImageWidth(new HandleRef(this, this.nativeImage), out num);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return num;
            }
        }

        public delegate bool GetThumbnailImageAbort();

        private enum ImageTypeEnum
        {
            Bitmap = 1,
            Metafile = 2
        }
    }
}

