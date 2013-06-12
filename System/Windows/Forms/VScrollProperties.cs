namespace System.Windows.Forms
{
    using System;

    public class VScrollProperties : ScrollProperties
    {
        public VScrollProperties(ScrollableControl container) : base(container)
        {
        }

        internal override int HorizontalDisplayPosition
        {
            get
            {
                return base.ParentControl.DisplayRectangle.X;
            }
        }

        internal override int Orientation
        {
            get
            {
                return 1;
            }
        }

        internal override int PageSize
        {
            get
            {
                return base.ParentControl.ClientRectangle.Height;
            }
        }

        internal override int VerticalDisplayPosition
        {
            get
            {
                return -base.value;
            }
        }
    }
}

