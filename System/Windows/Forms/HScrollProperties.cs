namespace System.Windows.Forms
{
    using System;

    public class HScrollProperties : ScrollProperties
    {
        public HScrollProperties(ScrollableControl container) : base(container)
        {
        }

        internal override int HorizontalDisplayPosition
        {
            get
            {
                return -base.value;
            }
        }

        internal override int Orientation
        {
            get
            {
                return 0;
            }
        }

        internal override int PageSize
        {
            get
            {
                return base.ParentControl.ClientRectangle.Width;
            }
        }

        internal override int VerticalDisplayPosition
        {
            get
            {
                return base.ParentControl.DisplayRectangle.Y;
            }
        }
    }
}

