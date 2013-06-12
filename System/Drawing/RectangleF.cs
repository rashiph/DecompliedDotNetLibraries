namespace System.Drawing
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Internal;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct RectangleF
    {
        public static readonly RectangleF Empty;
        private float x;
        private float y;
        private float width;
        private float height;
        public RectangleF(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public RectangleF(PointF location, SizeF size)
        {
            this.x = location.X;
            this.y = location.Y;
            this.width = size.Width;
            this.height = size.Height;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static RectangleF FromLTRB(float left, float top, float right, float bottom)
        {
            return new RectangleF(left, top, right - left, bottom - top);
        }

        [Browsable(false)]
        public PointF Location
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return new PointF(this.X, this.Y);
            }
            set
            {
                this.X = value.X;
                this.Y = value.Y;
            }
        }
        [Browsable(false)]
        public SizeF Size
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return new SizeF(this.Width, this.Height);
            }
            set
            {
                this.Width = value.Width;
                this.Height = value.Height;
            }
        }
        public float X
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
        public float Y
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
        public float Width
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
        public float Height
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
        public float Left
        {
            get
            {
                return this.X;
            }
        }
        [Browsable(false)]
        public float Top
        {
            get
            {
                return this.Y;
            }
        }
        [Browsable(false)]
        public float Right
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (this.X + this.Width);
            }
        }
        [Browsable(false)]
        public float Bottom
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
                if (this.Width > 0f)
                {
                    return (this.Height <= 0f);
                }
                return true;
            }
        }
        public override bool Equals(object obj)
        {
            if (!(obj is RectangleF))
            {
                return false;
            }
            RectangleF ef = (RectangleF) obj;
            return ((((ef.X == this.X) && (ef.Y == this.Y)) && (ef.Width == this.Width)) && (ef.Height == this.Height));
        }

        public static bool operator ==(RectangleF left, RectangleF right)
        {
            return ((((left.X == right.X) && (left.Y == right.Y)) && (left.Width == right.Width)) && (left.Height == right.Height));
        }

        public static bool operator !=(RectangleF left, RectangleF right)
        {
            return !(left == right);
        }

        public bool Contains(float x, float y)
        {
            return ((((this.X <= x) && (x < (this.X + this.Width))) && (this.Y <= y)) && (y < (this.Y + this.Height)));
        }

        public bool Contains(PointF pt)
        {
            return this.Contains(pt.X, pt.Y);
        }

        public bool Contains(RectangleF rect)
        {
            return ((((this.X <= rect.X) && ((rect.X + rect.Width) <= (this.X + this.Width))) && (this.Y <= rect.Y)) && ((rect.Y + rect.Height) <= (this.Y + this.Height)));
        }

        public override int GetHashCode()
        {
            return (int) (((((uint) this.X) ^ ((((uint) this.Y) << 13) | (((uint) this.Y) >> 0x13))) ^ ((((uint) this.Width) << 0x1a) | (((uint) this.Width) >> 6))) ^ ((((uint) this.Height) << 7) | (((uint) this.Height) >> 0x19)));
        }

        public void Inflate(float x, float y)
        {
            this.X -= x;
            this.Y -= y;
            this.Width += 2f * x;
            this.Height += 2f * y;
        }

        public void Inflate(SizeF size)
        {
            this.Inflate(size.Width, size.Height);
        }

        public static RectangleF Inflate(RectangleF rect, float x, float y)
        {
            RectangleF ef = rect;
            ef.Inflate(x, y);
            return ef;
        }

        public void Intersect(RectangleF rect)
        {
            RectangleF ef = Intersect(rect, this);
            this.X = ef.X;
            this.Y = ef.Y;
            this.Width = ef.Width;
            this.Height = ef.Height;
        }

        public static RectangleF Intersect(RectangleF a, RectangleF b)
        {
            float x = Math.Max(a.X, b.X);
            float num2 = Math.Min((float) (a.X + a.Width), (float) (b.X + b.Width));
            float y = Math.Max(a.Y, b.Y);
            float num4 = Math.Min((float) (a.Y + a.Height), (float) (b.Y + b.Height));
            if ((num2 >= x) && (num4 >= y))
            {
                return new RectangleF(x, y, num2 - x, num4 - y);
            }
            return Empty;
        }

        public bool IntersectsWith(RectangleF rect)
        {
            return ((((rect.X < (this.X + this.Width)) && (this.X < (rect.X + rect.Width))) && (rect.Y < (this.Y + this.Height))) && (this.Y < (rect.Y + rect.Height)));
        }

        public static RectangleF Union(RectangleF a, RectangleF b)
        {
            float x = Math.Min(a.X, b.X);
            float num2 = Math.Max((float) (a.X + a.Width), (float) (b.X + b.Width));
            float y = Math.Min(a.Y, b.Y);
            float num4 = Math.Max((float) (a.Y + a.Height), (float) (b.Y + b.Height));
            return new RectangleF(x, y, num2 - x, num4 - y);
        }

        public void Offset(PointF pos)
        {
            this.Offset(pos.X, pos.Y);
        }

        public void Offset(float x, float y)
        {
            this.X += x;
            this.Y += y;
        }

        internal GPRECTF ToGPRECTF()
        {
            return new GPRECTF(this.X, this.Y, this.Width, this.Height);
        }

        public static implicit operator RectangleF(Rectangle r)
        {
            return new RectangleF((float) r.X, (float) r.Y, (float) r.Width, (float) r.Height);
        }

        public override string ToString()
        {
            return ("{X=" + this.X.ToString(CultureInfo.CurrentCulture) + ",Y=" + this.Y.ToString(CultureInfo.CurrentCulture) + ",Width=" + this.Width.ToString(CultureInfo.CurrentCulture) + ",Height=" + this.Height.ToString(CultureInfo.CurrentCulture) + "}");
        }

        static RectangleF()
        {
            Empty = new RectangleF();
        }
    }
}

