namespace System.Web.UI.Design
{
    using System;
    using System.Web.UI.WebControls;

    public class DesignerAutoFormatStyle : Style
    {
        private System.Web.UI.WebControls.VerticalAlign _verticalAlign;

        public System.Web.UI.WebControls.VerticalAlign VerticalAlign
        {
            get
            {
                return this._verticalAlign;
            }
            set
            {
                this._verticalAlign = value;
            }
        }
    }
}

