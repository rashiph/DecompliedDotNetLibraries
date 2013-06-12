namespace System.Drawing
{
    using System;
    using System.Globalization;
    using System.IO;

    [AttributeUsage(AttributeTargets.Class)]
    public class ToolboxBitmapAttribute : Attribute
    {
        public static readonly ToolboxBitmapAttribute Default = new ToolboxBitmapAttribute(null, null);
        private static readonly ToolboxBitmapAttribute DefaultComponent;
        private static readonly Point largeDim = new Point(0x20, 0x20);
        private Image largeImage;
        private static readonly Point smallDim = new Point(0x10, 0x10);
        private Image smallImage;

        static ToolboxBitmapAttribute()
        {
            SafeNativeMethods.Gdip.DummyFunction();
            Bitmap img = null;
            Stream manifestResourceStream = typeof(ToolboxBitmapAttribute).Module.Assembly.GetManifestResourceStream(typeof(ToolboxBitmapAttribute), "DefaultComponent.bmp");
            if (manifestResourceStream != null)
            {
                img = new Bitmap(manifestResourceStream);
                MakeBackgroundAlphaZero(img);
            }
            DefaultComponent = new ToolboxBitmapAttribute(img, null);
        }

        public ToolboxBitmapAttribute(string imageFile) : this(GetImageFromFile(imageFile, false), GetImageFromFile(imageFile, true))
        {
        }

        public ToolboxBitmapAttribute(Type t) : this(GetImageFromResource(t, null, false), GetImageFromResource(t, null, true))
        {
        }

        private ToolboxBitmapAttribute(Image smallImage, Image largeImage)
        {
            this.smallImage = smallImage;
            this.largeImage = largeImage;
        }

        public ToolboxBitmapAttribute(Type t, string name) : this(GetImageFromResource(t, name, false), GetImageFromResource(t, name, true))
        {
        }

        public override bool Equals(object value)
        {
            if (value == this)
            {
                return true;
            }
            ToolboxBitmapAttribute attribute = value as ToolboxBitmapAttribute;
            if (attribute == null)
            {
                return false;
            }
            return ((attribute.smallImage == this.smallImage) && (attribute.largeImage == this.largeImage));
        }

        private static Image GetBitmapFromResource(Type t, string bitmapname, bool large)
        {
            if (bitmapname == null)
            {
                return null;
            }
            Image image = null;
            Stream manifestResourceStream = t.Module.Assembly.GetManifestResourceStream(t, bitmapname);
            if (manifestResourceStream != null)
            {
                Bitmap img = new Bitmap(manifestResourceStream);
                image = img;
                MakeBackgroundAlphaZero(img);
                if (large)
                {
                    image = new Bitmap(img, largeDim.X, largeDim.Y);
                }
            }
            return image;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private static Image GetIconFromResource(Type t, string bitmapname, bool large)
        {
            if (bitmapname == null)
            {
                return null;
            }
            return GetIconFromStream(t.Module.Assembly.GetManifestResourceStream(t, bitmapname), large);
        }

        private static Image GetIconFromStream(Stream stream, bool large)
        {
            if (stream == null)
            {
                return null;
            }
            Icon original = new Icon(stream);
            Icon icon2 = new Icon(original, large ? new Size(largeDim.X, largeDim.Y) : new Size(smallDim.X, smallDim.Y));
            return icon2.ToBitmap();
        }

        public Image GetImage(object component)
        {
            return this.GetImage(component, true);
        }

        public Image GetImage(Type type)
        {
            return this.GetImage(type, false);
        }

        public Image GetImage(object component, bool large)
        {
            if (component != null)
            {
                return this.GetImage(component.GetType(), large);
            }
            return null;
        }

        public Image GetImage(Type type, bool large)
        {
            return this.GetImage(type, null, large);
        }

        public Image GetImage(Type type, string imgName, bool large)
        {
            if ((large && (this.largeImage == null)) || (!large && (this.smallImage == null)))
            {
                Point point = new Point(0x20, 0x20);
                Image largeImage = null;
                if (large)
                {
                    largeImage = this.largeImage;
                }
                else
                {
                    largeImage = this.smallImage;
                }
                if (largeImage == null)
                {
                    largeImage = GetImageFromResource(type, imgName, large);
                }
                if ((large && (this.largeImage == null)) && (this.smallImage != null))
                {
                    largeImage = new Bitmap((Bitmap) this.smallImage, point.X, point.Y);
                }
                Bitmap img = largeImage as Bitmap;
                if (img != null)
                {
                    MakeBackgroundAlphaZero(img);
                }
                if (largeImage == null)
                {
                    largeImage = DefaultComponent.GetImage(type, large);
                }
                if (large)
                {
                    this.largeImage = largeImage;
                }
                else
                {
                    this.smallImage = largeImage;
                }
            }
            Image image2 = large ? this.largeImage : this.smallImage;
            if (this.Equals(Default))
            {
                this.largeImage = null;
                this.smallImage = null;
            }
            return image2;
        }

        private static Image GetImageFromFile(string imageFile, bool large)
        {
            Image image = null;
            try
            {
                if (imageFile == null)
                {
                    return image;
                }
                string extension = Path.GetExtension(imageFile);
                if ((extension != null) && string.Equals(extension, ".ico", StringComparison.OrdinalIgnoreCase))
                {
                    FileStream stream = File.Open(imageFile, FileMode.Open);
                    if (stream == null)
                    {
                        return image;
                    }
                    try
                    {
                        return GetIconFromStream(stream, large);
                    }
                    finally
                    {
                        stream.Close();
                    }
                }
                if (!large)
                {
                    image = Image.FromFile(imageFile);
                }
            }
            catch (Exception exception)
            {
                if (System.Drawing.ClientUtils.IsCriticalException(exception))
                {
                    throw;
                }
            }
            return image;
        }

        public static Image GetImageFromResource(Type t, string imageName, bool large)
        {
            Image image = null;
            try
            {
                string fullName = imageName;
                string bitmapname = null;
                string str3 = null;
                string str4 = null;
                if (fullName == null)
                {
                    fullName = t.FullName;
                    int num = fullName.LastIndexOf('.');
                    if (num != -1)
                    {
                        fullName = fullName.Substring(num + 1);
                    }
                    bitmapname = fullName + ".ico";
                    str3 = fullName + ".bmp";
                }
                else if (string.Compare(Path.GetExtension(imageName), ".ico", true, CultureInfo.CurrentCulture) == 0)
                {
                    bitmapname = fullName;
                }
                else if (string.Compare(Path.GetExtension(imageName), ".bmp", true, CultureInfo.CurrentCulture) == 0)
                {
                    str3 = fullName;
                }
                else
                {
                    str4 = fullName;
                    str3 = fullName + ".bmp";
                    bitmapname = fullName + ".ico";
                }
                image = GetBitmapFromResource(t, str4, large);
                if (image == null)
                {
                    image = GetBitmapFromResource(t, str3, large);
                }
                if (image == null)
                {
                    image = GetIconFromResource(t, bitmapname, large);
                }
            }
            catch (Exception)
            {
                bool flag1 = t == null;
            }
            return image;
        }

        private static void MakeBackgroundAlphaZero(Bitmap img)
        {
            Color pixel = img.GetPixel(0, img.Height - 1);
            img.MakeTransparent();
            Color color = Color.FromArgb(0, pixel);
            img.SetPixel(0, img.Height - 1, color);
        }
    }
}

