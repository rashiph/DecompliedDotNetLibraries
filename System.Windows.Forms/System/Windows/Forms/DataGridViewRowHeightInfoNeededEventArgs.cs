namespace System.Windows.Forms
{
    using System;
    using System.Globalization;

    public class DataGridViewRowHeightInfoNeededEventArgs : EventArgs
    {
        private int height = -1;
        private int minimumHeight = -1;
        private int rowIndex = -1;

        internal DataGridViewRowHeightInfoNeededEventArgs()
        {
        }

        internal void SetProperties(int rowIndex, int height, int minimumHeight)
        {
            this.rowIndex = rowIndex;
            this.height = height;
            this.minimumHeight = minimumHeight;
        }

        public int Height
        {
            get
            {
                return this.height;
            }
            set
            {
                if (value < this.minimumHeight)
                {
                    value = this.minimumHeight;
                }
                if (value > 0x10000)
                {
                    object[] args = new object[] { "Height", value.ToString(CultureInfo.CurrentCulture), 0x10000.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("Height", System.Windows.Forms.SR.GetString("InvalidHighBoundArgumentEx", args));
                }
                this.height = value;
            }
        }

        public int MinimumHeight
        {
            get
            {
                return this.minimumHeight;
            }
            set
            {
                if (value < 2)
                {
                    object[] args = new object[] { 2.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("MinimumHeight", value, System.Windows.Forms.SR.GetString("DataGridViewBand_MinimumHeightSmallerThanOne", args));
                }
                if (this.height < value)
                {
                    this.height = value;
                }
                this.minimumHeight = value;
            }
        }

        public int RowIndex
        {
            get
            {
                return this.rowIndex;
            }
        }
    }
}

