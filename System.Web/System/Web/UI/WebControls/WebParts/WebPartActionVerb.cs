namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.Web;

    internal abstract class WebPartActionVerb : WebPartVerb
    {
        protected WebPartActionVerb()
        {
        }

        [Browsable(false), Bindable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Checked
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("WebPartActionVerb_CantSetChecked"));
            }
        }
    }
}

