namespace System.Web.UI.HtmlControls
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [ControlBuilder(typeof(HtmlEmptyTagControlBuilder))]
    public abstract class HtmlInputControl : HtmlControl
    {
        private string _type;

        protected HtmlInputControl(string type) : base("input")
        {
            this._type = type;
            base.Attributes["type"] = type;
        }

        protected override void RenderAttributes(HtmlTextWriter writer)
        {
            writer.WriteAttribute("name", this.RenderedNameAttribute);
            base.Attributes.Remove("name");
            bool flag = false;
            string type = this.Type;
            if (!string.IsNullOrEmpty(type))
            {
                writer.WriteAttribute("type", type);
                base.Attributes.Remove("type");
                flag = true;
            }
            base.RenderAttributes(writer);
            if (flag && base.DesignMode)
            {
                base.Attributes.Add("type", type);
            }
            writer.Write(" /");
        }

        [DefaultValue(""), WebCategory("Behavior"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual string Name
        {
            get
            {
                return this.UniqueID;
            }
            set
            {
            }
        }

        internal virtual string RenderedNameAttribute
        {
            get
            {
                return this.Name;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(""), WebCategory("Behavior")]
        public string Type
        {
            get
            {
                string str = base.Attributes["type"];
                if (!string.IsNullOrEmpty(str))
                {
                    return str;
                }
                if (this._type == null)
                {
                    return string.Empty;
                }
                return this._type;
            }
        }

        [WebCategory("Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue("")]
        public virtual string Value
        {
            get
            {
                string str = base.Attributes["value"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                base.Attributes["value"] = HtmlControl.MapStringAttributeToString(value);
            }
        }
    }
}

