namespace System.Drawing.Printing
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.Serialization;

    [Serializable, TypeConverter(typeof(MarginsConverter))]
    public class Margins : ICloneable
    {
        private int bottom;
        [OptionalField]
        private double doubleBottom;
        [OptionalField]
        private double doubleLeft;
        [OptionalField]
        private double doubleRight;
        [OptionalField]
        private double doubleTop;
        private int left;
        private int right;
        private int top;

        public Margins() : this(100, 100, 100, 100)
        {
        }

        public Margins(int left, int right, int top, int bottom)
        {
            this.CheckMargin(left, "left");
            this.CheckMargin(right, "right");
            this.CheckMargin(top, "top");
            this.CheckMargin(bottom, "bottom");
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
            this.doubleLeft = left;
            this.doubleRight = right;
            this.doubleTop = top;
            this.doubleBottom = bottom;
        }

        private void CheckMargin(int margin, string name)
        {
            if (margin < 0)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InvalidLowBoundArgumentEx", new object[] { name, margin, "0" }));
            }
        }

        public object Clone()
        {
            return base.MemberwiseClone();
        }

        public override bool Equals(object obj)
        {
            Margins margins = obj as Margins;
            if (margins == this)
            {
                return true;
            }
            if (margins == null)
            {
                return false;
            }
            return ((((margins.Left == this.Left) && (margins.Right == this.Right)) && (margins.Top == this.Top)) && (margins.Bottom == this.Bottom));
        }

        public override int GetHashCode()
        {
            uint left = (uint) this.Left;
            uint right = (uint) this.Right;
            uint top = (uint) this.Top;
            uint bottom = (uint) this.Bottom;
            uint num5 = ((left ^ ((right << 13) | (right >> 0x13))) ^ ((top << 0x1a) | (top >> 6))) ^ ((bottom << 7) | (bottom >> 0x19));
            return (int) num5;
        }

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            if ((this.doubleLeft == 0.0) && (this.left != 0))
            {
                this.doubleLeft = this.left;
            }
            if ((this.doubleRight == 0.0) && (this.right != 0))
            {
                this.doubleRight = this.right;
            }
            if ((this.doubleTop == 0.0) && (this.top != 0))
            {
                this.doubleTop = this.top;
            }
            if ((this.doubleBottom == 0.0) && (this.bottom != 0))
            {
                this.doubleBottom = this.bottom;
            }
        }

        public static bool operator ==(Margins m1, Margins m2)
        {
            if (object.ReferenceEquals(m1, null) != object.ReferenceEquals(m2, null))
            {
                return false;
            }
            return (object.ReferenceEquals(m1, null) || ((((m1.Left == m2.Left) && (m1.Top == m2.Top)) && (m1.Right == m2.Right)) && (m1.Bottom == m2.Bottom)));
        }

        public static bool operator !=(Margins m1, Margins m2)
        {
            return !(m1 == m2);
        }

        public override string ToString()
        {
            return ("[Margins Left=" + this.Left.ToString(CultureInfo.InvariantCulture) + " Right=" + this.Right.ToString(CultureInfo.InvariantCulture) + " Top=" + this.Top.ToString(CultureInfo.InvariantCulture) + " Bottom=" + this.Bottom.ToString(CultureInfo.InvariantCulture) + "]");
        }

        public int Bottom
        {
            get
            {
                return this.bottom;
            }
            set
            {
                this.CheckMargin(value, "Bottom");
                this.bottom = value;
                this.doubleBottom = value;
            }
        }

        internal double DoubleBottom
        {
            get
            {
                return this.doubleBottom;
            }
            set
            {
                this.Bottom = (int) Math.Round(value);
                this.doubleBottom = value;
            }
        }

        internal double DoubleLeft
        {
            get
            {
                return this.doubleLeft;
            }
            set
            {
                this.Left = (int) Math.Round(value);
                this.doubleLeft = value;
            }
        }

        internal double DoubleRight
        {
            get
            {
                return this.doubleRight;
            }
            set
            {
                this.Right = (int) Math.Round(value);
                this.doubleRight = value;
            }
        }

        internal double DoubleTop
        {
            get
            {
                return this.doubleTop;
            }
            set
            {
                this.Top = (int) Math.Round(value);
                this.doubleTop = value;
            }
        }

        public int Left
        {
            get
            {
                return this.left;
            }
            set
            {
                this.CheckMargin(value, "Left");
                this.left = value;
                this.doubleLeft = value;
            }
        }

        public int Right
        {
            get
            {
                return this.right;
            }
            set
            {
                this.CheckMargin(value, "Right");
                this.right = value;
                this.doubleRight = value;
            }
        }

        public int Top
        {
            get
            {
                return this.top;
            }
            set
            {
                this.CheckMargin(value, "Top");
                this.top = value;
                this.doubleTop = value;
            }
        }
    }
}

