namespace System.Web.UI.WebControls
{
    using System;
    using System.Web.UI;

    public class DetailsViewPagerRow : DetailsViewRow, INonBindingContainer, INamingContainer
    {
        public DetailsViewPagerRow(int rowIndex, DataControlRowType rowType, DataControlRowState rowState) : base(rowIndex, rowType, rowState)
        {
        }
    }
}

