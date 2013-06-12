namespace System.Web.UI.HtmlControls
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [ConstructorNeedsTag(true)]
    public class HtmlGenericControl : HtmlContainerControl
    {
        public HtmlGenericControl() : this("span")
        {
        }

        public HtmlGenericControl(string tag)
        {
            if (tag == null)
            {
                tag = string.Empty;
            }
            base._tagName = tag;
        }

        [WebCategory("Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue("")]
        public string TagName
        {
            get
            {
                return base._tagName;
            }
            set
            {
                base._tagName = value;
            }
        }
    }
}

