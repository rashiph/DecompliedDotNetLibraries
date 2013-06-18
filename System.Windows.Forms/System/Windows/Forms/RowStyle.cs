namespace System.Windows.Forms
{
    using System;

    public class RowStyle : TableLayoutStyle
    {
        public RowStyle()
        {
        }

        public RowStyle(SizeType sizeType)
        {
            base.SizeType = sizeType;
        }

        public RowStyle(SizeType sizeType, float height)
        {
            base.SizeType = sizeType;
            this.Height = height;
        }

        public float Height
        {
            get
            {
                return base.Size;
            }
            set
            {
                base.Size = value;
            }
        }
    }
}

