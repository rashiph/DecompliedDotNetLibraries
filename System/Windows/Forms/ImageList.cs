namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;

    [Designer("System.Windows.Forms.Design.ImageListDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DesignerSerializer("System.Windows.Forms.Design.ImageListCodeDomSerializer, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ToolboxItemFilter("System.Windows.Forms"), DefaultProperty("Images"), System.Windows.Forms.SRDescription("DescriptionImageList"), TypeConverter(typeof(ImageListConverter))]
    public sealed class ImageList : Component
    {
        private System.Windows.Forms.ColorDepth colorDepth;
        private static Size DefaultImageSize = new Size(0x10, 0x10);
        private static Color fakeTransparencyColor = Color.FromArgb(13, 11, 12);
        private const int GROWBY = 4;
        private ImageCollection imageCollection;
        private Size imageSize;
        private bool inAddRange;
        private const int INITIAL_CAPACITY = 4;
        private NativeImageList nativeImageList;
        private IList originals;
        private Color transparentColor;
        private object userData;

        internal event EventHandler ChangeHandle;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRDescription("ImageListOnRecreateHandleDescr")]
        public event EventHandler RecreateHandle;

        public ImageList()
        {
            this.colorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.transparentColor = Color.Transparent;
            this.imageSize = DefaultImageSize;
            this.originals = new ArrayList();
        }

        public ImageList(IContainer container)
        {
            this.colorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.transparentColor = Color.Transparent;
            this.imageSize = DefaultImageSize;
            this.originals = new ArrayList();
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            container.Add(this);
        }

        private int AddIconToHandle(Original original, Icon icon)
        {
            int num2;
            try
            {
                int num = System.Windows.Forms.SafeNativeMethods.ImageList_ReplaceIcon(new HandleRef(this, this.Handle), -1, new HandleRef(icon, icon.Handle));
                if (num == -1)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ImageListAddFailed"));
                }
                num2 = num;
            }
            finally
            {
                if ((original.options & OriginalOptions.OwnsImage) != OriginalOptions.Default)
                {
                    icon.Dispose();
                }
            }
            return num2;
        }

        private int AddToHandle(Original original, Bitmap bitmap)
        {
            IntPtr monochromeMask = ControlPaint.CreateHBitmapTransparencyMask(bitmap);
            IntPtr handle = ControlPaint.CreateHBitmapColorMask(bitmap, monochromeMask);
            int num = System.Windows.Forms.SafeNativeMethods.ImageList_Add(new HandleRef(this, this.Handle), new HandleRef(null, handle), new HandleRef(null, monochromeMask));
            System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(null, handle));
            System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(null, monochromeMask));
            if (num == -1)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ImageListAddFailed"));
            }
            return num;
        }

        private static unsafe bool BitmapHasAlpha(BitmapData bmpData)
        {
            if ((bmpData.PixelFormat != PixelFormat.Format32bppArgb) && (bmpData.PixelFormat != PixelFormat.Format32bppRgb))
            {
                return false;
            }
            for (int i = 0; i < bmpData.Height; i++)
            {
                int num2 = i * bmpData.Stride;
                for (int j = 3; j < (bmpData.Width * 4); j += 4)
                {
                    byte* numPtr = (byte*) ((bmpData.Scan0.ToPointer() + num2) + j);
                    if (numPtr[0] != 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void CopyBitmapData(BitmapData sourceData, BitmapData targetData)
        {
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < targetData.Height; i++)
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
                System.Windows.Forms.UnsafeNativeMethods.CopyMemory(new HandleRef(this, ptr2), new HandleRef(this, ptr), Math.Abs(targetData.Stride));
                num += sourceData.Stride;
                num2 += targetData.Stride;
            }
        }

        private Bitmap CreateBitmap(Original original, out bool ownsBitmap)
        {
            Bitmap image;
            Color transparentColor = this.transparentColor;
            ownsBitmap = false;
            if ((original.options & OriginalOptions.CustomTransparentColor) != OriginalOptions.Default)
            {
                transparentColor = original.customTransparentColor;
            }
            if (original.image is Bitmap)
            {
                image = (Bitmap) original.image;
            }
            else if (original.image is Icon)
            {
                image = ((Icon) original.image).ToBitmap();
                ownsBitmap = true;
            }
            else
            {
                image = new Bitmap((Image) original.image);
                ownsBitmap = true;
            }
            if (transparentColor.A > 0)
            {
                Bitmap bitmap2 = image;
                image = (Bitmap) image.Clone();
                image.MakeTransparent(transparentColor);
                if (ownsBitmap)
                {
                    bitmap2.Dispose();
                }
                ownsBitmap = true;
            }
            Size size = image.Size;
            if ((original.options & OriginalOptions.ImageStrip) != OriginalOptions.Default)
            {
                if ((size.Width == 0) || ((size.Width % this.imageSize.Width) != 0))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("ImageListStripBadWidth"), "original");
                }
                if (size.Height != this.imageSize.Height)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("ImageListImageTooShort"), "original");
                }
                return image;
            }
            if (!size.Equals(this.ImageSize))
            {
                Bitmap bitmap3 = image;
                image = new Bitmap(bitmap3, this.ImageSize);
                if (ownsBitmap)
                {
                    bitmap3.Dispose();
                }
                ownsBitmap = true;
            }
            return image;
        }

        private void CreateHandle()
        {
            int flags = 1;
            switch (this.colorDepth)
            {
                case System.Windows.Forms.ColorDepth.Depth16Bit:
                    flags |= 0x10;
                    break;

                case System.Windows.Forms.ColorDepth.Depth24Bit:
                    flags |= 0x18;
                    break;

                case System.Windows.Forms.ColorDepth.Depth32Bit:
                    flags |= 0x20;
                    break;

                case System.Windows.Forms.ColorDepth.Depth4Bit:
                    flags |= 4;
                    break;

                case System.Windows.Forms.ColorDepth.Depth8Bit:
                    flags |= 8;
                    break;
            }
            IntPtr userCookie = System.Windows.Forms.UnsafeNativeMethods.ThemingScope.Activate();
            try
            {
                System.Windows.Forms.SafeNativeMethods.InitCommonControls();
                this.nativeImageList = new NativeImageList(System.Windows.Forms.SafeNativeMethods.ImageList_Create(this.imageSize.Width, this.imageSize.Height, flags, 4, 4));
            }
            finally
            {
                System.Windows.Forms.UnsafeNativeMethods.ThemingScope.Deactivate(userCookie);
            }
            if (this.Handle == IntPtr.Zero)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ImageListCreateFailed"));
            }
            System.Windows.Forms.SafeNativeMethods.ImageList_SetBkColor(new HandleRef(this, this.Handle), -1);
            for (int i = 0; i < this.originals.Count; i++)
            {
                Original original = (Original) this.originals[i];
                if (original.image is Icon)
                {
                    this.AddIconToHandle(original, (Icon) original.image);
                }
                else
                {
                    bool ownsBitmap = false;
                    Bitmap bitmap = this.CreateBitmap(original, out ownsBitmap);
                    this.AddToHandle(original, bitmap);
                    if (ownsBitmap)
                    {
                        bitmap.Dispose();
                    }
                }
            }
            this.originals = null;
        }

        private void DestroyHandle()
        {
            if (this.HandleCreated)
            {
                this.nativeImageList.Dispose();
                this.nativeImageList = null;
                this.originals = new ArrayList();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.originals != null)
                {
                    foreach (Original original in this.originals)
                    {
                        if ((original.options & OriginalOptions.OwnsImage) != OriginalOptions.Default)
                        {
                            ((IDisposable) original.image).Dispose();
                        }
                    }
                }
                this.DestroyHandle();
            }
            base.Dispose(disposing);
        }

        public void Draw(Graphics g, Point pt, int index)
        {
            this.Draw(g, pt.X, pt.Y, index);
        }

        public void Draw(Graphics g, int x, int y, int index)
        {
            this.Draw(g, x, y, this.imageSize.Width, this.imageSize.Height, index);
        }

        public void Draw(Graphics g, int x, int y, int width, int height, int index)
        {
            if ((index < 0) || (index >= this.Images.Count))
            {
                throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
            }
            IntPtr hdc = g.GetHdc();
            try
            {
                System.Windows.Forms.SafeNativeMethods.ImageList_DrawEx(new HandleRef(this, this.Handle), index, new HandleRef(g, hdc), x, y, width, height, -1, -1, 1);
            }
            finally
            {
                g.ReleaseHdcInternal(hdc);
            }
        }

        private Bitmap GetBitmap(int index)
        {
            if ((index < 0) || (index >= this.Images.Count))
            {
                throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
            }
            Bitmap image = null;
            if (this.ColorDepth == System.Windows.Forms.ColorDepth.Depth32Bit)
            {
                System.Windows.Forms.NativeMethods.IMAGEINFO pImageInfo = new System.Windows.Forms.NativeMethods.IMAGEINFO();
                if (System.Windows.Forms.SafeNativeMethods.ImageList_GetImageInfo(new HandleRef(this, this.Handle), index, pImageInfo))
                {
                    Bitmap bitmap2 = null;
                    BitmapData bmpData = null;
                    BitmapData targetData = null;
                    System.Windows.Forms.IntSecurity.ObjectFromWin32Handle.Assert();
                    try
                    {
                        bitmap2 = Image.FromHbitmap(pImageInfo.hbmImage);
                        bmpData = bitmap2.LockBits(new Rectangle(pImageInfo.rcImage_left, pImageInfo.rcImage_top, pImageInfo.rcImage_right - pImageInfo.rcImage_left, pImageInfo.rcImage_bottom - pImageInfo.rcImage_top), ImageLockMode.ReadOnly, bitmap2.PixelFormat);
                        int stride = bmpData.Stride;
                        int height = this.imageSize.Height;
                        if (BitmapHasAlpha(bmpData))
                        {
                            image = new Bitmap(this.imageSize.Width, this.imageSize.Height, PixelFormat.Format32bppArgb);
                            targetData = image.LockBits(new Rectangle(0, 0, this.imageSize.Width, this.imageSize.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                            this.CopyBitmapData(bmpData, targetData);
                        }
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                        if (bitmap2 != null)
                        {
                            if (bmpData != null)
                            {
                                bitmap2.UnlockBits(bmpData);
                            }
                            bitmap2.Dispose();
                        }
                        if ((image != null) && (targetData != null))
                        {
                            image.UnlockBits(targetData);
                        }
                    }
                }
            }
            if (image == null)
            {
                image = new Bitmap(this.imageSize.Width, this.imageSize.Height);
                using (Graphics graphics = Graphics.FromImage(image))
                {
                    IntPtr hdc = graphics.GetHdc();
                    try
                    {
                        System.Windows.Forms.SafeNativeMethods.ImageList_DrawEx(new HandleRef(this, this.Handle), index, new HandleRef(graphics, hdc), 0, 0, this.imageSize.Width, this.imageSize.Height, -1, -1, 1);
                    }
                    finally
                    {
                        graphics.ReleaseHdcInternal(hdc);
                    }
                }
            }
            image.MakeTransparent(fakeTransparencyColor);
            return image;
        }

        private void OnChangeHandle(EventArgs eventargs)
        {
            if (this.changeHandler != null)
            {
                this.changeHandler(this, eventargs);
            }
        }

        private void OnRecreateHandle(EventArgs eventargs)
        {
            if (this.recreateHandler != null)
            {
                this.recreateHandler(this, eventargs);
            }
        }

        private void PerformRecreateHandle(string reason)
        {
            if (this.HandleCreated)
            {
                if ((this.originals == null) || this.Images.Empty)
                {
                    this.originals = new ArrayList();
                }
                if (this.originals == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ImageListCantRecreate", new object[] { reason }));
                }
                this.DestroyHandle();
                this.CreateHandle();
                this.OnRecreateHandle(new EventArgs());
            }
        }

        private void ResetColorDepth()
        {
            this.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
        }

        private void ResetImageSize()
        {
            this.ImageSize = DefaultImageSize;
        }

        private void ResetTransparentColor()
        {
            this.TransparentColor = Color.LightGray;
        }

        private bool ShouldSerializeColorDepth()
        {
            return (this.Images.Count == 0);
        }

        private bool ShouldSerializeImageSize()
        {
            return (this.Images.Count == 0);
        }

        private bool ShouldSerializeTransparentColor()
        {
            return !this.TransparentColor.Equals(Color.LightGray);
        }

        public override string ToString()
        {
            string str = base.ToString();
            if (this.Images != null)
            {
                return (str + " Images.Count: " + this.Images.Count.ToString(CultureInfo.CurrentCulture) + ", ImageSize: " + this.ImageSize.ToString());
            }
            return str;
        }

        [System.Windows.Forms.SRDescription("ImageListColorDepthDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public System.Windows.Forms.ColorDepth ColorDepth
        {
            get
            {
                return this.colorDepth;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid_NotSequential(value, (int) value, new int[] { 4, 8, 0x10, 0x18, 0x20 }))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.ColorDepth));
                }
                if (this.colorDepth != value)
                {
                    this.colorDepth = value;
                    this.PerformRecreateHandle("ColorDepth");
                }
            }
        }

        [System.Windows.Forms.SRDescription("ImageListHandleDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        public IntPtr Handle
        {
            get
            {
                if (this.nativeImageList == null)
                {
                    this.CreateHandle();
                }
                return this.nativeImageList.Handle;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRDescription("ImageListHandleCreatedDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HandleCreated
        {
            get
            {
                return (this.nativeImageList != null);
            }
        }

        [DefaultValue((string) null), MergableProperty(false), System.Windows.Forms.SRCategory("CatAppearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ImageListImagesDescr")]
        public ImageCollection Images
        {
            get
            {
                if (this.imageCollection == null)
                {
                    this.imageCollection = new ImageCollection(this);
                }
                return this.imageCollection;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ImageListSizeDescr"), Localizable(true)]
        public Size ImageSize
        {
            get
            {
                return this.imageSize;
            }
            set
            {
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "ImageSize", "Size.Empty" }));
                }
                if ((value.Width <= 0) || (value.Width > 0x100))
                {
                    object[] args = new object[] { "ImageSize.Width", value.Width.ToString(CultureInfo.CurrentCulture), 1.ToString(CultureInfo.CurrentCulture), "256" };
                    throw new ArgumentOutOfRangeException("ImageSize", System.Windows.Forms.SR.GetString("InvalidBoundArgument", args));
                }
                if ((value.Height <= 0) || (value.Height > 0x100))
                {
                    object[] objArray3 = new object[] { "ImageSize.Height", value.Height.ToString(CultureInfo.CurrentCulture), 1.ToString(CultureInfo.CurrentCulture), "256" };
                    throw new ArgumentOutOfRangeException("ImageSize", System.Windows.Forms.SR.GetString("InvalidBoundArgument", objArray3));
                }
                if ((this.imageSize.Width != value.Width) || (this.imageSize.Height != value.Height))
                {
                    this.imageSize = new Size(value.Width, value.Height);
                    this.PerformRecreateHandle("ImageSize");
                }
            }
        }

        [Browsable(false), System.Windows.Forms.SRDescription("ImageListImageStreamDescr"), EditorBrowsable(EditorBrowsableState.Advanced), DefaultValue((string) null)]
        public ImageListStreamer ImageStream
        {
            get
            {
                if (this.Images.Empty)
                {
                    return null;
                }
                return new ImageListStreamer(this);
            }
            set
            {
                if (value == null)
                {
                    this.DestroyHandle();
                    this.Images.Clear();
                }
                else
                {
                    NativeImageList nativeImageList = value.GetNativeImageList();
                    if ((nativeImageList != null) && (nativeImageList != this.nativeImageList))
                    {
                        int num;
                        int num2;
                        bool handleCreated = this.HandleCreated;
                        this.DestroyHandle();
                        this.originals = null;
                        this.nativeImageList = new NativeImageList(System.Windows.Forms.SafeNativeMethods.ImageList_Duplicate(new HandleRef(nativeImageList, nativeImageList.Handle)));
                        if (System.Windows.Forms.SafeNativeMethods.ImageList_GetIconSize(new HandleRef(this, this.nativeImageList.Handle), out num, out num2))
                        {
                            this.imageSize = new Size(num, num2);
                        }
                        System.Windows.Forms.NativeMethods.IMAGEINFO pImageInfo = new System.Windows.Forms.NativeMethods.IMAGEINFO();
                        if (System.Windows.Forms.SafeNativeMethods.ImageList_GetImageInfo(new HandleRef(this, this.nativeImageList.Handle), 0, pImageInfo))
                        {
                            System.Windows.Forms.NativeMethods.BITMAP bm = new System.Windows.Forms.NativeMethods.BITMAP();
                            System.Windows.Forms.UnsafeNativeMethods.GetObject(new HandleRef(null, pImageInfo.hbmImage), Marshal.SizeOf(bm), bm);
                            switch (bm.bmBitsPixel)
                            {
                                case 0x10:
                                    this.colorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
                                    break;

                                case 0x18:
                                    this.colorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
                                    break;

                                case 0x20:
                                    this.colorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
                                    break;

                                case 4:
                                    this.colorDepth = System.Windows.Forms.ColorDepth.Depth4Bit;
                                    break;

                                case 8:
                                    this.colorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
                                    break;
                            }
                        }
                        this.Images.ResetKeys();
                        if (handleCreated)
                        {
                            this.OnRecreateHandle(new EventArgs());
                        }
                    }
                }
            }
        }

        [Localizable(false), TypeConverter(typeof(StringConverter)), System.Windows.Forms.SRCategory("CatData"), Bindable(true), System.Windows.Forms.SRDescription("ControlTagDescr"), DefaultValue((string) null)]
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

        [System.Windows.Forms.SRDescription("ImageListTransparentColorDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public Color TransparentColor
        {
            get
            {
                return this.transparentColor;
            }
            set
            {
                this.transparentColor = value;
            }
        }

        private bool UseTransparentColor
        {
            get
            {
                return (this.TransparentColor.A > 0);
            }
        }

        [Editor("System.Windows.Forms.Design.ImageCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public sealed class ImageCollection : IList, ICollection, IEnumerable
        {
            private ArrayList imageInfoCollection = new ArrayList();
            private int lastAccessedIndex = -1;
            private ImageList owner;

            internal ImageCollection(ImageList owner)
            {
                this.owner = owner;
            }

            public void Add(Icon value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.Add(new ImageList.Original(value.Clone(), ImageList.OriginalOptions.OwnsImage), null);
            }

            public void Add(Image value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                ImageList.Original original = new ImageList.Original(value, ImageList.OriginalOptions.Default);
                this.Add(original, null);
            }

            public int Add(Image value, Color transparentColor)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                ImageList.Original original = new ImageList.Original(value, ImageList.OriginalOptions.CustomTransparentColor, transparentColor);
                return this.Add(original, null);
            }

            public void Add(string key, Icon icon)
            {
                ImageInfo imageInfo = new ImageInfo {
                    Name = key
                };
                ImageList.Original original = new ImageList.Original(icon, ImageList.OriginalOptions.Default);
                this.Add(original, imageInfo);
            }

            public void Add(string key, Image image)
            {
                ImageInfo imageInfo = new ImageInfo {
                    Name = key
                };
                ImageList.Original original = new ImageList.Original(image, ImageList.OriginalOptions.Default);
                this.Add(original, imageInfo);
            }

            private int Add(ImageList.Original original, ImageInfo imageInfo)
            {
                if ((original == null) || (original.image == null))
                {
                    throw new ArgumentNullException("original");
                }
                int num = -1;
                if (original.image is Bitmap)
                {
                    if (this.owner.originals != null)
                    {
                        num = this.owner.originals.Add(original);
                    }
                    if (this.owner.HandleCreated)
                    {
                        bool ownsBitmap = false;
                        Bitmap bitmap = this.owner.CreateBitmap(original, out ownsBitmap);
                        num = this.owner.AddToHandle(original, bitmap);
                        if (ownsBitmap)
                        {
                            bitmap.Dispose();
                        }
                    }
                }
                else
                {
                    if (!(original.image is Icon))
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("ImageListBitmap"));
                    }
                    if (this.owner.originals != null)
                    {
                        num = this.owner.originals.Add(original);
                    }
                    if (this.owner.HandleCreated)
                    {
                        num = this.owner.AddIconToHandle(original, (Icon) original.image);
                    }
                }
                if ((original.options & ImageList.OriginalOptions.ImageStrip) != ImageList.OriginalOptions.Default)
                {
                    for (int i = 0; i < original.nImages; i++)
                    {
                        this.imageInfoCollection.Add(new ImageInfo());
                    }
                }
                else
                {
                    if (imageInfo == null)
                    {
                        imageInfo = new ImageInfo();
                    }
                    this.imageInfoCollection.Add(imageInfo);
                }
                if (!this.owner.inAddRange)
                {
                    this.owner.OnChangeHandle(new EventArgs());
                }
                return num;
            }

            public void AddRange(Image[] images)
            {
                if (images == null)
                {
                    throw new ArgumentNullException("images");
                }
                this.owner.inAddRange = true;
                foreach (Image image in images)
                {
                    this.Add(image);
                }
                this.owner.inAddRange = false;
                this.owner.OnChangeHandle(new EventArgs());
            }

            public int AddStrip(Image value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if ((value.Width == 0) || ((value.Width % this.owner.ImageSize.Width) != 0))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("ImageListStripBadWidth"), "value");
                }
                if (value.Height != this.owner.ImageSize.Height)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("ImageListImageTooShort"), "value");
                }
                int nImages = value.Width / this.owner.ImageSize.Width;
                ImageList.Original original = new ImageList.Original(value, ImageList.OriginalOptions.ImageStrip, nImages);
                return this.Add(original, null);
            }

            [Conditional("DEBUG")]
            private void AssertInvariant()
            {
            }

            public void Clear()
            {
                if (this.owner.originals != null)
                {
                    this.owner.originals.Clear();
                }
                this.imageInfoCollection.Clear();
                if (this.owner.HandleCreated)
                {
                    System.Windows.Forms.SafeNativeMethods.ImageList_Remove(new HandleRef(this.owner, this.owner.Handle), -1);
                }
                this.owner.OnChangeHandle(new EventArgs());
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public bool Contains(Image image)
            {
                throw new NotSupportedException();
            }

            public bool ContainsKey(string key)
            {
                return this.IsValidIndex(this.IndexOfKey(key));
            }

            public IEnumerator GetEnumerator()
            {
                Image[] imageArray = new Image[this.Count];
                for (int i = 0; i < imageArray.Length; i++)
                {
                    imageArray[i] = this.owner.GetBitmap(i);
                }
                return imageArray.GetEnumerator();
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public int IndexOf(Image image)
            {
                throw new NotSupportedException();
            }

            public int IndexOfKey(string key)
            {
                if ((key != null) && (key.Length != 0))
                {
                    if ((this.IsValidIndex(this.lastAccessedIndex) && (this.imageInfoCollection[this.lastAccessedIndex] != null)) && WindowsFormsUtils.SafeCompareStrings(((ImageInfo) this.imageInfoCollection[this.lastAccessedIndex]).Name, key, true))
                    {
                        return this.lastAccessedIndex;
                    }
                    for (int i = 0; i < this.Count; i++)
                    {
                        if ((this.imageInfoCollection[i] != null) && WindowsFormsUtils.SafeCompareStrings(((ImageInfo) this.imageInfoCollection[i]).Name, key, true))
                        {
                            this.lastAccessedIndex = i;
                            return i;
                        }
                    }
                    this.lastAccessedIndex = -1;
                }
                return -1;
            }

            private bool IsValidIndex(int index)
            {
                return ((index >= 0) && (index < this.Count));
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public void Remove(Image image)
            {
                throw new NotSupportedException();
            }

            public void RemoveAt(int index)
            {
                if ((index < 0) || (index >= this.Count))
                {
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                }
                if (!System.Windows.Forms.SafeNativeMethods.ImageList_Remove(new HandleRef(this.owner, this.owner.Handle), index))
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ImageListRemoveFailed"));
                }
                if (((this.imageInfoCollection != null) && (index >= 0)) && (index < this.imageInfoCollection.Count))
                {
                    this.imageInfoCollection.RemoveAt(index);
                    this.owner.OnChangeHandle(new EventArgs());
                }
            }

            public void RemoveByKey(string key)
            {
                int index = this.IndexOfKey(key);
                if (this.IsValidIndex(index))
                {
                    this.RemoveAt(index);
                }
            }

            internal void ResetKeys()
            {
                if (this.imageInfoCollection != null)
                {
                    this.imageInfoCollection.Clear();
                }
                for (int i = 0; i < this.Count; i++)
                {
                    this.imageInfoCollection.Add(new ImageInfo());
                }
            }

            public void SetKeyName(int index, string name)
            {
                if (!this.IsValidIndex(index))
                {
                    throw new IndexOutOfRangeException();
                }
                if (this.imageInfoCollection[index] == null)
                {
                    this.imageInfoCollection[index] = new ImageInfo();
                }
                ((ImageInfo) this.imageInfoCollection[index]).Name = name;
            }

            void ICollection.CopyTo(Array dest, int index)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    dest.SetValue(this.owner.GetBitmap(i), index++);
                }
            }

            int IList.Add(object value)
            {
                if (!(value is Image))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("ImageListBadImage"), "value");
                }
                this.Add((Image) value);
                return (this.Count - 1);
            }

            bool IList.Contains(object image)
            {
                return ((image is Image) && this.Contains((Image) image));
            }

            int IList.IndexOf(object image)
            {
                if (image is Image)
                {
                    return this.IndexOf((Image) image);
                }
                return -1;
            }

            void IList.Insert(int index, object value)
            {
                throw new NotSupportedException();
            }

            void IList.Remove(object image)
            {
                if (image is Image)
                {
                    this.Remove((Image) image);
                    this.owner.OnChangeHandle(new EventArgs());
                }
            }

            [Browsable(false)]
            public int Count
            {
                get
                {
                    if (this.owner.HandleCreated)
                    {
                        return System.Windows.Forms.SafeNativeMethods.ImageList_GetImageCount(new HandleRef(this.owner, this.owner.Handle));
                    }
                    int num = 0;
                    foreach (ImageList.Original original in this.owner.originals)
                    {
                        if (original != null)
                        {
                            num += original.nImages;
                        }
                    }
                    return num;
                }
            }

            public bool Empty
            {
                get
                {
                    return (this.Count == 0);
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
            public Image this[int index]
            {
                get
                {
                    if ((index < 0) || (index >= this.Count))
                    {
                        throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                    }
                    return this.owner.GetBitmap(index);
                }
                set
                {
                    if ((index < 0) || (index >= this.Count))
                    {
                        throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                    }
                    if (value == null)
                    {
                        throw new ArgumentNullException("value");
                    }
                    if (!(value is Bitmap))
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("ImageListBitmap"));
                    }
                    Bitmap bitmap = (Bitmap) value;
                    bool flag = false;
                    if (this.owner.UseTransparentColor)
                    {
                        bitmap = (Bitmap) bitmap.Clone();
                        bitmap.MakeTransparent(this.owner.transparentColor);
                        flag = true;
                    }
                    try
                    {
                        IntPtr monochromeMask = ControlPaint.CreateHBitmapTransparencyMask(bitmap);
                        IntPtr handle = ControlPaint.CreateHBitmapColorMask(bitmap, monochromeMask);
                        bool flag2 = System.Windows.Forms.SafeNativeMethods.ImageList_Replace(new HandleRef(this.owner, this.owner.Handle), index, new HandleRef(null, handle), new HandleRef(null, monochromeMask));
                        System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(null, handle));
                        System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(null, monochromeMask));
                        if (!flag2)
                        {
                            throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ImageListReplaceFailed"));
                        }
                    }
                    finally
                    {
                        if (flag)
                        {
                            bitmap.Dispose();
                        }
                    }
                }
            }

            public Image this[string key]
            {
                get
                {
                    if ((key != null) && (key.Length != 0))
                    {
                        int index = this.IndexOfKey(key);
                        if (this.IsValidIndex(index))
                        {
                            return this[index];
                        }
                    }
                    return null;
                }
            }

            public StringCollection Keys
            {
                get
                {
                    StringCollection strings = new StringCollection();
                    for (int i = 0; i < this.imageInfoCollection.Count; i++)
                    {
                        ImageInfo info = this.imageInfoCollection[i] as ImageInfo;
                        if (((info != null) && (info.Name != null)) && (info.Name.Length != 0))
                        {
                            strings.Add(info.Name);
                        }
                        else
                        {
                            strings.Add(string.Empty);
                        }
                    }
                    return strings;
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
                    return this;
                }
            }

            bool IList.IsFixedSize
            {
                get
                {
                    return false;
                }
            }

            object IList.this[int index]
            {
                get
                {
                    return this[index];
                }
                set
                {
                    if (!(value is Image))
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("ImageListBadImage"), "value");
                    }
                    this[index] = (Image) value;
                }
            }

            internal class ImageInfo
            {
                private string name;

                public string Name
                {
                    get
                    {
                        return this.name;
                    }
                    set
                    {
                        this.name = value;
                    }
                }
            }
        }

        internal class Indexer
        {
            private System.Windows.Forms.ImageList imageList;
            private int index = -1;
            private string key = string.Empty;
            private bool useIntegerIndex = true;

            public virtual int ActualIndex
            {
                get
                {
                    if (this.useIntegerIndex)
                    {
                        return this.Index;
                    }
                    if (this.ImageList != null)
                    {
                        return this.ImageList.Images.IndexOfKey(this.Key);
                    }
                    return -1;
                }
            }

            public virtual System.Windows.Forms.ImageList ImageList
            {
                get
                {
                    return this.imageList;
                }
                set
                {
                    this.imageList = value;
                }
            }

            public virtual int Index
            {
                get
                {
                    return this.index;
                }
                set
                {
                    this.key = string.Empty;
                    this.index = value;
                    this.useIntegerIndex = true;
                }
            }

            public virtual string Key
            {
                get
                {
                    return this.key;
                }
                set
                {
                    this.index = -1;
                    this.key = (value == null) ? string.Empty : value;
                    this.useIntegerIndex = false;
                }
            }
        }

        internal class NativeImageList : IDisposable
        {
            private IntPtr himl;

            internal NativeImageList(IntPtr himl)
            {
                this.himl = himl;
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            public void Dispose(bool disposing)
            {
                if (this.himl != IntPtr.Zero)
                {
                    System.Windows.Forms.SafeNativeMethods.ImageList_Destroy(new HandleRef(null, this.himl));
                    this.himl = IntPtr.Zero;
                }
            }

            ~NativeImageList()
            {
                this.Dispose(false);
            }

            internal IntPtr Handle
            {
                get
                {
                    return this.himl;
                }
            }
        }

        private class Original
        {
            internal Color customTransparentColor;
            internal object image;
            internal int nImages;
            internal ImageList.OriginalOptions options;

            internal Original(object image, ImageList.OriginalOptions options) : this(image, options, Color.Transparent)
            {
            }

            internal Original(object image, ImageList.OriginalOptions options, Color customTransparentColor)
            {
                this.customTransparentColor = Color.Transparent;
                this.nImages = 1;
                if (!(image is Icon) && !(image is Image))
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ImageListEntryType"));
                }
                this.image = image;
                this.options = options;
                this.customTransparentColor = customTransparentColor;
                ImageList.OriginalOptions options1 = options & ImageList.OriginalOptions.CustomTransparentColor;
            }

            internal Original(object image, ImageList.OriginalOptions options, int nImages) : this(image, options, Color.Transparent)
            {
                this.nImages = nImages;
            }
        }

        [Flags]
        private enum OriginalOptions
        {
            CustomTransparentColor = 2,
            Default = 0,
            ImageStrip = 1,
            OwnsImage = 4
        }
    }
}

