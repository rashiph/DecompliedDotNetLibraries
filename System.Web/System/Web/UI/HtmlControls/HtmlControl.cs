namespace System.Web.UI.HtmlControls
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.Web;
    using System.Web.UI;

    [ToolboxItem(false), Designer("System.Web.UI.Design.HtmlIntrinsicControlDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public abstract class HtmlControl : Control, IAttributeAccessor
    {
        private System.Web.UI.AttributeCollection _attributes;
        internal string _tagName;

        protected HtmlControl() : this("span")
        {
        }

        protected HtmlControl(string tag)
        {
            this._tagName = tag;
        }

        protected override ControlCollection CreateControlCollection()
        {
            return new EmptyControlCollection(this);
        }

        protected virtual string GetAttribute(string name)
        {
            return this.Attributes[name];
        }

        internal static string MapIntegerAttributeToString(int n)
        {
            if (n == -1)
            {
                return null;
            }
            return n.ToString(NumberFormatInfo.InvariantInfo);
        }

        internal static string MapStringAttributeToString(string s)
        {
            if ((s != null) && (s.Length == 0))
            {
                return null;
            }
            return s;
        }

        internal void PreProcessRelativeReferenceAttribute(HtmlTextWriter writer, string attribName)
        {
            string str = this.Attributes[attribName];
            if (!string.IsNullOrEmpty(str))
            {
                try
                {
                    str = base.ResolveClientUrl(str);
                }
                catch (Exception exception)
                {
                    throw new HttpException(System.Web.SR.GetString("Property_Had_Malformed_Url", new object[] { attribName, exception.Message }));
                }
                writer.WriteAttribute(attribName, str);
                this.Attributes.Remove(attribName);
            }
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            this.RenderBeginTag(writer);
        }

        protected virtual void RenderAttributes(HtmlTextWriter writer)
        {
            if (this.ID != null)
            {
                writer.WriteAttribute("id", this.ClientID);
            }
            this.Attributes.Render(writer);
        }

        protected virtual void RenderBeginTag(HtmlTextWriter writer)
        {
            writer.WriteBeginTag(this.TagName);
            this.RenderAttributes(writer);
            writer.Write('>');
        }

        protected virtual void SetAttribute(string name, string value)
        {
            this.Attributes[name] = value;
        }

        string IAttributeAccessor.GetAttribute(string name)
        {
            return this.GetAttribute(name);
        }

        void IAttributeAccessor.SetAttribute(string name, string value)
        {
            this.SetAttribute(name, value);
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Web.UI.AttributeCollection Attributes
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (this._attributes == null)
                {
                    this._attributes = new System.Web.UI.AttributeCollection(this.ViewState);
                }
                return this._attributes;
            }
        }

        [WebCategory("Behavior"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), TypeConverter(typeof(MinimizableAttributeTypeConverter)), DefaultValue(false)]
        public bool Disabled
        {
            get
            {
                string str = this.Attributes["disabled"];
                if (str == null)
                {
                    return false;
                }
                return str.Equals("disabled");
            }
            set
            {
                if (value)
                {
                    this.Attributes["disabled"] = "disabled";
                }
                else
                {
                    this.Attributes["disabled"] = null;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public CssStyleCollection Style
        {
            get
            {
                return this.Attributes.CssStyle;
            }
        }

        [DefaultValue(""), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebCategory("Appearance")]
        public virtual string TagName
        {
            get
            {
                return this._tagName;
            }
        }

        protected override bool ViewStateIgnoresCase
        {
            get
            {
                return true;
            }
        }
    }
}

