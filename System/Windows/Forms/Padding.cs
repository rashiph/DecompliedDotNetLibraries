namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential), TypeConverter(typeof(PaddingConverter))]
    public struct Padding
    {
        private bool _all;
        private int _top;
        private int _left;
        private int _right;
        private int _bottom;
        public static readonly Padding Empty;
        public Padding(int all)
        {
            this._all = true;
            this._top = this._left = this._right = this._bottom = all;
        }

        public Padding(int left, int top, int right, int bottom)
        {
            this._top = top;
            this._left = left;
            this._right = right;
            this._bottom = bottom;
            this._all = ((this._top == this._left) && (this._top == this._right)) && (this._top == this._bottom);
        }

        [RefreshProperties(RefreshProperties.All)]
        public int All
        {
            get
            {
                if (!this._all)
                {
                    return -1;
                }
                return this._top;
            }
            set
            {
                if (!this._all || (this._top != value))
                {
                    this._all = true;
                    this._top = this._left = this._right = this._bottom = value;
                }
            }
        }
        [RefreshProperties(RefreshProperties.All)]
        public int Bottom
        {
            get
            {
                if (this._all)
                {
                    return this._top;
                }
                return this._bottom;
            }
            set
            {
                if (this._all || (this._bottom != value))
                {
                    this._all = false;
                    this._bottom = value;
                }
            }
        }
        [RefreshProperties(RefreshProperties.All)]
        public int Left
        {
            get
            {
                if (this._all)
                {
                    return this._top;
                }
                return this._left;
            }
            set
            {
                if (this._all || (this._left != value))
                {
                    this._all = false;
                    this._left = value;
                }
            }
        }
        [RefreshProperties(RefreshProperties.All)]
        public int Right
        {
            get
            {
                if (this._all)
                {
                    return this._top;
                }
                return this._right;
            }
            set
            {
                if (this._all || (this._right != value))
                {
                    this._all = false;
                    this._right = value;
                }
            }
        }
        [RefreshProperties(RefreshProperties.All)]
        public int Top
        {
            get
            {
                return this._top;
            }
            set
            {
                if (this._all || (this._top != value))
                {
                    this._all = false;
                    this._top = value;
                }
            }
        }
        [Browsable(false)]
        public int Horizontal
        {
            get
            {
                return (this.Left + this.Right);
            }
        }
        [Browsable(false)]
        public int Vertical
        {
            get
            {
                return (this.Top + this.Bottom);
            }
        }
        [Browsable(false)]
        public System.Drawing.Size Size
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return new System.Drawing.Size(this.Horizontal, this.Vertical);
            }
        }
        public static Padding Add(Padding p1, Padding p2)
        {
            return (p1 + p2);
        }

        public static Padding Subtract(Padding p1, Padding p2)
        {
            return (p1 - p2);
        }

        public override bool Equals(object other)
        {
            return ((other is Padding) && (((Padding) other) == this));
        }

        public static Padding operator +(Padding p1, Padding p2)
        {
            return new Padding(p1.Left + p2.Left, p1.Top + p2.Top, p1.Right + p2.Right, p1.Bottom + p2.Bottom);
        }

        public static Padding operator -(Padding p1, Padding p2)
        {
            return new Padding(p1.Left - p2.Left, p1.Top - p2.Top, p1.Right - p2.Right, p1.Bottom - p2.Bottom);
        }

        public static bool operator ==(Padding p1, Padding p2)
        {
            return ((((p1.Left == p2.Left) && (p1.Top == p2.Top)) && (p1.Right == p2.Right)) && (p1.Bottom == p2.Bottom));
        }

        public static bool operator !=(Padding p1, Padding p2)
        {
            return !(p1 == p2);
        }

        public override int GetHashCode()
        {
            return (((this.Left ^ WindowsFormsUtils.RotateLeft(this.Top, 8)) ^ WindowsFormsUtils.RotateLeft(this.Right, 0x10)) ^ WindowsFormsUtils.RotateLeft(this.Bottom, 0x18));
        }

        public override string ToString()
        {
            return ("{Left=" + this.Left.ToString(CultureInfo.CurrentCulture) + ",Top=" + this.Top.ToString(CultureInfo.CurrentCulture) + ",Right=" + this.Right.ToString(CultureInfo.CurrentCulture) + ",Bottom=" + this.Bottom.ToString(CultureInfo.CurrentCulture) + "}");
        }

        private void ResetAll()
        {
            this.All = 0;
        }

        private void ResetBottom()
        {
            this.Bottom = 0;
        }

        private void ResetLeft()
        {
            this.Left = 0;
        }

        private void ResetRight()
        {
            this.Right = 0;
        }

        private void ResetTop()
        {
            this.Top = 0;
        }

        internal void Scale(float dx, float dy)
        {
            this._top = (int) (this._top * dy);
            this._left = (int) (this._left * dx);
            this._right = (int) (this._right * dx);
            this._bottom = (int) (this._bottom * dy);
        }

        internal bool ShouldSerializeAll()
        {
            return this._all;
        }

        [Conditional("DEBUG")]
        private void Debug_SanityCheck()
        {
            bool flag1 = this._all;
        }

        static Padding()
        {
            Empty = new Padding(0);
        }
    }
}

