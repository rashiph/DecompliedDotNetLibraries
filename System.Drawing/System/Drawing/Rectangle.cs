namespace System.Drawing
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true), TypeConverter(typeof(RectangleConverter))]
    public struct Rectangle
    {
        public static readonly Rectangle Empty;
        private int x;
        private int y;
        private int width;
        private int height;
        public Rectangle(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public Rectangle(Point location, System.Drawing.Size size)
        {
            this.x = location.X;
            this.y = location.Y;
            this.width = size.Width;
            this.height = size.Height;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static Rectangle FromLTRB(int left, int top, int right, int bottom)
        {
            return new Rectangle(left, top, right - left, bottom - top);
        }

        [Browsable(false)]
        public Point Location
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return new Point(this.X, this.Y);
            }
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            set
            {
                this.X = value.X;
                this.Y = value.Y;
            }
        }
        [Browsable(false)]
        public System.Drawing.Size Size
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return new System.Drawing.Size(this.Width, this.Height);
            }
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            set
            {
                this.Width = value.Width;
                this.Height = value.Height;
            }
        }
        public int X
        {
            get
            {
                return this.x;
            }
            set
            {
                this.x = value;
            }
        }
        public int Y
        {
            get
            {
                return this.y;
            }
            set
            {
                this.y = value;
            }
        }
        public int Width
        {
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
            get
            {
                return this.height;
            }
            set
            {
                this.height = value;
            }
        }
        [Browsable(false)]
        public int Left
        {
            get
            {
                return this.X;
            }
        }
        [Browsable(false)]
        public int Top
        {
            get
            {
                return this.Y;
            }
        }
        [Browsable(false)]
        public int Right
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (this.X + this.Width);
            }
        }
        [Browsable(false)]
        public int Bottom
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (this.Y + this.Height);
            }
        }
        [Browsable(false)]
        public bool IsEmpty
        {
            get
            {
                return ((((this.height == 0) && (this.width == 0)) && (this.x == 0)) && (this.y == 0));
            }
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Rectangle))
            {
                return false;
            }
            Rectangle rectangle = (Rectangle) obj;
            return ((((rectangle.X == this.X) && (rectangle.Y == this.Y)) && (rectangle.Width == this.Width)) && (rectangle.Height == this.Height));
        }

        public static bool operator ==(Rectangle left, Rectangle right)
        {
            return ((((left.X == right.X) && (left.Y == right.Y)) && (left.Width == right.Width)) && (left.Height == right.Height));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator !=(Rectangle left, Rectangle right)
        {
            return !(left == right);
        }

        public static Rectangle Ceiling(RectangleF value)
        {
            return new Rectangle((int) Math.Ceiling((double) value.X), (int) Math.Ceiling((double) value.Y), (int) Math.Ceiling((double) value.Width), (int) Math.Ceiling((double) value.Height));
        }

        public static Rectangle Truncate(RectangleF value)
        {
            return new Rectangle((int) value.X, (int) value.Y, (int) value.Width, (int) value.Height);
        }

        public static Rectangle Round(RectangleF value)
        {
            return new Rectangle((int) Math.Round((double) value.X), (int) Math.Round((double) value.Y), (int) Math.Round((double) value.Width), (int) Math.Round((double) value.Height));
        }

        public bool Contains(int x, int y)
        {
            return ((((this.X <= x) && (x < (this.X + this.Width))) && (this.Y <= y)) && (y < (this.Y + this.Height)));
        }

        public bool Contains(Point pt)
        {
            return this.Contains(pt.X, pt.Y);
        }

        public bool Contains(Rectangle rect)
        {
            return ((((this.X <= rect.X) && ((rect.X + rect.Width) <= (this.X + this.Width))) && (this.Y <= rect.Y)) && ((rect.Y + rect.Height) <= (this.Y + this.Height)));
        }

        public override int GetHashCode()
        {
            return (((this.X ^ ((this.Y << 13) | (this.Y >> 0x13))) ^ ((this.Width << 0x1a) | (this.Width >> 6))) ^ ((this.Height << 7) | (this.Height >> 0x19)));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public void Inflate(int width, int height)
        {
            this.X -= width;
            this.Y -= height;
            this.Width += 2 * width;
            this.Height += 2 * height;
        }

        public void Inflate(System.Drawing.Size size)
        {
            this.Inflate(size.Width, size.Height);
        }

        public static Rectangle Inflate(Rectangle rect, int x, int y)
        {
            Rectangle rectangle = rect;
            rectangle.Inflate(x, y);
            return rectangle;
        }

        public void Intersect(Rectangle rect)
        {
            Rectangle rectangle = Intersect(rect, this);
            this.X = rectangle.X;
            this.Y = rectangle.Y;
            this.Width = rectangle.Width;
            this.Height = rectangle.Height;
        }

        public static Rectangle Intersect(Rectangle a, Rectangle b)
        {
            int x = Math.Max(a.X, b.X);
            int num2 = Math.Min((int) (a.X + a.Width), (int) (b.X + b.Width));
            int y = Math.Max(a.Y, b.Y);
            int num4 = Math.Min((int) (a.Y + a.Height), (int) (b.Y + b.Height));
            if ((num2 >= x) && (num4 >= y))
            {
                return new Rectangle(x, y, num2 - x, num4 - y);
            }
            return Empty;
        }

        public bool IntersectsWith(Rectangle rect)
        {
            return ((((rect.X < (this.X + this.Width)) && (this.X < (rect.X + rect.Width))) && (rect.Y < (this.Y + this.Height))) && (this.Y < (rect.Y + rect.Height)));
        }

        public static Rectangle Union(Rectangle a, Rectangle b)
        {
            int x = Math.Min(a.X, b.X);
            int num2 = Math.Max((int) (a.X + a.Width), (int) (b.X + b.Width));
            int y = Math.Min(a.Y, b.Y);
            int num4 = Math.Max((int) (a.Y + a.Height), (int) (b.Y + b.Height));
            return new Rectangle(x, y, num2 - x, num4 - y);
        }

        public void Offset(Point pos)
        {
            this.Offset(pos.X, pos.Y);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public void Offset(int x, int y)
        {
            this.X += x;
            this.Y += y;
        }

        public override string ToString()
        {
            return ("{X=" + this.X.ToString(CultureInfo.CurrentCulture) + ",Y=" + this.Y.ToString(CultureInfo.CurrentCulture) + ",Width=" + this.Width.ToString(CultureInfo.CurrentCulture) + ",Height=" + this.Height.ToString(CultureInfo.CurrentCulture) + "}");
        }

        static Rectangle()
        {
            Empty = new Rectangle();
        }
    }
}

