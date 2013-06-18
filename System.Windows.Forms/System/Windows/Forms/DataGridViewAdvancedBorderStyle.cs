namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;

    public sealed class DataGridViewAdvancedBorderStyle : ICloneable
    {
        private bool all;
        private DataGridViewAdvancedCellBorderStyle banned1;
        private DataGridViewAdvancedCellBorderStyle banned2;
        private DataGridViewAdvancedCellBorderStyle banned3;
        private DataGridViewAdvancedCellBorderStyle bottom;
        private DataGridViewAdvancedCellBorderStyle left;
        private DataGridView owner;
        private DataGridViewAdvancedCellBorderStyle right;
        private DataGridViewAdvancedCellBorderStyle top;

        public DataGridViewAdvancedBorderStyle() : this(null, DataGridViewAdvancedCellBorderStyle.NotSet, DataGridViewAdvancedCellBorderStyle.NotSet, DataGridViewAdvancedCellBorderStyle.NotSet)
        {
        }

        internal DataGridViewAdvancedBorderStyle(DataGridView owner) : this(owner, DataGridViewAdvancedCellBorderStyle.NotSet, DataGridViewAdvancedCellBorderStyle.NotSet, DataGridViewAdvancedCellBorderStyle.NotSet)
        {
        }

        internal DataGridViewAdvancedBorderStyle(DataGridView owner, DataGridViewAdvancedCellBorderStyle banned1, DataGridViewAdvancedCellBorderStyle banned2, DataGridViewAdvancedCellBorderStyle banned3)
        {
            this.all = true;
            this.top = DataGridViewAdvancedCellBorderStyle.None;
            this.left = DataGridViewAdvancedCellBorderStyle.None;
            this.right = DataGridViewAdvancedCellBorderStyle.None;
            this.bottom = DataGridViewAdvancedCellBorderStyle.None;
            this.owner = owner;
            this.banned1 = banned1;
            this.banned2 = banned2;
            this.banned3 = banned3;
        }

        public override bool Equals(object other)
        {
            DataGridViewAdvancedBorderStyle style = other as DataGridViewAdvancedBorderStyle;
            if (style == null)
            {
                return false;
            }
            return ((((style.all == this.all) && (style.top == this.top)) && ((style.left == this.left) && (style.bottom == this.bottom))) && (style.right == this.right));
        }

        public override int GetHashCode()
        {
            return WindowsFormsUtils.GetCombinedHashCodes(new int[] { this.top, this.left, this.bottom, this.right });
        }

        object ICloneable.Clone()
        {
            return new DataGridViewAdvancedBorderStyle(this.owner, this.banned1, this.banned2, this.banned3) { all = this.all, top = this.top, right = this.right, bottom = this.bottom, left = this.left };
        }

        public override string ToString()
        {
            return ("DataGridViewAdvancedBorderStyle { All=" + this.All.ToString() + ", Left=" + this.Left.ToString() + ", Right=" + this.Right.ToString() + ", Top=" + this.Top.ToString() + ", Bottom=" + this.Bottom.ToString() + " }");
        }

        public DataGridViewAdvancedCellBorderStyle All
        {
            get
            {
                if (!this.all)
                {
                    return DataGridViewAdvancedCellBorderStyle.NotSet;
                }
                return this.top;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 7))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(DataGridViewAdvancedCellBorderStyle));
                }
                if (((value == DataGridViewAdvancedCellBorderStyle.NotSet) || (value == this.banned1)) || ((value == this.banned2) || (value == this.banned3)))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_AdvancedCellBorderStyleInvalid", new object[] { "All" }));
                }
                if (!this.all || (this.top != value))
                {
                    this.all = true;
                    this.top = this.left = this.right = this.bottom = value;
                    if (this.owner != null)
                    {
                        this.owner.OnAdvancedBorderStyleChanged(this);
                    }
                }
            }
        }

        public DataGridViewAdvancedCellBorderStyle Bottom
        {
            get
            {
                if (this.all)
                {
                    return this.top;
                }
                return this.bottom;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 7))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(DataGridViewAdvancedCellBorderStyle));
                }
                if (value == DataGridViewAdvancedCellBorderStyle.NotSet)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_AdvancedCellBorderStyleInvalid", new object[] { "Bottom" }));
                }
                this.BottomInternal = value;
            }
        }

        internal DataGridViewAdvancedCellBorderStyle BottomInternal
        {
            set
            {
                if ((this.all && (this.top != value)) || (!this.all && (this.bottom != value)))
                {
                    if (this.all && (this.right == DataGridViewAdvancedCellBorderStyle.OutsetDouble))
                    {
                        this.right = DataGridViewAdvancedCellBorderStyle.Outset;
                    }
                    this.all = false;
                    this.bottom = value;
                    if (this.owner != null)
                    {
                        this.owner.OnAdvancedBorderStyleChanged(this);
                    }
                }
            }
        }

        public DataGridViewAdvancedCellBorderStyle Left
        {
            get
            {
                if (this.all)
                {
                    return this.top;
                }
                return this.left;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 7))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(DataGridViewAdvancedCellBorderStyle));
                }
                if (value == DataGridViewAdvancedCellBorderStyle.NotSet)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_AdvancedCellBorderStyleInvalid", new object[] { "Left" }));
                }
                this.LeftInternal = value;
            }
        }

        internal DataGridViewAdvancedCellBorderStyle LeftInternal
        {
            set
            {
                if ((this.all && (this.top != value)) || (!this.all && (this.left != value)))
                {
                    if (((this.owner != null) && this.owner.RightToLeftInternal) && ((value == DataGridViewAdvancedCellBorderStyle.InsetDouble) || (value == DataGridViewAdvancedCellBorderStyle.OutsetDouble)))
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_AdvancedCellBorderStyleInvalid", new object[] { "Left" }));
                    }
                    if (this.all)
                    {
                        if (this.right == DataGridViewAdvancedCellBorderStyle.OutsetDouble)
                        {
                            this.right = DataGridViewAdvancedCellBorderStyle.Outset;
                        }
                        if (this.bottom == DataGridViewAdvancedCellBorderStyle.OutsetDouble)
                        {
                            this.bottom = DataGridViewAdvancedCellBorderStyle.Outset;
                        }
                    }
                    this.all = false;
                    this.left = value;
                    if (this.owner != null)
                    {
                        this.owner.OnAdvancedBorderStyleChanged(this);
                    }
                }
            }
        }

        public DataGridViewAdvancedCellBorderStyle Right
        {
            get
            {
                if (this.all)
                {
                    return this.top;
                }
                return this.right;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 7))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(DataGridViewAdvancedCellBorderStyle));
                }
                if (value == DataGridViewAdvancedCellBorderStyle.NotSet)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_AdvancedCellBorderStyleInvalid", new object[] { "Right" }));
                }
                this.RightInternal = value;
            }
        }

        internal DataGridViewAdvancedCellBorderStyle RightInternal
        {
            set
            {
                if ((this.all && (this.top != value)) || (!this.all && (this.right != value)))
                {
                    if (((this.owner != null) && !this.owner.RightToLeftInternal) && ((value == DataGridViewAdvancedCellBorderStyle.InsetDouble) || (value == DataGridViewAdvancedCellBorderStyle.OutsetDouble)))
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_AdvancedCellBorderStyleInvalid", new object[] { "Right" }));
                    }
                    if (this.all && (this.bottom == DataGridViewAdvancedCellBorderStyle.OutsetDouble))
                    {
                        this.bottom = DataGridViewAdvancedCellBorderStyle.Outset;
                    }
                    this.all = false;
                    this.right = value;
                    if (this.owner != null)
                    {
                        this.owner.OnAdvancedBorderStyleChanged(this);
                    }
                }
            }
        }

        public DataGridViewAdvancedCellBorderStyle Top
        {
            get
            {
                return this.top;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 7))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(DataGridViewAdvancedCellBorderStyle));
                }
                if (value == DataGridViewAdvancedCellBorderStyle.NotSet)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_AdvancedCellBorderStyleInvalid", new object[] { "Top" }));
                }
                this.TopInternal = value;
            }
        }

        internal DataGridViewAdvancedCellBorderStyle TopInternal
        {
            set
            {
                if ((this.all && (this.top != value)) || (!this.all && (this.top != value)))
                {
                    if (this.all)
                    {
                        if (this.right == DataGridViewAdvancedCellBorderStyle.OutsetDouble)
                        {
                            this.right = DataGridViewAdvancedCellBorderStyle.Outset;
                        }
                        if (this.bottom == DataGridViewAdvancedCellBorderStyle.OutsetDouble)
                        {
                            this.bottom = DataGridViewAdvancedCellBorderStyle.Outset;
                        }
                    }
                    this.all = false;
                    this.top = value;
                    if (this.owner != null)
                    {
                        this.owner.OnAdvancedBorderStyleChanged(this);
                    }
                }
            }
        }
    }
}

