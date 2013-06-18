namespace System.Windows.Forms
{
    using System;

    public class ColumnStyle : TableLayoutStyle
    {
        public ColumnStyle()
        {
        }

        public ColumnStyle(SizeType sizeType)
        {
            base.SizeType = sizeType;
        }

        public ColumnStyle(SizeType sizeType, float width)
        {
            base.SizeType = sizeType;
            this.Width = width;
        }

        public float Width
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

