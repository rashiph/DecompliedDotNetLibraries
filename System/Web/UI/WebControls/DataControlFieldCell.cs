namespace System.Web.UI.WebControls
{
    using System;
    using System.Web.UI;

    public class DataControlFieldCell : TableCell
    {
        private DataControlField _containingField;

        public DataControlFieldCell(DataControlField containingField)
        {
            this._containingField = containingField;
        }

        protected DataControlFieldCell(HtmlTextWriterTag tagKey, DataControlField containingField) : base(tagKey)
        {
            this._containingField = containingField;
        }

        public DataControlField ContainingField
        {
            get
            {
                return this._containingField;
            }
        }
    }
}

