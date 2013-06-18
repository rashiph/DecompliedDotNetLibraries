namespace System.Web.UI.WebControls
{
    using System;
    using System.Web.UI;

    public class FormViewPagerRow : FormViewRow, INonBindingContainer, INamingContainer
    {
        public FormViewPagerRow(int rowIndex, DataControlRowType rowType, DataControlRowState rowState) : base(rowIndex, rowType, rowState)
        {
        }
    }
}

