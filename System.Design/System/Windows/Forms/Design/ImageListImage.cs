namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Drawing.Imaging;
    using System.IO;

    [Editor(typeof(ImageListImageEditor), typeof(UITypeEditor))]
    internal class ImageListImage
    {
        private Bitmap _image;
        private string _name;

        public ImageListImage(Bitmap image)
        {
            this.Image = image;
        }

        public ImageListImage(Bitmap image, string name)
        {
            this.Image = image;
            this.Name = name;
        }

        public static ImageListImage ImageListImageFromStream(Stream stream, bool imageIsIcon)
        {
            if (imageIsIcon)
            {
                return new ImageListImage(new Icon(stream).ToBitmap());
            }
            return new ImageListImage((Bitmap) System.Drawing.Image.FromStream(stream));
        }

        public float HorizontalResolution
        {
            get
            {
                return this._image.HorizontalResolution;
            }
        }

        [Browsable(false)]
        public Bitmap Image
        {
            get
            {
                return this._image;
            }
            set
            {
                this._image = value;
            }
        }

        public string Name
        {
            get
            {
                if (this._name != null)
                {
                    return this._name;
                }
                return "";
            }
            set
            {
                this._name = value;
            }
        }

        public SizeF PhysicalDimension
        {
            get
            {
                return (SizeF) this._image.Size;
            }
        }

        public System.Drawing.Imaging.PixelFormat PixelFormat
        {
            get
            {
                return this._image.PixelFormat;
            }
        }

        public ImageFormat RawFormat
        {
            get
            {
                return this._image.RawFormat;
            }
        }

        public System.Drawing.Size Size
        {
            get
            {
                return this._image.Size;
            }
        }

        public float VerticalResolution
        {
            get
            {
                return this._image.VerticalResolution;
            }
        }
    }
}

