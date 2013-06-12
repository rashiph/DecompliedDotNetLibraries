namespace System.Drawing
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential), TypeConverter(typeof(SizeConverter)), ComVisible(true)]
    public struct Size
    {
        public static readonly Size Empty;
        private int width;
        private int height;
        public Size(Point pt)
        {
            this.width = pt.X;
            this.height = pt.Y;
        }

        public Size(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public static implicit operator SizeF(Size p)
        {
            return new SizeF((float) p.Width, (float) p.Height);
        }

        public static Size operator +(Size sz1, Size sz2)
        {
            return Add(sz1, sz2);
        }

        public static Size operator -(Size sz1, Size sz2)
        {
            return Subtract(sz1, sz2);
        }

        public static bool operator ==(Size sz1, Size sz2)
        {
            return ((sz1.Width == sz2.Width) && (sz1.Height == sz2.Height));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator !=(Size sz1, Size sz2)
        {
            return !(sz1 == sz2);
        }

        public static explicit operator Point(Size size)
        {
            return new Point(size.Width, size.Height);
        }

        [Browsable(false)]
        public bool IsEmpty
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return ((this.width == 0) && (this.height == 0));
            }
        }
        public int Width
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.width;
            }
            set
            {
                this.width = value;
            }
        }
        public int Height
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.height;
            }
            set
            {
                this.height = value;
            }
        }
        public static Size Add(Size sz1, Size sz2)
        {
            return new Size(sz1.Width + sz2.Width, sz1.Height + sz2.Height);
        }

        public static Size Ceiling(SizeF value)
        {
            return new Size((int) Math.Ceiling((double) value.Width), (int) Math.Ceiling((double) value.Height));
        }

        public static Size Subtract(Size sz1, Size sz2)
        {
            return new Size(sz1.Width - sz2.Width, sz1.Height - sz2.Height);
        }

        public static Size Truncate(SizeF value)
        {
            return new Size((int) value.Width, (int) value.Height);
        }

        public static Size Round(SizeF value)
        {
            return new Size((int) Math.Round((double) value.Width), (int) Math.Round((double) value.Height));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Size))
            {
                return false;
            }
            Size size = (Size) obj;
            return ((size.width == this.width) && (size.height == this.height));
        }

        public override int GetHashCode()
        {
            return (this.width ^ this.height);
        }

        public override string ToString()
        {
            return ("{Width=" + this.width.ToString(CultureInfo.CurrentCulture) + ", Height=" + this.height.ToString(CultureInfo.CurrentCulture) + "}");
        }

        static Size()
        {
            Empty = new Size();
        }
    }
}

