namespace System.Drawing.Imaging
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime;

    [TypeConverter(typeof(ImageFormatConverter))]
    public sealed class ImageFormat
    {
        private static ImageFormat bmp = new ImageFormat(new System.Guid("{b96b3cab-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat emf = new ImageFormat(new System.Guid("{b96b3cac-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat exif = new ImageFormat(new System.Guid("{b96b3cb2-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat flashPIX = new ImageFormat(new System.Guid("{b96b3cb4-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat gif = new ImageFormat(new System.Guid("{b96b3cb0-0728-11d3-9d7b-0000f81ef32e}"));
        private System.Guid guid;
        private static ImageFormat icon = new ImageFormat(new System.Guid("{b96b3cb5-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat jpeg = new ImageFormat(new System.Guid("{b96b3cae-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat memoryBMP = new ImageFormat(new System.Guid("{b96b3caa-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat photoCD = new ImageFormat(new System.Guid("{b96b3cb3-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat png = new ImageFormat(new System.Guid("{b96b3caf-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat tiff = new ImageFormat(new System.Guid("{b96b3cb1-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat wmf = new ImageFormat(new System.Guid("{b96b3cad-0728-11d3-9d7b-0000f81ef32e}"));

        public ImageFormat(System.Guid guid)
        {
            this.guid = guid;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override bool Equals(object o)
        {
            ImageFormat format = o as ImageFormat;
            if (format == null)
            {
                return false;
            }
            return (this.guid == format.guid);
        }

        internal ImageCodecInfo FindEncoder()
        {
            foreach (ImageCodecInfo info in ImageCodecInfo.GetImageEncoders())
            {
                if (info.FormatID.Equals(this.guid))
                {
                    return info;
                }
            }
            return null;
        }

        public override int GetHashCode()
        {
            return this.guid.GetHashCode();
        }

        public override string ToString()
        {
            if (this == memoryBMP)
            {
                return "MemoryBMP";
            }
            if (this == bmp)
            {
                return "Bmp";
            }
            if (this == emf)
            {
                return "Emf";
            }
            if (this == wmf)
            {
                return "Wmf";
            }
            if (this == gif)
            {
                return "Gif";
            }
            if (this == jpeg)
            {
                return "Jpeg";
            }
            if (this == png)
            {
                return "Png";
            }
            if (this == tiff)
            {
                return "Tiff";
            }
            if (this == exif)
            {
                return "Exif";
            }
            if (this == icon)
            {
                return "Icon";
            }
            return ("[ImageFormat: " + this.guid + "]");
        }

        public static ImageFormat Bmp
        {
            get
            {
                return bmp;
            }
        }

        public static ImageFormat Emf
        {
            get
            {
                return emf;
            }
        }

        public static ImageFormat Exif
        {
            get
            {
                return exif;
            }
        }

        public static ImageFormat Gif
        {
            get
            {
                return gif;
            }
        }

        public System.Guid Guid
        {
            get
            {
                return this.guid;
            }
        }

        public static ImageFormat Icon
        {
            get
            {
                return icon;
            }
        }

        public static ImageFormat Jpeg
        {
            get
            {
                return jpeg;
            }
        }

        public static ImageFormat MemoryBmp
        {
            get
            {
                return memoryBMP;
            }
        }

        public static ImageFormat Png
        {
            get
            {
                return png;
            }
        }

        public static ImageFormat Tiff
        {
            get
            {
                return tiff;
            }
        }

        public static ImageFormat Wmf
        {
            get
            {
                return wmf;
            }
        }
    }
}

